// Plan (pseudocódigo detallado):
// 1. Crear una clase `CustomWebApplicationFactory` que derive de `WebApplicationFactory<Program>`.
//    - En `ConfigureWebHost` reemplazar la configuración del DbContext por una base de datos en memoria.
//    - Usar un nombre de base de datos único por instancia para aislamiento entre pruebas.
// 2. Escribir tests de integración que usen `CustomWebApplicationFactory` y `HttpClient` para:
//    - Crear una lista de compra con POST `/api/v1/purchaselists` -> comprobar 201 Created y obtener el id.
//    - Obtener la lista con GET `/api/v1/purchaselists/{id}` -> comprobar campos devueltos.
//    - Añadir item con POST `/api/v1/purchaselists/{id}/items` -> comprobar 204 NoContent.
//    - Obtener la lista y comprobar que el item está presente.
// 3. Mantener tests independientes y deterministas usando base de datos en memoria y nombre único.
// 4. Usar xUnit y las utilidades de `Microsoft.AspNetCore.Mvc.Testing` y `Microsoft.EntityFrameworkCore.InMemory`.
// 5. No realizar llamadas externas; todo se ejecuta contra el servidor en memoria con la configuración de tests.
// 6. Si es necesario, ajustar el tipo del DbContext y el namespace según la solución real.
// Nota: ajustar `ShoppingListDbContext` y el namespace del mismo si en tu solución difieren.

using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Ajusta estos namespaces según tu solución real.
// El test asume que el proyecto de la API tiene clase `Program` pública (punto de entrada).
// Y que el DbContext se llama `ShoppingListDbContext` en el namespace `MiKompri.ShoppingList.Infrastructure.Persistence`.
// Si tus nombres son distintos, cámbialos en la línea donde se hace typeof(...).
using MiKompri.ShoppingList.Api; // Para `Program`
 using MiKompri.ShoppingList.Infrastructure.Persistence; // Descomenta/ajusta si existe el DbContext en otro assembly

namespace MiKompri.ShoppingList.Api.IntegrationTests
{
    // Factory personalizada que reemplaza el DbContext por InMemory
    public class CustomWebApplicationFactory //: WebApplicationFactory<Program>
    {
    //    private readonly string _inMemoryDbName = Guid.NewGuid().ToString();

    //    protected override void ConfigureWebHost(IWebHostBuilder builder)
    //    {
    //        builder.ConfigureTestServices(services =>
    //        {
    //            // Intentamos localizar y reemplazar cualquier DbContextOptions<T> para nuestro DbContext.
    //            // Ajusta el tipo `ShoppingListDbContext` al tipo real en tu solución.
    //            var dbContextType = Type.GetType("MiKompri.ShoppingList.Infrastructure.Persistence.ShoppingListDbContext, MiKompri.ShoppingList.Infrastructure");

    //            if (dbContextType != null)
    //            {
    //                // Eliminar registros existentes relacionados con el DbContext (si existen)
    //                var descriptors = services.Where(d =>
    //                    d.ServiceType.IsGenericType &&
    //                    d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
    //                    && d.ServiceType.GenericTypeArguments[0] == dbContextType
    //                ).ToList();

    //                foreach (var d in descriptors)
    //                {
    //                    services.Remove(d);
    //                }

    //                // Añadir DbContext in-memory vía reflection (AddDbContext<T> no puede usarse con un Type dinámico sin MakeGenericMethod)
    //                var efInMemoryExtension = typeof(EntityFrameworkServiceCollectionExtensions);
    //                var addDbContextMethod = efInMemoryExtension
    //                    .GetMethods()
    //                    .Where(m => m.Name == "AddDbContext" && m.IsGenericMethodDefinition)
    //                    .Select(m => m)
    //                    .FirstOrDefault(m =>
    //                    {
    //                        var parameters = m.GetParameters();
    //                        // Buscamos la sobrecarga con (IServiceCollection, Action<DbContextOptionsBuilder>, ServiceLifetime, ServiceLifetime)
    //                        return parameters.Length >= 2;
    //                    });

    //                if (addDbContextMethod != null)
    //                {
    //                    var genericAddDbContext = addDbContextMethod.MakeGenericMethod(dbContextType);

