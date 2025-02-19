using Auth.Application.Common.Interfaces;
using Auth.Domain.Enums;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auth.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILoggerService _logger;

    public EmailService(
        IEmailTemplateService templateService,
        IConfiguration configuration,
        ILoggerService logger)
    {
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var clientId = _configuration["Gmail:ClientId"];
            var clientSecret = _configuration["Gmail:ClientSecret"];
            var refreshToken = _configuration["Gmail:RefreshToken"];
            var redirectUri = _configuration["Gmail:RedirectUri"];

            // Validate configuration
            if (string.IsNullOrEmpty(clientId) || 
                string.IsNullOrEmpty(clientSecret) || 
                string.IsNullOrEmpty(refreshToken) ||
                string.IsNullOrEmpty(redirectUri))
            {
                var ex = new InvalidOperationException("Gmail OAuth configuration is missing");
                _logger.LogError(ex, "Missing OAuth configuration");
                throw ex;
            }

            using var client = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" },
                { "redirect_uri", redirectUri }
            };

            _logger.LogInformation("Requesting new access token with client_id: {ClientId}", clientId);

            var response = await client.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest)
            );

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Token response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var ex = new Exception($"Failed to refresh token: {responseContent}");
                _logger.LogError(ex, "Failed to refresh token. Status: {Status}", response.StatusCode);
                throw ex;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>(options);
            if (result?.AccessToken == null)
            {
                var ex = new Exception("Access token not found in response");
                _logger.LogError(ex, "Access token missing from response");
                throw ex;
            }

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh access token");
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(string email, string name, string token)
    {
        try
        {
            var template = await _templateService.GetTemplateAsync(EmailTemplateType.EmailConfirmation);
            var frontendUrl = _configuration["FrontendUrl"];
            var encodedToken = Uri.EscapeDataString(token);
            var verificationLink = $"{frontendUrl}/verify-email?token={encodedToken}";

            var parameters = new Dictionary<string, string>
            {
                { "UserName", name },
                { "ConfirmationLink", verificationLink }
            };

            var body = _templateService.ReplaceParameters(template, parameters);
            await SendEmailAsync(email, "Xác nhận email của bạn", body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient();
        
        try
        {
            // Kết nối tới Gmail SMTP server
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

            // Lấy access token mới
            var accessToken = await GetAccessTokenAsync();

            // Xác thực với OAuth2
            var oauth2 = new SaslMechanismOAuth2(
                _configuration["Gmail:UserEmail"],
                accessToken
            );
            await client.AuthenticateAsync(oauth2);

            // Tạo email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MyCleanArchitectAuth", _configuration["Gmail:UserEmail"]));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            // Gửi email
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    public Task SendPasswordChangedEmailAsync(string email, string name)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetEmailAsync(string email, string name, string token)
    {
        throw new NotImplementedException();
    }

    public Task SendWelcomeEmailAsync(string email, string name)
    {
        throw new NotImplementedException();
    }
}

