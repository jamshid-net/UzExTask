using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Entities.Auth;
using ProjectTemplate.Domain.Events;
using ProjectTemplate.Shared.Helpers;

namespace ProjectTemplate.Application.Users.Manage.Commands;
public class CreateUserCommand : IRequest<bool>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int RoleId { get; set; }
}
public class CreateUserCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateUserCommand, bool>
{

    public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var hashSalt = CryptoPassword.CreateHashSalted(request.Password);
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            UserName = request.UserName,
            IsActive = request.IsActive,
            PasswordHash = hashSalt.Hash,
            PasswordSalt = hashSalt.Salt,
            LastLogin = DateTimeOffset.UtcNow,
            RoleId = request.RoleId
        };
        user.AddDomainEvent(new UserCreatedEvent(user));
        await dbContext.Users.AddAsync(user, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
