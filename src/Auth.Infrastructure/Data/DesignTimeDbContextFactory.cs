using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace Auth.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Load .env file từ thư mục gốc của solution
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Auth.API", ".env");
        Env.Load(envPath);

        // Lấy connection string từ environment variable
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        // Tạo DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 