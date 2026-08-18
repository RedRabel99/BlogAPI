using BlogAPI.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Infrastructure.TypeConfiguration;

public class OutboxMessageTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.ProcessedOn);

        builder.Property(x => x.Error)
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.NextAttemptOn);

        builder.HasIndex(x => new { x.NextAttemptOn, x.OccurredOn })
            .HasFilter("\"ProcessedOn\" IS NULL");
    }
}
