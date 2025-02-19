using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Auth.API.Configurations;

public static class JwtConfiguration
{
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSecret = configuration["JwtSettings:Secret"];
            var key = Encoding.ASCII.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT Secret not configured"));

            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception}");
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    // Prevent default response
                    context.HandleResponse();

                    // Custom unauthorized response
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    
                    var response = new 
                    { 
                        status = 401,
                        message = "Unauthorized access",
                        errors = new[] { "You are not authorized to access this resource" }
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            };
        });

        return services;
    }
} 