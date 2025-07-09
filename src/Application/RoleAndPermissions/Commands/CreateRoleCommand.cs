using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Entities.Auth;
using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Application.RoleAndPermissions.Commands;
public record CreateRoleCommand(string Name, EnumPermission[] Permissions) : IRequest<bool>;
public class CreateRoleCommandHandler(IApplicationDbContext dbContext, IAuthCacheService cacheService) : IRequestHandler<CreateRoleCommand, bool>
{
    public async Task<bool> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Role name cannot be null or empty.", nameof(request.Name));
        }
        var foundPermissions = await dbContext.Permissions
            .Where(p => request.Permissions.Contains(p.EnumPermission))
            .ToListAsync(cancellationToken);

        if (foundPermissions.Count != request.Permissions.Length)
        {
            throw new InvalidOperationException("One or more permission IDs are invalid.");
        }

        var role = new Role
        {
            Name = request.Name,
            Permissions = foundPermissions
        };

        dbContext.Roles.Add(role);
        var isSaved = await dbContext.SaveChangesAsync(cancellationToken) > 0;

        if (isSaved)
        {
            await cacheService.SetPermissionAsync(role.Id, request.Permissions, cancellationToken);
        }
        return isSaved;

    }
}
