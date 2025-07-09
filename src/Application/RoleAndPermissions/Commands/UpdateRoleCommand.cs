using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Entities.Auth;
using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Application.RoleAndPermissions.Commands;
public record UpdateRoleCommand(int Id, string Name, EnumPermission[] Permissions) : IRequest<bool>;
public class UpdateRoleCommandHandler(IApplicationDbContext dbContext, IAuthCacheService cacheService) : IRequestHandler<UpdateRoleCommand, bool>
{
    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Role));
        }
        role.Name = request.Name;
        role.Permissions = await dbContext.Permissions
                                          .Where(p => request.Permissions.Contains(p.EnumPermission))
                                          .ToListAsync(cancellationToken);

        dbContext.Roles.Update(role);
        var isSaved = await dbContext.SaveChangesAsync(cancellationToken) > 0;

        if (isSaved)
        {
            await cacheService.SetPermissionAsync(role.Id, request.Permissions, cancellationToken);
        }
        return isSaved;
    }
}
