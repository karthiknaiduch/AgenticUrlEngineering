using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Data;
using UrlShortener.AnalyticsWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<UrlShortenerDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration
                .GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<KafkaAnalyticsConsumer>();

var host = builder.Build();

host.Run();