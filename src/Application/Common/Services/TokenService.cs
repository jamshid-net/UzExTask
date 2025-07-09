using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Application.Users.Auth.Commands;
using ProjectTemplate.Domain.Entities.Auth;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Shared.Constants;
using ProjectTemplate.Shared.Extensions;
using ProjectTemplate.Shared.Helpers;

namespace ProjectTemplate.Application.Common.Services;
public class TokenService(IApplicationDbContext dbContext) : ITokenService
{
    public async Task<TokenResponseModel> GenerateTokenAsync(LoginCommand command, CancellationToken ct = default)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command), "Login command cannot be null.");
        }
        var foundUser = await dbContext.Users.SingleOrDefaultAsync(x => x.UserName == command.UserName, ct);
        
        if (foundUser is null)
            throw new NotFoundException(command.UserName, nameof(User));

        if (!foundUser.IsActive)
        {
            throw new ErrorFromClientException("User is not active. Please contact support.");
        }

        var comingHashedPassword = CryptoPassword.GetHashSalted(command.Password, foundUser.PasswordSalt);

        if (foundUser.PasswordHash != comingHashedPassword)
        {
            foundUser.FailedLoginAttempts++;
            dbContext.Users.Update(foundUser);
            await dbContext.SaveChangesAsync(ct);
            throw new ErrorFromClientException("Invalid username or password.");
        }

        var refreshToken = Guid.NewGuid().ToLowerString();

        var userToken = new UserRefreshToken
        {
            UserId = foundUser.Id,
            DeviceId = command.DeviceId,
            RefreshToken = refreshToken,
            UpdateDate = DateTimeOffset.UtcNow
        };
        await InsertToken(userToken, ct);

        return  GetJwt(userToken, refreshToken);
        
    }

    public async Task<TokenResponseModel> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty.");

        var userToken = await FindTokenAsync(refreshToken, ct);

        if (userToken is null)
            throw new NotFoundException(refreshToken, nameof(UserRefreshToken));

        if(userToken.User is
        {
            IsActive: false
        })
        {
            throw new ErrorFromClientException("User is not active. Please contact support.");
        }
            
        var min = DateTimeOffset.Now.Subtract(userToken.UpdateDate).TotalMinutes;

        if (min > AuthOptions.ExpireMinutesRefresh)
            throw new RefreshTokenExpiredException("Refresh token has expired");


        var newRefreshToken = Guid.NewGuid().ToLowerString();

        //expire the old refresh_token and add a new refresh_token
        userToken.RefreshToken = newRefreshToken;
        userToken.UpdateDate = DateTimeOffset.UtcNow;


        var data = GetJwt(userToken, newRefreshToken);
        dbContext.UserRefreshTokens.Update(userToken);
        await dbContext.SaveChangesAsync(ct);
        return data;
    }

    public async Task<bool> DeleteRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
       return await dbContext.UserRefreshTokens
                       .Where(x => x.RefreshToken == refreshToken)
                       .ExecuteDeleteAsync(cancellationToken) > 0; 
    }


    private TokenResponseModel GetJwt(UserRefreshToken userToken, string refreshToken)
    {
        var utcNow = DateTime.UtcNow;
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToLowerString()),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            new (StaticClaims.DeviceId, userToken.DeviceId ?? string.Empty),
            new (StaticClaims.UserId, userToken.UserId.ToString()),
            new (StaticClaims.RoleId, userToken.User?.RoleId.ToString() ?? "0"),
        ];


        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: utcNow.Add(TimeSpan.FromMinutes(AuthOptions.ExpireMinutes)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new TokenResponseModel(encodedJwt, 
                                                refreshToken, 
                            utcNow.Add(TimeSpan.FromMinutes(AuthOptions.ExpireMinutes)), 
                            utcNow.Add(TimeSpan.FromMinutes(AuthOptions.ExpireMinutesRefresh)));
    }

    private async Task<UserRefreshToken> InsertToken(UserRefreshToken userToken, CancellationToken ct = default)
    {
        var userTokens = await dbContext.UserRefreshTokens.Where(x => x.UserId == userToken.UserId)
                                                           .ToListAsync(ct);
     
        var idsToKeep = userTokens
            .OrderByDescending(x => x.Id)
            .Take(AuthOptions.MaxDeviceCount)
            .Select(x => x.Id)
            .ToList();

        await dbContext.UserRefreshTokens
            .Where(x => x.UserId == userToken.UserId && !idsToKeep.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        await dbContext.UserRefreshTokens.AddAsync(userToken, ct);

        await dbContext.SaveChangesAsync(ct);

        return userToken;
    }

    private Task<UserRefreshToken?> FindTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return dbContext.UserRefreshTokens
                         .Include(x => x.User)
                         .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, ct);
    }
}
