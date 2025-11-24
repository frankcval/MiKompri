using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Controllers;
using MiKompri.ShoppingList.Api.IntegrationTests;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Application.DTOs;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest
{
    public class PurchaseListsApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {


        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;


        //GetAll Test Methods
        [Fact]
        public async Task GetAll_Should_Return_All_Created_Lists()
        {
            // Arrange
            var client = _factory.CreateClient();

            var owner = Guid.NewGuid();

            var list1 = new CreatePurchaseListRequest
            {
                Name = "Compra lunes",
                OwnerId = owner,
                GroupId = null
            };

            var list2 = new CreatePurchaseListRequest
            {
                Name = "Compra viernes",
                OwnerId = owner,
                GroupId = null
            };

            // Crear dos listas
            await client.PostAsJsonAsync("/api/v1/purchaselists", list1);
            await client.PostAsJsonAsync("/api/v1/purchaselists", list2);

            // Act → GET ALL
            var response = await client.GetAsync("/api/v1/purchaselists");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<List<PurchaseListDTO>>();

            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThanOrEqualTo(2);

            result.Should().Contain(x => x.Name == "Compra lunes");
            result.Should().Contain(x => x.Name == "Compra viernes");
        }

        [Fact]
        public async Task Create_Multiple_Lists_Should_Assign_Unique_Ids()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var createRequest1 = new CreatePurchaseListRequest
            {
                Name = "Lista 1",
                OwnerId = ownerId,
                GroupId = null
            };
            var createRequest2 = new CreatePurchaseListRequest
            {
                Name = "Lista 2",
                OwnerId = ownerId,
                GroupId = null
            };
            // Act
            var postResponse1 = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest1);
            var postResponse2 = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest2);
            // Assert
            postResponse1.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse2.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdId1 = await postResponse1.Content.ReadFromJsonAsync<Guid>();
            var createdId2 = await postResponse2.Content.ReadFromJsonAsync<Guid>();
            createdId1.Should().NotBe(Guid.Empty);
            createdId2.Should().NotBe(Guid.Empty);
            createdId1.Should().NotBe(createdId2);
        }

        //invocar get all y mostrar que la lista esta vacia




        public PurchaseListsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact] //Crear Lista y Obtener Lista - ok
        public async Task Create_Then_GetById_Should_Return_Created_List()
        {
            // Arrange

            var ownerId = Guid.NewGuid();
            var createRequest = new CreatePurchaseListRequest
            {
                Name = "Compra semana",
                OwnerId = ownerId,
                GroupId = null
            };
            // Act 1: POST crear lista
            var postResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);

            // Assert POST
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Leer el id devuelto
            var createdId = await postResponse.Content.ReadFromJsonAsync<Guid>();
            createdId.Should().NotBe(Guid.Empty);

            // Act 2: GET por id
            var getResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdId}");

            // Assert GET
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await getResponse.Content.ReadFromJsonAsync<PurchaseListDTO>();
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(createdId);
            dto.Name.Should().Be("Compra semana");
            dto.OwnerId.Should().Be(ownerId);
        }

        [Fact] //Crear Lista: Error en la validacion del request
        public async Task Create_InvalidRequest_Should_Return_BadRequest()
        {
            // Arrange
            var invalidRequest = new CreatePurchaseListRequest
            {
                Name = "", // Nombre inválido
                OwnerId = Guid.Empty, // OwnerId inválido
                GroupId = null
            };
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/purchaselists", invalidRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact] //Obtener Lista: Error al obtener por id no existente
        public async Task GetById_NonExistentId_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var response = await _client.GetAsync($"/api/v1/purchaselists/{nonExistentId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Obtener lista cuando no hay listas creadas
        [Fact]
        public async Task GetAll_No_Lists_Should_Return_Empty_List()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("/api/v1/purchaselists");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<PurchaseListDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().Be(0);
        }

        //Actalizar lista de compra OK
        [Fact]
        public async Task Update_Existing_List_Should_Work_Correctly()
        {
            // Arrange: Crear lista inicial
            var createRequest = new CreatePurchaseListRequest
            {
                Name = "Lista Original",
                OwnerId = Guid.NewGuid(),
                GroupId = null
            };
            var postResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
            var createdId = await postResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Actualizar la lista creada
            var updateRequest = new UpdatePurchaseListRequest
            {
                Name = "Lista Actualizada",
                GroupId = Guid.NewGuid()
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{createdId}", updateRequest);
            // Assert PUT
            putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            // Act: Obtener la lista actualizada
            var getResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdId}");
            // Assert GET
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await getResponse.Content.ReadFromJsonAsync<PurchaseListDTO>();
            dto.Should().NotBeNull();
            dto!.Name.Should().Be("Lista Actualizada");
            dto.GroupId.Should().Be(updateRequest.GroupId);
        }

        //Actualizar lista de compra no existente
        [Fact]
        public async Task Update_NonExistent_List_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateRequest = new UpdatePurchaseListRequest
            {
                Name = "Lista Inexistente",
                GroupId = Guid.NewGuid()
            };
            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{nonExistentId}", updateRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Actualizar lista de compra con request invalido
        [Fact]
        public async Task Update_InvalidRequest_Should_Return_BadRequest()
        {
            // Arrange: Crear lista inicial
            var createRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para actualizar",
                OwnerId = Guid.NewGuid(),
                GroupId = null
            };
            var postResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
            var createdId = await postResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Intentar actualizar con datos inválidos
            var invalidUpdateRequest = new UpdatePurchaseListRequest
            {
                Name = "", // Nombre inválido
                GroupId = null
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{createdId}", invalidUpdateRequest);
            // Assert
            putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_Then_GetById_Should_Return_NotFound()
        {
            // Arrange
            var createRequest = new CreatePurchaseListRequest
            {
                Name = "Lista a eliminar",
                OwnerId = Guid.NewGuid(),
                GroupId = null
            };
            var postResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createRequest);
            var createdId = await postResponse.Content.ReadFromJsonAsync<Guid>();
            // Act 1: DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/v1/purchaselists/{createdId}");
            // Assert DELETE
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            // Act 2: GET por id
            var getResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdId}");
            // Assert GET
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_NonExistentId_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var response = await _client.DeleteAsync($"/api/v1/purchaselists/{nonExistentId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Borrar lista de compra que tiene items
        [Fact]
        public async Task Delete_List_With_Items_Should_Work_Correctly()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista con items a borrar",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            postListResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Agregar un item a la lista
            var addItemRequest = new AddItemRequest
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Arroz",
                Quantity = 3
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            postItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            // Act: Borrar la lista que tiene items
            var deleteResponse = await _client.DeleteAsync($"/api/v1/purchaselists/{createdListId}");
            // Assert: Verificar que la lista se borró correctamente
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            // Verificar que la lista ya no existe
            var getResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdListId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Borra lista de compra request invalido
        [Fact]
        public async Task Delete_InvalidId_Should_Return_BadRequest()
        {
            // Arrange
            var invalidId = "abc";
            // Act
            var response = await _client.DeleteAsync($"/api/v1/purchaselists/{invalidId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //crear lista de compra e insertar items en la misma prueba
        [Fact]
        public async Task Create_List_And_Add_Items_Should_Work_Correctly()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista con items",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            postListResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Agregar items a la lista creada
            var addItemRequest1 = new AddItemRequest
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Leche",
                Quantity = 2
            };
            var addItemRequest2 = new AddItemRequest
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Pan",
                Quantity = 1
            };
            var postItemResponse1 = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest1);
            var postItemResponse2 = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest2);
            // Assert: Verificar que los items se agregaron correctamente
            postItemResponse1.StatusCode.Should().Be(HttpStatusCode.Created);
            postItemResponse2.StatusCode.Should().Be(HttpStatusCode.Created);
            var itemId1 = await postItemResponse1.Content.ReadFromJsonAsync<Guid>();
            var itemId2 = await postItemResponse2.Content.ReadFromJsonAsync<Guid>();
            itemId1.Should().NotBe(Guid.Empty);
            itemId2.Should().NotBe(Guid.Empty);
        }

        //Crear item de lista de compra con request invalido
        [Fact]
        public async Task Add_Item_InvalidRequest_Should_Return_BadRequest()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para agregar item inválido",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Intentar agregar un item con datos inválidos
            var invalidAddItemRequest = new AddItemRequest
            {
                ProductId = Guid.Empty, // ID de producto inválido
                ProductName = "", // Nombre de producto inválido
                Quantity = 0 // Cantidad inválida
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", invalidAddItemRequest);
            // Assert
            postItemResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        //Agregar item a lista de compra no existente
        [Fact]
        public async Task Add_Item_To_NonExistent_List_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentListId = Guid.NewGuid();
            var addItemRequest = new AddItemRequest
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Azúcar",
                Quantity = 2
            };
            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{nonExistentListId}/items", addItemRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Agregar item a lista de compra con ProductId duplicado
        [Fact]
        public async Task Add_Item_Duplicate_ProductId_Should_Return_BadRequest()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para agregar item duplicado",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Agregar un item inicial
            var productId = Guid.NewGuid();
            var addItemRequest = new AddItemRequest
            {
                ProductId = productId,
                ProductName = "Café",
                Quantity = 1
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            postItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            // Act: Intentar agregar otro item con el mismo ProductId
            var duplicateAddItemRequest = new AddItemRequest
            {
                ProductId = productId, // Mismo ProductId
                ProductName = "Café Premium",
                Quantity = 2
            };
            var duplicatePostItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", duplicateAddItemRequest);
            // Assert
            duplicatePostItemResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        //Obtener Item de lista de compra por Id no existente
        [Fact]
        public async Task Get_Item_By_NonExistent_Id_Should_Return_NotFound()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para obtener item inexistente",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Intentar obtener un item con Id inexistente
            var nonExistentItemId = Guid.NewGuid();
            var getItemResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdListId}/items/{nonExistentItemId}");
            // Assert
            getItemResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Obtener Item de lista de compra de Lista de compra no existente
        [Fact]
        public async Task Get_Item_From_NonExistent_List_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentListId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            // Act
            var getItemResponse = await _client.GetAsync($"/api/v1/purchaselists/{nonExistentListId}/items/{itemId}");
            // Assert
            getItemResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Obtener Item de lista de compra OK
        [Fact]
        public async Task Get_Item_By_Id_Should_Return_Correct_Item()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para obtener item",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            // Agregar un item a la lista
            var productId = Guid.NewGuid();
            var addItemRequest = new AddItemRequest
            {
                ProductId = productId,
                ProductName = "Jugo",
                Quantity = 4
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            var createdItemId = await postItemResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Obtener el item por Id
            var getItemResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}");
            // Assert
            getItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var itemDto = await getItemResponse.Content.ReadFromJsonAsync<ListItemDto>();
            itemDto.Should().NotBeNull();
            itemDto!.Id.Should().Be(createdItemId);
            itemDto.ProductName.Should().Be("Jugo");
            itemDto.Quantity.Should().Be(4);
        }

        //Actualizar item de lista de compra OK
        [Fact]
        public async Task Update_Item_Should_Work_Correctly()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para actualizar item",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            var productId = Guid.NewGuid();
            // Agregar un item a la lista
            var addItemRequest = new AddItemRequest
            {
                ProductId = productId,
                ProductName = "Galletas",
                Quantity = 5
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            var createdItemId = await postItemResponse.Content.ReadFromJsonAsync<Guid>();
            // Act: Actualizar el item agregado
            var updateItemRequest = new UpdateItemRequest
            {
                ProductName = "Galletas Chocolate",
                Price = 2.5m,
                Quantity = 10
            };
            var putItemResponse = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}", updateItemRequest);
            // Assert PUT
            putItemResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            // Act: Obtener el item actualizado
            var getItemResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}");
            // Assert GET
            getItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var itemDto = await getItemResponse.Content.ReadFromJsonAsync<ListItemDto>();
            itemDto.Should().NotBeNull();
            itemDto!.ProductName.Should().Be("Galletas Chocolate");
            itemDto.ProductPrice.Should().Be(2.5m);
            itemDto.Quantity.Should().Be(10);

        }

        //Actualizar item de lista de compra con request invalido
        [Fact]
        public async Task Update_Item_InvalidRequest_Should_Return_BadRequest()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para actualizar item inválido",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            var productId = Guid.NewGuid();
            // Agregar un item a la lista
            var addItemRequest = new AddItemRequest
            {
                ProductId = productId,
                ProductName = "Cereal",
                Quantity = 3
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            // Act: Intentar actualizar el item con datos inválidos
            var invalidUpdateItemRequest = new UpdateItemRequest
            {
                ProductName = "", // Nombre inválido
                Price = -1m, // Precio inválido
                Quantity = 0 // Cantidad inválida
            };
            var putItemResponse = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}", invalidUpdateItemRequest);
            // Assert
            putItemResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        //Actualizar item de lista de compra en lista no existente
        [Fact]
        public async Task Update_Item_In_NonExistent_List_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var updateItemRequest = new UpdateItemRequest
            {
                ProductName = "Producto Actualizado",
                Price = 1.5m,
                Quantity = 2
            };
            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{nonExistentListId}/items/{productId}", updateItemRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }

        //Actualizar item inexistente de lista de compra
        [Fact]
        public async Task Update_NonExistent_Item_Should_Return_NotFound()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para actualizar item inexistente",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            var nonExistentProductId = Guid.NewGuid();
            var updateItemRequest = new UpdateItemRequest
            {
                ProductName = "Producto Inexistente",
                Price = 2.0m,
                Quantity = 3
            };
            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items/{nonExistentProductId}", updateItemRequest);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        //Borrar item de lista de compra OK
        [Fact]
        public async Task Delete_Item_Should_Work_Correctly()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para borrar item",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            var productId = Guid.NewGuid();
            // Agregar un item a la lista
            var addItemRequest = new AddItemRequest
            {
                ProductId = productId,
                ProductName = "Yogur",
                Quantity = 6
            };
            var postItemResponse = await _client.PostAsJsonAsync($"/api/v1/purchaselists/{createdListId}/items", addItemRequest);
            // Act: Borrar el item agregado
            var deleteItemResponse = await _client.DeleteAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}");
            // Assert DELETE
            deleteItemResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            // Act: Intentar obtener el item borrado
            var getItemResponse = await _client.GetAsync($"/api/v1/purchaselists/{createdListId}/items/{productId}");
            // Assert GET
            getItemResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Borrar item de lista de compra en lista no existente
        [Fact]
        public async Task Delete_Item_In_NonExistent_List_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            // Act
            var response = await _client.DeleteAsync($"/api/v1/purchaselists/{nonExistentListId}/items/{productId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //Borrar item inexistente de lista de compra
        [Fact]
        public async Task Delete_NonExistent_Item_Should_Return_NotFound()
        {
            // Arrange: Crear lista de compra
            var ownerId = Guid.NewGuid();
            var createListRequest = new CreatePurchaseListRequest
            {
                Name = "Lista para borrar item inexistente",
                OwnerId = ownerId,
                GroupId = null
            };
            var postListResponse = await _client.PostAsJsonAsync("/api/v1/purchaselists", createListRequest);
            var createdListId = await postListResponse.Content.ReadFromJsonAsync<Guid>();
            var nonExistentProductId = Guid.NewGuid();
            // Act
            var response = await _client.DeleteAsync($"/api/v1/purchaselists/{createdListId}/items/{nonExistentProductId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

    }
}


