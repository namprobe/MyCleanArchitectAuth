namespace Auth.Application.Common.DTOs.Auth;

public class RevokeTokenDto
{
    public string RefreshToken { get; set; }
    public string DeviceId { get; set; } = "swagger-device"; // Default for testing
} 