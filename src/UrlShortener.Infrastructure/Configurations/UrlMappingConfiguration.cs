using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Configurations;

public class UrlMappingConfiguration : IEntityTypeConfiguration<UrlMapping>
{
    public void Configure(EntityTypeBuilder<UrlMapping> builder)
    {
        builder.ToTable("UrlMappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ShortCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.ShortCode)
            .IsUnique();

        builder.Property(x => x.OriginalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}