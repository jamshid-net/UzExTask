using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Application.Common.Interfaces;
public interface ICustomIdentityService
{
    Task<bool> HasPermissionAsync(int? userId, EnumPermission permission, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int? userId, IReadOnlyCollection<EnumPermission>? permissions, CancellationToken ct = default);
    Task<string?> GetUserNameAsync(int? userId, CancellationToken ct = default);

}
