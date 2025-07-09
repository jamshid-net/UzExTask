using ProjectTemplate.Application.Common.Interfaces;

namespace ProjectTemplate.Application.Users.Auth.Commands;
public record LogoutCommand(string RefreshToken) : IRequest<bool>; 
public class LogoutCommandHandler(ITokenService tokenService) : IRequestHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            throw new ArgumentNullException(nameof(request.RefreshToken), "Refresh token cannot be null or empty.");
        }
        return await tokenService.DeleteRefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}
