using BCrypt.Net;
namespace HETHONGHOCPHAN.Utils
{
    public static class PasswordHasher
    {
        // Hàm để mã hóa (hash) mật khẩu
        public static string HashPassword(string password)
        {
            // BCrypt sẽ tự động tạo salt và hash mật khẩu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Hàm để xác minh mật khẩu thô với chuỗi đã hash trong database
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // So sánh mật khẩu người dùng nhập với chuỗi hash
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

