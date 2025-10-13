using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HETHONGHOCPHAN.Models
{
    public class Student
    {
        [Key]
        public string StudentId { get; set; } = string.Empty;
        public string Fullname { get; set; }
        public string Gender { get; set; }

        [Column("YearOfBirth")]
        public int YearOfBirth { get; set; }

        [Column("Course")]
        public string CourseYear { get; set; } = string.Empty;

        public string ClassName { get; set; }

        [Column("HashedPassword")]
        public string PasswordHash { get; set; }

        // 🆕 Thêm cột Email
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
