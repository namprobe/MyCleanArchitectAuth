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
        
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }
        else
        {
            throw new FileNotFoundException($"Environment file not found at {envPath}");
        }

        // Lấy connection string từ environment variable
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("CONNECTION_STRING environment variable is not set");
        }

        // Tạo DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString, 
            sqlOptions => 
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 