using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using GameRadar.Api.Services;
using GameRadar.Api.Interfaces;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameRadar API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure Redis Caching
var redisConnectionString = builder.Configuration.GetConnectionString("Redis:ConnectionString");
if (string.IsNullOrEmpty(redisConnectionString)) 
{
    // Fallback or handle error if connection string is not found
    // For now, let's try to get it directly if GetConnectionString fails for nested keys
    redisConnectionString = builder.Configuration["Redis:ConnectionString"];
}

if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "GameRadar_"; // Optional: prefix for cache keys
    });
}
else
{
    // Log or handle the absence of Redis connection string
    Console.WriteLine("Redis connection string not found. Distributed caching will not be available.");
    // Optionally, register a memory cache as a fallback if Redis is not configured
    builder.Services.AddDistributedMemoryCache(); 
}

// Register HttpClient with retry policy
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GameRadar API v1"));
}

app.UseCors("AllowAll");

// Don't use HTTPS redirection in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
