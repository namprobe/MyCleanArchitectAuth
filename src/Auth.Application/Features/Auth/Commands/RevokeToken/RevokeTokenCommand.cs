using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken, string DeviceId, string UserId) : IRequest<Result<bool>>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RevokeTokenCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Lấy session từ refresh token
            var session = await _unitOfWork.UserSessions.GetByRefreshTokenAsync(request.RefreshToken);
            if (session == null)
            {
                return Result<bool>.Failure(new[] { "Invalid refresh token" });
            }

            // Verify device id
            if (session.DeviceId != request.DeviceId)
            {
                return Result<bool>.Failure(new[] { "Invalid device id" });
            }

            // Verify user id
            if (session.UserId != request.UserId)
            {
                return Result<bool>.Failure(new[] { "Unauthorized to revoke this token" });
            }

            // Revoke session
            session.IsRevoked = true;
            session.RevokedOn = DateTime.UtcNow;
            await _unitOfWork.UserSessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        });
    }
} 