using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Application.Users.Manage.Queries;
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string UserName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastLogin { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int RoleId { get; set; }
    private class Mapping: Profile
    {
        public Mapping()
        {
            CreateMap<User, UserDto>();
        }
    }
}
