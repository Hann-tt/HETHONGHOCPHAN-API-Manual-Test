using HETHONGHOCPHAN.Models;
using HETHONGHOCPHAN.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // Bắt buộc cho IConfigurationRoot và ConfigurationBuilder
using System.IO; // Bắt buộc cho Directory.GetCurrentDirectory()
using Microsoft.Extensions.Configuration.Json; // Bắt buộc cho AddJsonFile()

namespace HETHONGHOCPHAN
{
    // Class này chỉ để hướng dẫn công cụ Migration cách khởi tạo DbContext
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 1. Lấy thông tin cấu hình (Connection String) từ appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // 2. Lấy Connection String
            var connectionString = configuration.GetConnectionString("DULIEUHOCPHAN");

            // 3. Tạo DbContextOptionsBuilder
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlServer(connectionString);

            // 4. Khởi tạo và trả về DbContext
            return new ApplicationDbContext(builder.Options);
        }
    }
}
