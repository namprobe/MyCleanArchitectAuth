namespace Auth.API.Configurations;

public static class EnvironmentConfiguration
{
    public static WebApplicationBuilder AddEnvironmentConfiguration(this WebApplicationBuilder builder)
    {
        // Load .env file
        DotNetEnv.Env.Load();
            
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
        builder.Configuration["JwtSettings:RefreshTokenExpiresInDays"] = 
            Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRES_IN_DAYS");

        // Add Gmail OAuth2 settings
        builder.Configuration["Gmail:UserEmail"] = 
            Environment.GetEnvironmentVariable("GMAIL_USER_EMAIL");
        builder.Configuration["Gmail:ClientId"] = 
            Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID");
        builder.Configuration["Gmail:ClientSecret"] = 
            Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET");
        builder.Configuration["Gmail:RefreshToken"] = 
            Environment.GetEnvironmentVariable("GMAIL_REFRESH_TOKEN");
        builder.Configuration["Gmail:AccessToken"] = 
            Environment.GetEnvironmentVariable("GMAIL_ACCESS_TOKEN");
        builder.Configuration["Gmail:RedirectUri"] = 
            Environment.GetEnvironmentVariable("REDIRECT_URI");

        // Add Frontend URL
        builder.Configuration["FrontendUrl"] = 
            Environment.GetEnvironmentVariable("FRONTEND_URL");

        return builder;
    }
} 