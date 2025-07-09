namespace ProjectTemplate.Domain.Entities.Auth;
public class UserRefreshToken : BaseEntity
{
    public required string RefreshToken { get; set; }
    public int UserId { get; set; }
    public virtual User? User { get; set; }
    public string? DeviceId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
}
