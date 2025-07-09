using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Models;

namespace ProjectTemplate.Application.Users.Auth.Commands;
public record LoginCommand(string UserName, string Password, string? DeviceId) : IRequest<TokenResponseModel>;
public class LoginCommandHandler(ITokenService tokenService) : IRequestHandler<LoginCommand, TokenResponseModel>
{
    public async Task<TokenResponseModel> Handle(LoginCommand request, CancellationToken cancellationToken)
    => await tokenService.GenerateTokenAsync(request, cancellationToken);
    
}


