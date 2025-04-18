namespace Auth.Application.Common.DTOs.Auth;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }  // Optional vì có thể fail khi lấy IP
    public string? UserAgent { get; set; }  // Browser User Agent
} 