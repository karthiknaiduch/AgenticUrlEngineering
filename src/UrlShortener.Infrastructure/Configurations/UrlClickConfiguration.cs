using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Configurations;

public class UrlClickConfiguration
    : IEntityTypeConfiguration<UrlClick>
{
    public void Configure(
        EntityTypeBuilder<UrlClick> builder)
    {
        builder.ToTable("UrlClicks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
         builder.Property(x => x.EventId)
        .IsRequired();

    builder.HasIndex(x => x.EventId)
        .IsUnique();

        builder.Property(x => x.UrlMappingId)
            .IsRequired();

        builder.Property(x => x.TimestampUtc)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(100);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(1000);

        builder.Property(x => x.Referrer)
            .HasMaxLength(2048);

        builder.HasIndex(x => x.UrlMappingId);

        builder.HasIndex(x => x.TimestampUtc);
    }
}