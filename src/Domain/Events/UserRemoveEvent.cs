using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Domain.Events;
public class UserRemoveEvent(User user) : BaseEvent
{
    public User User => user;
}
