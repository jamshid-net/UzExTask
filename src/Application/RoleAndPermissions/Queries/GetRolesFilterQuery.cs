using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.QueryFilter;

namespace ProjectTemplate.Application.RoleAndPermissions.Queries;

public class GetRolesFilterQuery : FilterRequest, IRequest<PageList<RoleDto>>;
public class GetRolesFilterQueryHandler(IApplicationDbContext dbContext, IMapper mapper) : IRequestHandler<GetRolesFilterQuery, PageList<RoleDto>>
{
    public async Task<PageList<RoleDto>> Handle(GetRolesFilterQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Roles
                                                   .AsNoTracking()
                                                   .ProjectTo<RoleDto>(mapper.ConfigurationProvider)
                                                   .ToPageListAsync(request, cancellationToken);


        return result;
    }
}
