using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.QueryFilter;

namespace ProjectTemplate.Application.Users.Manage.Queries;

public class GetUserFilterQuery : FilterRequest, IRequest<PageList<UserDto>>;

public class GetUserFilterQueryHandler(IApplicationDbContext dbContext, IMapper mapper) : IRequestHandler<GetUserFilterQuery, PageList<UserDto>>
{
    public async Task<PageList<UserDto>> Handle(GetUserFilterQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Users
                              .AsNoTracking()
                              .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                              .ToPageListAsync(request, cancellationToken);

    }
}

