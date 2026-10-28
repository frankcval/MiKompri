using MiKompri.ProductCatalog.Api.Extensions;
using MiKompri.ProductCatalog.Api.Middleware;
using MiKompri.ProductCatalog.Api.Security;
using MiKompri.ProductCatalog.Application;
using MiKompri.ProductCatalog.Application.Options;
using MiKompri.ProductCatalog.Infrastructure;
using MiKompri.ProductCatalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

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

builder.Services.AddControllers();
builder.Services.Configure<ProductCatalogOptions>(
    builder.Configuration.GetSection(ProductCatalogOptions.SectionName));
builder.Services.AddProductCatalogApplication();
builder.Services.AddProductCatalogInfrastructure(builder.Configuration);
builder.Services.AddMiKompriHealthChecks(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.TokenValidationParameters = ProductCatalogJwtValidation.BuildTokenValidationParameters(
            issuer: builder.Configuration["Authentication:Authority"],
            audience: builder.Configuration["Authentication:Audience"],
            validAudiencesRaw: builder.Configuration["Authentication:ValidAudiences"]);
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
    if (db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseGlobalExceptionHandling();
app.UseCors(CorsPolicy);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();
app.MapMiKompriHealthChecks();

app.Run();

Log.CloseAndFlush();

public partial class Program;
