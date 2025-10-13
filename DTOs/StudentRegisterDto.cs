using System;
using System.ComponentModel.DataAnnotations;

namespace HETHONGHOCPHAN.DTOs
{
    public class StudentRegisterDto
    {
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        [StringLength(10, ErrorMessage = "Mã sinh viên không vượt quá 10 ký tự.")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Khóa học (Năm) là bắt buộc.")]
        public string CourseYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
        public string ClassName { get; set; }

        // 🆕 Thêm Email
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; }
    }
}
