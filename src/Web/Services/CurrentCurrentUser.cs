using System.Security.Claims;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Shared.Constants;

namespace ProjectTemplate.Web.Services;

public class CurrentCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int? Id => GetIntClaim(StaticClaims.UserId);
    public int? RoleId => GetIntClaim(StaticClaims.RoleId);

    private int? GetIntClaim(string claimType)
    {
        var claimValue = httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
        return int.TryParse(claimValue, out var result) ? result : null;
    }
}
