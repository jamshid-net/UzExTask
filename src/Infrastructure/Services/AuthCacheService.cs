using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Enums;
using ProjectTemplate.Shared.Constants;
using Serilog;

namespace ProjectTemplate.Infrastructure.Services;
public class AuthCacheService(IDistributedCache cache) : IAuthCacheService
{
    
    public async Task<List<EnumPermission>> GetPermissionsAsync(int roleId, CancellationToken ct = default)
    {
        if (roleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(roleId), "Role ID must be greater than zero.");
       
        var values = await cache.GetStringAsync($"{CacheKeyPrefixes.Role}:{roleId}", ct);

        if(string.IsNullOrEmpty(values))
            return [];

        return JsonSerializer.Deserialize<List<EnumPermission>>(values) ?? [];

    }

    public async Task<bool> SetPermissionAsync(int roleId, EnumPermission[] permissions, CancellationToken ct = default)
    {
        if(roleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(roleId), "Role ID must be greater than zero.");

        try
        {
            if (permissions is null or [])
                await cache.SetStringAsync($"{CacheKeyPrefixes.Role}:{roleId}", string.Empty, ct);

            string json = JsonSerializer.Serialize(permissions);
            await cache.SetStringAsync($"{CacheKeyPrefixes.Role}:{roleId}", json, ct);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error setting permissions for role ID {RoleId}", roleId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(int roleId, CancellationToken ct = default)
    {
        try
        {
           await cache.RemoveAsync($"{CacheKeyPrefixes.Role}:{roleId}", ct);
           return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error removing role ID {RoleId} from cache", roleId);
            return false;
        }
    }

    public async Task<bool> SetUserIdToBlockedUsersToCacheAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AuthOptions.ExpireMinutes + AuthOptions.ExpireMinutes)
            };
            await cache.SetStringAsync($"{CacheKeyPrefixes.BlockUser}:{userId}", "blocked", cacheOptions, ct);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error setting user ID {UserId} to blocked users cache", userId);
            return false;
        }
    }

    public async Task<bool> RemoveFromBlockedUsersCacheAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync($"{CacheKeyPrefixes.BlockUser}:{userId}", ct);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error setting user ID {UserId} to blocked users cache", userId);
            return false;
        }
    }

    public async Task<bool> UserIsBlockedAsync(int userId, CancellationToken ct = default)
    {
        var blockedValue = await cache.GetStringAsync($"{CacheKeyPrefixes.BlockUser}:{userId}", ct);
        return !string.IsNullOrEmpty(blockedValue);
    }
}
