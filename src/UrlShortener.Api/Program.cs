using UrlShortener.Application.Services;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
//using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddScoped<UrlShortenerService>();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();