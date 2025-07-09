using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Domain.Events;
public class UserCreatedEvent(User user) : BaseEvent
{
    public User User => user;
}
