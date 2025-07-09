using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Models;

namespace ProjectTemplate.Application.Users.Auth.Commands;
public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponseModel>;
public class RefreshTokenCommandHandler(ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, TokenResponseModel>
{
    public async Task<TokenResponseModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await tokenService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return token;
    }
}
