namespace Auth.Application.Common.DTOs.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }  // Thường 15-60 phút
    public DateTime RefreshTokenExpiresAt { get; set; } // Thường 7-30 ngày
    public bool IsEmailVerified { get; set; }  // Thêm trường này
} 