namespace Auth.Application.Common.DTOs.Auth;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Mặc định cho testing
    public string DeviceId { get; set; } = "swagger-device";
    public string DeviceName { get; set; } = "Swagger Test Device";
} 