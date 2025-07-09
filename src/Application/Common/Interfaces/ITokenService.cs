using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Application.Users.Auth.Commands;

namespace ProjectTemplate.Application.Common.Interfaces;
public interface ITokenService
{
    Task<TokenResponseModel> GenerateTokenAsync(LoginCommand command, CancellationToken ct = default);
    Task<TokenResponseModel> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> DeleteRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
