using System.Text;
using Clms.Api.Auth;
using Clms.Api.Data;
using Clms.Api.Endpoints;
using Clms.Api.Instruments;
using Clms.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- configuration
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenService>();

var watcherOptions = builder.Configuration.GetSection("InstrumentWatcher").Get<InstrumentWatcherOptions>()
                     ?? new InstrumentWatcherOptions();
builder.Services.AddSingleton(watcherOptions);

// ---------------------------------------------------------------------- database
var connectionString = builder.Configuration.GetConnectionString("Clms")
    ?? "Host=localhost;Port=5432;Database=clms;Username=clms;Password=clms";

builder.Services.AddDbContext<ClmsDbContext>(o => o.UseNpgsql(connectionString));

// ----------------------------------------------------------------- authn / authz
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// One policy per role, named after the role — so endpoints read
// .RequireAuthorization(ClmsRoles.LabManager) instead of repeating policy wiring.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ClmsRoles.LabManager, p => p.RequireRole(ClmsRoles.LabManager))
    .AddPolicy(ClmsRoles.SpecimenCollector, p => p.RequireRole(ClmsRoles.SpecimenCollector, ClmsRoles.LabManager))
    .AddPolicy(ClmsRoles.LabTechnician, p => p.RequireRole(ClmsRoles.LabTechnician, ClmsRoles.LabManager))
    .AddPolicy(ClmsRoles.Cashier, p => p.RequireRole(ClmsRoles.Cashier, ClmsRoles.LabManager));

// ---------------------------------------------------------------------- services
builder.Services.AddOpenApi();
builder.Services.AddHostedService<InstrumentFileWatcher>();

const string CorsPolicy = "ClmsClients";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(builder.Configuration
            .GetSection("AllowedClientOrigins").Get<string[]>()
            ?? ["http://localhost:5080", "https://localhost:7080"])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// ---------------------------------------------------------------------- pipeline
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference(); // interactive API docs at /scalar/v1

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }))
   .AllowAnonymous()
   .WithTags("Health");

app.MapAuthEndpoints();
app.MapInventoryEndpoints();
app.MapTestOrderEndpoints();

// Create schema + demo data on boot. See SeedData for the migrations note.
await SeedData.InitializeAsync(app.Services);

app.Run();