    //                    // Construir una lambda Action<DbContextOptionsBuilder> que llame a UseInMemoryDatabase(_inMemoryDbName)
    //                    // Para simplificar la invocación por reflexión usaremos un pequeño helper tipo local.
    //                    // Aquí en código C# normal no es trivial construir el delegate por reflexión sin conocer el tipo genérico,
    //                    // así que en la mayoría de soluciones se preferirá referenciar directamente el DbContext tipo concreto.
    //                    // Si no puedes resolver el tipo por nombre, reemplaza esta clase y registra directamente:
    //                    // services.AddDbContext<ShoppingListDbContext>(opt => opt.UseInMemoryDatabase(_inMemoryDbName));
    //                }
    //            }
    //            else
    //            {
    //                // Si no se encuentra el DbContext por tipo reflexivo, intentar registro directo por convención:
    //                // Intentar eliminar registros de DbContextOptions<DbContext> y añadir un DbContext genérico in-memory.
    //                // Esto es un fallback y puede no funcionar si la app usa un DbContext concreto.
    //                var genericDescriptors = services.Where(d =>
    //                    d.ServiceType.IsGenericType &&
    //                    d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
    //                ).ToList();

    //                foreach (var d in genericDescriptors)
    //                {
    //                    services.Remove(d);
    //                }

    //                // Registrar un DbContext de tipo `DbContext` básico en memoria para asegurar que EF Core esté disponible.
    //                services.AddDbContext<DbContext>(options =>
    //                {
    //                    options.UseInMemoryDatabase(_inMemoryDbName);
    //                });
    //            }

    //            // Opcional: si la aplicación registra migraciones o inicializadores que se ejecutan al arrancar,
    //            // puedes forzar aquí la creación de la BD en memoria y agregar datos semilla.
    //        });
    //    }
    //}

    //public class PurchaseListsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    //{
    //    private readonly CustomWebApplicationFactory _factory;

    //    public PurchaseListsIntegrationTests(CustomWebApplicationFactory factory)
    //    {
    //        _factory = factory;
    //    }

    //    [Fact]
    //    public async Task CreateList_ReturnsCreated_And_GetById_ReturnsCreatedEntity()
    //    {
    //        var client = _factory.CreateClient();

    //        var createRequest = new
    //        {
    //            Name = "Lista integración",
    //            OwnerId = Guid.NewGuid(),
    //            GroupId = (Guid?)null
    //        };

    //        // Crear lista
    //        var postResponse = await client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
    //        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

    //        // Obtener location / id desde la respuesta
    //        var location = postResponse.Headers.Location;
    //        Assert.NotNull(location);

    //        // Llamar GET al location
    //        var getResponse = await client.GetAsync(location);
    //        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    //        var content = await getResponse.Content.ReadAsStringAsync();
    //        using var doc = JsonDocument.Parse(content);
    //        var root = doc.RootElement;

    //        Assert.Equal("Lista integración", root.GetProperty("name").GetString());
    //        Assert.True(root.GetProperty("id").GetGuid() != Guid.Empty);
    //    }

    //    [Fact]
    //    public async Task AddItem_ToList_WhenCreated_ItemAppearsInGet()
    //    {
    //        var client = _factory.CreateClient();

    //        var createRequest = new
    //        {
    //            Name = "Lista con item",
    //            OwnerId = Guid.NewGuid(),
    //            GroupId = (Guid?)null
    //        };

    //        var postResponse = await client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
    //        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

    //        var location = postResponse.Headers.Location;
    //        Assert.NotNull(location);

    //        // Obtener id desde location (/api/v1/purchaselists/{id})
    //        var id = location.Segments.Last(); // último segmento debería ser {id} o " {id}"
    //        id = id.Trim('/');

    //        // Preparar item
    //        var addItemRequest = new
    //        {
    //            ProductId = Guid.NewGuid(),
    //            ProductName = "Manzanas",
    //            Price = 1.5m,
    //            Quantity = 5
    //        };

    //        var addItemResponse = await client.PostAsJsonAsync($"/api/v1/purchaselists/{id}/items", addItemRequest);
    //        Assert.Equal(HttpStatusCode.NoContent, addItemResponse.StatusCode);

    //        // Obtener lista y comprobar items
    //        var getResponse = await client.GetAsync($"/api/v1/purchaselists/{id}");
    //        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    //        var content = await getResponse.Content.ReadAsStringAsync();
    //        using var doc = JsonDocument.Parse(content);
    //        var root = doc.RootElement;

    //        var items = root.GetProperty("items");
    //        Assert.True(items.GetArrayLength() == 1);
    //        var item = items[0];
    //        Assert.Equal("Manzanas", item.GetProperty("productName").GetString());
    //        Assert.Equal(5, item.GetProperty("quantity").GetInt32());
    //    }
   }
}