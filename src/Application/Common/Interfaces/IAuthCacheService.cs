using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Application.Common.Interfaces;
public interface IAuthCacheService
{
    Task<List<EnumPermission>> GetPermissionsAsync(int roleId, CancellationToken ct = default);
    Task<bool> SetPermissionAsync(int roleId, EnumPermission[] permissions, CancellationToken ct = default);
    Task<bool> RemoveRoleAsync(int roleId, CancellationToken ct = default);
    Task<bool> SetUserIdToBlockedUsersToCacheAsync(int userId, CancellationToken ct = default);
    Task<bool> RemoveFromBlockedUsersCacheAsync(int userId, CancellationToken ct = default);

    Task<bool> UserIsBlockedAsync(int userId, CancellationToken ct = default);
}
