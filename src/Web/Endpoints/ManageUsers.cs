using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Application.Common.QueryFilter;
using ProjectTemplate.Application.Common.Security;
using ProjectTemplate.Application.Users.Manage.Commands;
using ProjectTemplate.Application.Users.Manage.Queries;
using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Web.Endpoints;

public class ManageUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(GetUsers).RequiredPermission(EnumPermission.GetUser);
        group.MapPost(CreateUser).RequiredPermission(EnumPermission.CreateUser);
        group.MapDelete(RemoveUser).RequiredPermission(EnumPermission.DeleteUser);

    }

    
    public async Task<Ok<PageList<UserDto>>> GetUsers(ISender sender, [FromBody] GetUserFilterQuery filter, CancellationToken ct)
    {
        var res = await sender.Send(filter, ct);

        return TypedResults.Ok(res);
    }

    public async Task<Results<Ok, BadRequest>> CreateUser(ISender sender, [FromBody] CreateUserCommand createUserCommand, CancellationToken ct)
    {
        var res = await sender.Send(createUserCommand, ct);

        return res ? TypedResults.Ok() : TypedResults.BadRequest();

    }
    public async Task<Results<Ok, BadRequest>> RemoveUser(ISender sender, [FromBody] RemoveUserCommand removeUser, CancellationToken ct)
    {
        var resultIsSuccess = await sender.Send(removeUser, ct);

        return resultIsSuccess ? TypedResults.Ok() : TypedResults.BadRequest();

    }


}
