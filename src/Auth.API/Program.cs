using Auth.Application;
using Auth.Infrastructure;
using DotNetEnv;

namespace Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load .env file
            Env.Load();
            
            // Override settings from environment variables
            builder.Configuration["ConnectionStrings:DefaultConnection"] = 
                Environment.GetEnvironmentVariable("CONNECTION_STRING");
                
            builder.Configuration["JwtSettings:Secret"] = 
                Environment.GetEnvironmentVariable("JWT_SECRET");
            builder.Configuration["JwtSettings:Issuer"] = 
                Environment.GetEnvironmentVariable("JWT_ISSUER");
            builder.Configuration["JwtSettings:Audience"] = 
                Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            builder.Configuration["JwtSettings:ExpiresInMinutes"] = 
                Environment.GetEnvironmentVariable("JWT_EXPIRES_IN_MINUTES");

            // Add services to the container.
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
