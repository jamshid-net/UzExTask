using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Application.RoleAndPermissions.Queries;
public class RoleDto : BaseAuditDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    private class Mapping: Profile
    {
        public Mapping()
        {
            CreateMap<Role, RoleDto>();
        }
    }
}
