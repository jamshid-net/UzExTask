using ProjectTemplate.Application.Common.Interfaces;

namespace ProjectTemplate.Application.RoleAndPermissions.Commands;
public record DeleteRoleCommand(int RoleId) : IRequest<bool>;

public class DeleteRoleCommandHandler(IApplicationDbContext dbContext, IAuthCacheService cacheService) : IRequestHandler<DeleteRoleCommand, bool>
{
    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var effectedRows = await dbContext.Roles.Where(x => x.Id == request.RoleId).ExecuteDeleteAsync(cancellationToken);
        
        if (effectedRows > 0)
        {
           await cacheService.RemoveRoleAsync(request.RoleId, cancellationToken);
        }
        return effectedRows > 0;
    }
}
