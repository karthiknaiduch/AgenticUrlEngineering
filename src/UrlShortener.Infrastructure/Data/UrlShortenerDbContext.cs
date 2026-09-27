using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

public class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(
        DbContextOptions<UrlShortenerDbContext> options)
        : base(options)
    {
    }

    public DbSet<UrlMapping> UrlMappings => Set<UrlMapping>();

    public DbSet<UrlClick> UrlClicks => Set<UrlClick>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UrlShortenerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}