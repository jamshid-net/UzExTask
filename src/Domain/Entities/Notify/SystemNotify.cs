using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Domain.Entities.Notify;
public class SystemNotify : BaseEntity
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public bool IsRead { get; set; } = false;
    public virtual ICollection<User> Receivers { get; set; } = [];
}
