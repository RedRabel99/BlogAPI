using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Infrastructure.TypeConfiguration;

internal class UserProfileTypeConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfile");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(40);
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(40);

        builder.HasIndex(x => x.ApplicationUserId).IsUnique();
    }
}
