using Microsoft.EntityFrameworkCore;
using MiKompri.ShoppingList.Api.Middleware;
using MiKompri.ShoppingList.Application;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Capa de aplicación (MediatR, validadores, etc.)
builder.Services.AddAplicaction();

// Capa de infraestructura (DbContext, repos, UnitOfWork)
//builder.Services.AddIn
builder.Services.AddInfrastructure(builder.Configuration);
// REGISTRO DE HEALTH CHECKS (ANTES DE Build)
builder.Services.AddMiKompriHealthChecks(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapMiKompriHealthChecks();

// Ejecuta la app; al terminar continúa y cerramos Serilog
app.Run();

Log.CloseAndFlush();
