using Nest.Thermostat.Api.Configuration;
using Nest.Thermostat.Api.Infrastructure;
using Nest.Thermostat.Api.Services;
using Nest.Thermostat.Core.Caching;
using Nest.Thermostat.Core.Repositories;
using Nest.Thermostat.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<NestProxySettings>(builder.Configuration.GetSection("NestProxy"));
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

var apiSettings = builder.Configuration.GetSection("NestApi").Get<NestApiSettings>() ?? new NestApiSettings();
builder.Services.AddSingleton(apiSettings);

// Core services
builder.Services.AddSingleton<IDocumentStore, FileDocumentStore>();
builder.Services.AddSingleton<ICache, InMemoryCache>();
builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();

// API services
builder.Services.AddSingleton<FileLoggingService>();
builder.Services.AddHttpClient<NestProxyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});

// ASP.NET Core
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Nest Thermostat API", Version = "v1" });
});

var app = builder.Build();

// Ensure storage directory exists
var storageSettings = builder.Configuration.GetSection("Storage").Get<StorageSettings>() ?? new StorageSettings();
Directory.CreateDirectory(storageSettings.BasePath);

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    name = "Nest Thermostat API",
    version = "1.0.0",
    endpoints = new[]
    {
        "/nest/entry",
        "/nest/ping",
        "/nest/passphrase",
        "/nest/transport",
        "/nest/weather/v1",
        "/devices",
        "/health"
    }
}));

app.Run();
