using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.IntegrationTests;
using MiKompri.ShoppingList.Api.Models;
using MiKompri.ShoppingList.Application.DTOs;
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
    }
}

