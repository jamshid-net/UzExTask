using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Application.RoleAndPermissions.Queries;
public class RoleDetailsDto : BaseAuditDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<string> Permissions { get; set; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Role, RoleDetailsDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(p => p.Name).ToList()));
        }
    }
}
