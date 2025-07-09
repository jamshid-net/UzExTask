using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Application.Users.Auth.Commands;

namespace ProjectTemplate.Web.Endpoints;

public class AuthUser : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(Login);
        group.MapPost(RefreshToken);
        group.MapDelete(Logout);
    }

    public async Task<Ok<TokenResponseModel>> Login(ISender sender, [FromBody] LoginCommand command, CancellationToken ct)
    {
        var res = await sender.Send(command, ct);
        return TypedResults.Ok(res);
    }

    public async Task<Ok<TokenResponseModel>> RefreshToken(ISender sender, [FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var res = await sender.Send(command, ct);
        return TypedResults.Ok(res);
    }
    public async Task<Ok> Logout(ISender sender, [FromBody] LogoutCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return TypedResults.Ok();
    }
}
