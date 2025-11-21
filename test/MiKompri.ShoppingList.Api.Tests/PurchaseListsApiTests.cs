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
    public class PurchaseListsApiTests: IClassFixture<CustomWebApplicationFactory<Program>>
    {
 

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public PurchaseListsApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        [Fact]
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
        public async Task GetById_NonExistentId_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var response = await _client.GetAsync($"/api/v1/purchaselists/{nonExistentId}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
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
}
}

