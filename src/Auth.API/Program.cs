using Auth.Application;
using Auth.Infrastructure;
using Auth.API.Configurations;
using Microsoft.AspNetCore.RateLimiting;

namespace Auth.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add configurations
        builder.AddEnvironmentConfiguration();

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddCorsConfiguration(builder.Configuration);
        builder.Services.AddJwtConfiguration(builder.Configuration);
        builder.Services.AddSwaggerConfiguration();
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("refresh-token", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5; // 5 requests per minute
                opt.QueueLimit = 0;
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseRateLimiter();

        app.Run();
    }
}
