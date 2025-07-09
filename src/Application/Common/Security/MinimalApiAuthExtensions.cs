using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Enums;
using ProjectTemplate.Shared.Constants;


namespace ProjectTemplate.Application.Common.Security;
public static class MinimalApiAuthExtensions
{
    public static RouteHandlerBuilder RequiredPermission(
        this RouteHandlerBuilder builder,
        params EnumPermission[] enumPermissions)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // 1. Authenticated?
            if (user?.Identity is { IsAuthenticated: false })
                return Results.Unauthorized();
            
            var cacheService = httpContext.RequestServices.GetRequiredService<IAuthCacheService>();

            // 2. Is user blocked?
            if (!int.TryParse(user?.FindFirst(StaticClaims.UserId)?.Value, out int userId))
                return Results.Unauthorized();

            if (await cacheService.UserIsBlockedAsync(userId))
                return Results.Unauthorized();

            // 3. Permissions check
            if (enumPermissions is { Length: > 0 })
            {
                if (!int.TryParse(user?.FindFirst(StaticClaims.RoleId)?.Value, out int userRoleId))
                    return Results.Forbid();

                var permissions = await cacheService.GetPermissionsAsync(userRoleId);
                if (!permissions.Intersect(enumPermissions).Any())
                    return Results.Forbid();
            }

            return await next(context);
        });

        return builder;
    }

}

