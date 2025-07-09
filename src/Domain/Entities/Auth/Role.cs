namespace ProjectTemplate.Domain.Entities.Auth;
public class Role : BaseAuditableEntity
{
    public required string Name { get; set; }
    public virtual ICollection<Permission> Permissions { get; set; } = [];
}
