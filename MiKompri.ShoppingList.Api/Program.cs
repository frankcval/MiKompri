using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiKompri.ShoppingList.Api.Middleware;
using MiKompri.ShoppingList.Api.Security;
using MiKompri.ShoppingList.Api.Services;
using MiKompri.ShoppingList.Application;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Infrastructure;
using MiKompri.ShoppingList.Infrastructure.Persistence;
using Npgsql;
using Serilog;


Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()         // Bootstrap logger (por si falla el host)
        .CreateBootstrapLogger();

Log.Information("Iniciando MiKompri.ShoppingList.Api");

var builder = WebApplication.CreateBuilder(args);

//  Sustituimos el proveedor de logging por Serilog leyendo de appsettings
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
    // No hace falta añadir WriteTo aquí porque ya está en appsettings.json
});
// ----------------------
//  CORS  reglas, por ahora abiertas
// ----------------------
const string CorsPolicy = "MiKompriCors";
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentDateProvider, ClientCurrentDateProvider>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Preserve original Entra tid/oid claims for provisioning and UsersApi forwarding.
        options.MapInboundClaims = false;
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.TokenValidationParameters = ShoppingListJwtValidation.BuildTokenValidationParameters(
            issuer: builder.Configuration["Authentication:Authority"],
            audience: builder.Configuration["Authentication:Audience"],
            validAudiencesRaw: builder.Configuration["Authentication:ValidAudiences"]);
    });

builder.Services.AddAuthorization();

// Capa de aplicación (MediatR, validadores, etc.)
builder.Services.AddAplicaction();

// Capa de infraestructura (DbContext, repos, UnitOfWork)
//builder.Services.AddIn
builder.Services.AddInfrastructure(builder.Configuration);
// REGISTRO DE HEALTH CHECKS (ANTES DE Build)
builder.Services.AddMiKompriHealthChecks(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT con el esquema Bearer. Ejemplo: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ShoppingListDbContext>();
    if (db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            );

            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            SELECT '20251121112328_InitialCreate', '9.0.10'
            WHERE EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = 'purchase_lists'
            )
            AND NOT EXISTS (
                SELECT 1
                FROM ""__EFMigrationsHistory""
                WHERE ""MigrationId"" = '20251121112328_InitialCreate'
            );
        ");

        db.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ----------------------
//  Middleware global de excepciones
// ----------------------

// Logging de requests (usa ILogger - ahora va a Serilog)
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseGlobalExceptionHandling();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserProvisioningMiddleware>();

// Local Vite calls HTTP directly; redirecting to another port strips its Bearer header.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapMiKompriHealthChecks();

// Ejecuta la app; al terminar continúa y cerramos Serilog
app.Run();

Log.CloseAndFlush();

/// <summary>
/// Marcador de entrada para WebApplicationFactory en tests de integración.
/// Usar <see cref="ShoppingListApiProgram"/> cuando el proyecto de test
/// también referencie MiKompri.Users.Api para evitar ambigüedad con Program.
/// </summary>
public sealed class ShoppingListApiProgram { }
