using UrlShortener.Application.Services;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using StackExchange.Redis;
using UrlShortener.Api.Workers;
using UrlShortener.Infrastructure.Cache;
using Confluent.Kafka;
using UrlShortener.Infrastructure.Messaging;
using UrlShortener.Api.Middleware;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!));

builder.Services.AddScoped<IUrlCache, RedisUrlCache>();



//builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql")
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"]!,
        name: "redis");

builder.Services.AddScoped<UrlShortenerService>();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IProducer<string, string>>(
    _ =>
    {
        var config = new ProducerConfig
        {
            BootstrapServers =
    builder.Configuration["Kafka:BootstrapServers"]!
        };

        return new ProducerBuilder<string, string>(config)
            .Build();
    });

builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}