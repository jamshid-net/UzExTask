using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Application.Common.Services;
public class CustomIdentityService(IApplicationDbContext dbContext) : ICustomIdentityService
{
    public async Task<bool> HasPermissionAsync(int? userId, EnumPermission permission, CancellationToken ct = default)
    {
        if (userId is null)
            return false;

        var hasPermission = await dbContext
                                     .Users
                                     .Where(u => u.Id == userId)
                                     .AsNoTracking()
                                     .AnyAsync(u => u.Role != null && u.Role.Permissions.Any(p => p.EnumPermission == permission), ct);

        return hasPermission;
    }

    public async Task<bool> HasPermissionAsync(int? userId, IReadOnlyCollection<EnumPermission>? permissions, CancellationToken ct = default)
    {
        if(userId is null || permissions is null || !permissions.Any())
            return false;

        var hasPermission = await dbContext
                                      .Users
                                      .Where(u => u.Id == userId)
                                      .AsNoTracking()
                                      .AnyAsync(u => u.Role != null && permissions.All(p => u.Role.Permissions.Any(x => x.EnumPermission == p)), ct);
        return hasPermission;
    }

    public async Task<string?> GetUserNameAsync(int? userId, CancellationToken ct = default)
    {
        if (userId is null)
            return null;

        return await dbContext
                                  .Users
                                  .AsNoTracking()
                                  .Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefaultAsync(ct);
                    
    }
}

