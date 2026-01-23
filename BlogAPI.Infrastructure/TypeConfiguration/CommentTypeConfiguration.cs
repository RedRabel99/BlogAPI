using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Infrastructure.TypeConfiguration;

public class CommentTypeConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.UserProfileId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
