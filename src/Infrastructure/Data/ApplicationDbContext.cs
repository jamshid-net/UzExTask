using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Domain.Common;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public IQueryable<TEntity> SetEntity<TEntity>() where TEntity : BaseEntity => Set<TEntity>();
    public IQueryable<TEntity> SetEntityNoTracking<TEntity>() where TEntity : class => Set<TEntity>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
