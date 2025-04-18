using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using System.Linq;

namespace MyCleanArchitectAuth.Application.Features.Auth.Queries.CheckSession
{
    public record CheckSessionQuery(string UserId, string DeviceId) : IRequest<Result<bool>>;

    public class CheckSessionQueryHandler : IRequestHandler<CheckSessionQuery, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckSessionQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(CheckSessionQuery request, CancellationToken cancellationToken)
        {
            // Lấy session mới nhất của device này
            var session = await _unitOfWork.UserSessions
                .GetLatestSessionByDeviceIdAsync(request.UserId, request.DeviceId);
            
            if (session == null)
            {
                return Result<bool>.Failure(new[] { "Session not found" });
            }

            // Kiểm tra session có bị revoke không
            if (session.IsRevoked)
            {
                return Result<bool>.Failure(new[] { "Session revoked" });
            }

            // Kiểm tra session có hết hạn không
            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                session.IsRevoked = true;
                session.RevokedOn = DateTime.UtcNow;
                await _unitOfWork.UserSessions.UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Cập nhật lại số lượng session active
                var activeSessions = (await _unitOfWork.UserSessions
                    .GetActiveSessionsAsync(request.UserId))
                    .Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                    .Count();
                    
                return Result<bool>.Failure(new[] { "Session expired" });
            }

            // Cập nhật LastActivity
            session.LastActivity = DateTime.UtcNow;
            await _unitOfWork.UserSessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
} 