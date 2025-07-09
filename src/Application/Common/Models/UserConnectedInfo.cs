namespace ProjectTemplate.Application.Common.Models;

public class UserConnectedInfo
{
    public required string ConnectionId { get; set; }
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? Referer { get; set; }
    public DateTimeOffset ConnectionTimeUtc { get; set; }
    public int RoleId { get; set; }
}
