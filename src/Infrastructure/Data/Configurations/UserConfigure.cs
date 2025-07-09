using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Domain.Entities.Auth;

namespace ProjectTemplate.Infrastructure.Data.Configurations;
public class UserConfigure : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.UserName)
               .IsUnique();

        builder.HasQueryFilter(x => x.IsActive);

    }
}
