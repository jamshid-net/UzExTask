namespace ProjectTemplate.Domain.Entities.Auth;
public class User : BaseAuditableEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string UserName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastLogin { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public int FailedLoginAttempts { get; set; } = 0;
    public int RoleId { get; set; }
    public virtual Role? Role { get; set; }
}
