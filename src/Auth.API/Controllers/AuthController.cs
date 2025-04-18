using Auth.Application.Common.DTOs.Auth;
using Auth.Application.Common.Interfaces;
using Auth.Application.Features.Auth.Commands.Login;
using Auth.Application.Features.Auth.Commands.RefreshToken;
using Auth.Application.Features.Auth.Commands.Register;
using Auth.Application.Features.Auth.Commands.RevokeToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using MyCleanArchitectAuth.Application.Features.Auth.Queries.CheckSession;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILoggerService _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;


        public AuthController(IMediator mediator, ILoggerService logger, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                //Gọi RegisterCommand để tạo user
                var command = new RegisterCommand(registerDto);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { Errors = result.Errors });
                }

                //Lấy thông tin user để gửi email
                var user = await _unitOfWork.Users.GetUserByIdAsync(result.Data);

                //Gửi email xác thực
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendVerificationEmailAsync(
                            user?.Email, $"{user?.FirstName} {user?.LastName}", user?.VerificationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send verification email to {Email}", user?.Email);
                    }
                });

                return Ok(new
                {
                    Message = "Registration successful. Please check your email to verify your account.",
                    UserId = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new{Errors = "An error occurred while registration."});
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var command = new LoginCommand(loginDto);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new {Errors = "An error occurred while login."});
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            try
            {
                _logger.LogInformation("Received verification token: {Token}", verifyEmailDto.Token); // Debug log

                var user = await _unitOfWork.Users.GetByVerificationTokenAsync(verifyEmailDto.Token);

                if (user == null)
                {
                    _logger.LogWarning("Invalid verification token: {Token}", verifyEmailDto.Token);
                    return BadRequest(new { Errors = "Invalid verification token." });
                }
                
                // Kiểm tra xem email đã được xác thực chưa
                if (user.EmailConfirmed)
                {
                    return BadRequest(new { Errors = "Email already verified." });
                }

                user.EmailConfirmed = true;
                user.VerificationToken = null;
                await _unitOfWork.SaveChangesAsync();

                //Gửi email chào mừng
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email, $"{user.FirstName} {user.LastName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                    }
                });
                
                return Ok(new { Message = "Email verified successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(500, new { Errors = "An error occurred while verifying email." });
            }
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.LogInformation("Refreshing token for device: {DeviceId}", refreshTokenDto.DeviceId);
                
                // Validate request
                if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken) || 
                    string.IsNullOrEmpty(refreshTokenDto.DeviceId))
                {
                    return BadRequest(new { Errors = "Invalid refresh token request" });
                }

                var command = new RefreshTokenCommand(refreshTokenDto.RefreshToken, refreshTokenDto.DeviceId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Token refresh failed: {Errors}", string.Join(", ", result.Errors));
                    return BadRequest(new { Errors = result.Errors });
                }

                _logger.LogInformation("Token refreshed successfully for device: {DeviceId}", refreshTokenDto.DeviceId);
                return Ok(new { data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh for device: {DeviceId}", refreshTokenDto.DeviceId);
                return StatusCode(500, new { Errors = "An error occurred while refreshing token." });
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
        {
            try
            {
                _logger.LogInformation("=== Starting revoke token process ===");
                
                // Log all headers
                foreach (var header in Request.Headers)
                {
                    _logger.LogInformation("Header {Key}: {Value}", header.Key, header.Value);
                }
                
                // Log authorization header specifically
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogInformation("Authorization header: {Auth}", authHeader);
                
                // Log user identity information
                _logger.LogInformation("IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
                _logger.LogInformation("AuthenticationType: {AuthType}", User.Identity?.AuthenticationType);
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("User ID from claims: {UserId}", userId);
                
                // Log all claims
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim {Type}: {Value}", claim.Type, claim.Value);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid token - no user ID found");
                    return Unauthorized(new { Errors = "Invalid token" });
                }

                var command = new RevokeTokenCommand(
                    revokeTokenDto.RefreshToken,
                    revokeTokenDto.DeviceId,
                    userId
                );

                _logger.LogInformation("Sending command: {@Command}", command);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to revoke token: {@Errors}", result.Errors);
                    return BadRequest(new { Errors = result.Errors });
                }

                _logger.LogInformation("Token revoked successfully");
                return Ok(new { Message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(500, new { Errors = "An error occurred while revoking token." });
            }
        }

        [Authorize]
        [HttpGet("check-session")]
        public async Task<IActionResult> CheckSession([FromQuery] string deviceId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Errors = "Invalid token" });
                }

                var query = new CheckSessionQuery(userId, deviceId);
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return Unauthorized(new { Errors = result.Errors });
                }

                return Ok(new { Message = "Session is valid" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session status");
                return StatusCode(500, new { Errors = "An error occurred while checking session." });
            }
        }
    }
}