using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Data;
using UrlShortener.AnalyticsWorker;

var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine(
    $"Kafka BootstrapServers = {builder.Configuration["Kafka:BootstrapServers"]}");

Console.WriteLine(
    $"Connection String = {builder.Configuration.GetConnectionString("DefaultConnection")}");

builder.Services.AddDbContext<UrlShortenerDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<KafkaAnalyticsConsumer>();

var host = builder.Build();

host.Run();