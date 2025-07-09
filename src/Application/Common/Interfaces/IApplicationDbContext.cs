using ProjectTemplate.Domain.Common;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);
    IQueryable<TEntity> SetEntity<TEntity>() where TEntity : BaseEntity;
    IQueryable<TEntity> SetEntityNoTracking<TEntity>() where TEntity : class;
}
