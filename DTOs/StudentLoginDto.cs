// Namespace cho các đối tượng truyền dữ liệu (Data Transfer Objects)
using System.ComponentModel.DataAnnotations;

namespace HETHONGHOCPHAN.DTOs
{
    // Đây là đối tượng được gửi từ Frontend khi sinh viên đăng nhập
    public class StudentLoginDto
    {
        // StudentId là bắt buộc
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        public string StudentId { get; set; } = string.Empty;

        // Password là bắt buộc và phải có độ dài tối thiểu 6 ký tự
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;
    }
}
