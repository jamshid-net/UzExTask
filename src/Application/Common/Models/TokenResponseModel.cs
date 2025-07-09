namespace ProjectTemplate.Application.Common.Models;
public record TokenResponseModel(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiration,
    DateTimeOffset RefreshTokenExpiration
);
