using ProjectTemplate.Application.Common.Interfaces;

namespace ProjectTemplate.Application.RoleAndPermissions.Queries;
public record GetRoleDetailQuery(int RoleId) : IRequest<RoleDetailsDto>;
public class GetRoleDetailQueryHandler(IApplicationDbContext dbContext, IMapper mapper) : IRequestHandler<GetRoleDetailQuery, RoleDetailsDto>
{
    public async Task<RoleDetailsDto> Handle(GetRoleDetailQuery request, CancellationToken cancellationToken)
    {
        var roleDetails = await dbContext.Roles.AsNoTracking()
                               .ProjectTo<RoleDetailsDto>(mapper.ConfigurationProvider)
                               .FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);

        if(roleDetails is null)
        {
            throw new NotFoundException(request.RoleId.ToString(), nameof(RoleDetailsDto));
        }

        return roleDetails;
    }
}
