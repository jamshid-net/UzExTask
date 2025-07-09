using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Events;

namespace ProjectTemplate.Application.Users.Manage.Commands;
public record RemoveUserCommand(int UserId) : IRequest<bool>;
public class RemoveUserCommandHandler(IApplicationDbContext context, 
                                      IAuthCacheService cache, 
                                      INotifyHubService notifyHubService) : IRequestHandler<RemoveUserCommand, bool>
{
    public async Task<bool> Handle(RemoveUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken);
        if (user is null)
            throw new NotFoundException(request.UserId.ToString(), "User");

        user.IsActive = false;

        var isSaved = await context.SaveChangesAsync(cancellationToken) > 0;

        if (isSaved)
        {
            user.AddDomainEvent(new UserRemoveEvent(user));
            await cache.SetUserIdToBlockedUsersToCacheAsync(request.UserId, cancellationToken);
            await notifyHubService.NotifyAllAsync($"{user.UserName} is removed");
        }

        return isSaved;
    }
}

