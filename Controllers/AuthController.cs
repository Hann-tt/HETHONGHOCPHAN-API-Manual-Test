using HETHONGHOCPHAN.Data;
using HETHONGHOCPHAN.DTOs;
using HETHONGHOCPHAN.Models;
using HETHONGHOCPHAN.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace HETHONGHOCPHAN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ========== ĐĂNG KÝ ==========
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            if (await _context.Students.AnyAsync(s => s.StudentId == dto.StudentId))
            {
                return Conflict(new { success = false, message = "Mã sinh viên đã tồn tại." });
            }

            var hashedPassword = PasswordHasher.HashPassword(dto.Password);

            var newStudent = new Student
            {
                StudentId = dto.StudentId,
                PasswordHash = hashedPassword,
                Fullname = dto.Fullname,
                Gender = dto.Gender,
                YearOfBirth = dto.DateOfBirth.Year,
                CourseYear = dto.CourseYear,
                ClassName = dto.ClassName,
                Email = dto.Email
            };

            await _context.Students.AddAsync(newStudent);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đăng ký thành công!" });
        }

        // ========== ĐĂNG NHẬP ==========
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StudentLoginDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (student == null)
                return Unauthorized(new { success = false, message = "Mã sinh viên hoặc mật khẩu không chính xác." });

            if (!PasswordHasher.VerifyPassword(dto.Password, student.PasswordHash))
                return Unauthorized(new { success = false, message = "Mã sinh viên hoặc mật khẩu không chính xác." });

            var token = CreateToken(student);
            return Ok(new { success = true, token });
        }

        // ========== QUÊN MẬT KHẨU (Bước 1: Gửi OTP) ==========
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.StudentId))
                return BadRequest(new { success = false, message = "MSSV là bắt buộc." });

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (student == null)
                return NotFound(new { success = false, message = "Không tìm thấy MSSV trong hệ thống." });

            if (string.IsNullOrWhiteSpace(student.Email))
                return BadRequest(new { success = false, message = "Tài khoản này chưa có email để khôi phục." });

            var otp = new Random().Next(100000, 999999).ToString();

            // Sửa lỗi #1: Thêm StudentId vào record
            var resetRecord = new PasswordReset
            {
                StudentId = student.StudentId, // Đã thêm StudentId
                Email = student.Email,
                OTP = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await _context.PasswordResets.AddAsync(resetRecord);
            await _context.SaveChangesAsync();

            var emailSent = await SendOtpEmailAsync(student.Email, otp);
            if (!emailSent)
            {
                return StatusCode(500, new { success = false, message = "Không gửi được email OTP. Vui lòng kiểm tra cấu hình SMTP." });
            }

            // Sửa lỗi #2: Trả về Email để Frontend hiển thị và sử dụng
            return Ok(new
            {
                success = true,
                message = "Mã OTP đã được gửi tới email đăng ký.",
                email = student.Email // Đã thêm email
            });
        }

        // ========== ĐẶT LẠI MẬT KHẨU (Bước 2 & 3: Xác thực OTP và Đặt mật khẩu mới) ==========
        // Frontend gọi endpoint này với 3 trường: StudentId, Otp, NewPassword
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.StudentId) ||
                string.IsNullOrWhiteSpace(dto.Otp) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { success = false, message = "Thiếu thông tin bắt buộc." });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (student == null)
                return NotFound(new { success = false, message = "Không tìm thấy MSSV trong hệ thống." });

            // Sửa lỗi #3: Dùng StudentId để tìm OTP record, đảm bảo chính xác tuyệt đối
            var otpRecord = await _context.PasswordResets
                .Where(p => p.StudentId == dto.StudentId // Dùng StudentId thay vì Email
                         && p.OTP == dto.Otp
                         && !p.IsUsed
                         && p.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(p => p.ExpiryTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                return BadRequest(new { success = false, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });

            // Cập nhật mật khẩu và đánh dấu OTP đã sử dụng
            student.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            otpRecord.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đặt lại mật khẩu thành công." });
        }

        // ========== GỬI EMAIL OTP ==========
        private async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"];
                var smtpPort = int.Parse(_configuration["Smtp:Port"]);
                var smtpUser = _configuration["Smtp:Username"];
                var smtpPass = _configuration["Smtp:Password"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(smtpUser, "Hệ thống học phần"),
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body = $"Mã OTP của bạn là: {otp}\n\nMã này sẽ hết hạn sau 5 phút.",
                    IsBodyHtml = false
                };

                mail.To.Add(toEmail);
                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP ERROR] {ex.Message}");
                return false;
            }
        }

        // ========== TẠO TOKEN ==========
        private string CreateToken(Student student)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var keyString = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, student.StudentId),
                new Claim(JwtRegisteredClaimNames.Name, student.Fullname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
