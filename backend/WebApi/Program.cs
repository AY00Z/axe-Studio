using backend.Domain.Entities.Project;
using backend.Domain.Interfaces.Services;
using backend.Infrastructure.Repositories;
using backend.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Register your custom services with interfaces
builder.Services.AddScoped<IConfigFilterService, ConfigFilterService>();
builder.Services.AddScoped<IConfigUpdateService, ConfigUpdateService>();
builder.Services.AddScoped<IProjectsConfigService, ProjectsConfigService>();

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add repository
builder.Services.AddScoped<IProjectsConfigRepository, ProjectsConfigRepository>();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Additional JSON serialization configuration
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.WriteIndented = true;
    options.Converters.Add(new JsonStringEnumConverter());
});

// ========== ADD KEYCLOAK AUTHENTICATION (NEW) ==========
builder.Services.AddAuthentication(options =>
{
    // Default scheme remains JWT Bearer as fallback
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Keycloak", options =>  // Named scheme for Keycloak
{
    options.Authority = builder.Configuration["Keycloak:Authority"]; // e.g., "http://localhost:8080/realms/MyRealm"
    options.Audience = builder.Configuration["Keycloak:Audience"];   // Your client ID in Keycloak
    options.RequireHttpsMetadata = true; 

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
})
// Keep existing JWT config as fallback
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Add authentication middleware (MUST come before UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();