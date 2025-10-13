using System.ComponentModel.DataAnnotations;
using HETHONGHOCPHAN.Models;
using System.ComponentModel.DataAnnotations.Schema; // Thêm nếu cần cho các thuộc tính đặc biệt

// Model đã được thống nhất tên trường với AuthController
public class PasswordReset
{
    [Key]
    public int Id { get; set; }

    // Thêm StudentId để dễ dàng truy vấn và đảm bảo tính Required
    [Required]
    public string StudentId { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    // Trường này khớp với tên bạn dùng trong AuthController
    [Required]
    public string OTP { get; set; } = string.Empty;

    // Trường này khớp với tên bạn dùng trong AuthController
    [Required]
    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; } = false;
}
