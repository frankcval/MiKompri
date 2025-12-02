using FluentValidation;
using MiKompri.ShoppingList.Api.Middleware;
using MiKompri.ShoppingList.Application;
using MiKompri.ShoppingList.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ----------------------
//  Middleware global de excepciones
// ----------------------

// Logging de requests
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseGlobalExceptionHandling();
app.UseCors(CorsPolicy);


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapMiKompriHealthChecks();

app.Run();
