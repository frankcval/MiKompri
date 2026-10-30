using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MiKompri.ShoppingList.Application.DTOs;
using MiKompri.ShoppingList.Application.Tests.IntegrationTest;
using MiKompri.ShoppingList.Domain.Entities;
using MiKompri.ShoppingList.Infrastructure.Persistence;

namespace MiKompri.ShoppingList.Api.Tests.Security;

public class PersonalListsBearerTests
{
    private const string Issuer = "https://sts.windows.net/a6b72a33-ade4-45d3-a2a7-52b88d96f5eb/";
    private const string Audience = "api://36adb79e-9563-4642-8bc9-455358d76273";
    private static readonly RsaSecurityKey Key = new(RSA.Create(2048)) { KeyId = "bearer-test" };
    private const string ClientId = "personal-lists-bearer-spa";

    private static WebApplicationFactory<ShoppingListApiProgram> WithBearer(
        CustomWebApplicationFactory<ShoppingListApiProgram> factory) => factory.WithWebHostBuilder(builder =>
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
            ["Authentication:Authority"] = "https://login.microsoftonline.com/a6b72a33-ade4-45d3-a2a7-52b88d96f5eb/v2.0",
            ["Authentication:TenantId"] = "a6b72a33-ade4-45d3-a2a7-52b88d96f5eb",
            ["Authentication:Audience"] = Audience, ["Authentication:ValidAudiences"] = Audience,
            ["Authentication:ClientIds"] = ClientId, ["Authentication:ExternalId:Enabled"] = "false"
        })).ConfigureServices(services =>
        {
            // Match Docker's HTTP + HTTPS ports and catch cross-origin redirects that strip Bearer.
            services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options => options.HttpsPort = 8081);
            // Exercise the real JWT handler, including production claim mapping, without Entra/network.
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            services.Configure<JwtBearerOptions>("MiKompri-legacy", options =>
            {
                options.Authority = string.Empty;
                options.MetadataAddress = string.Empty;
                options.Configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                options.Configuration.SigningKeys.Add(Key);
                options.TokenValidationParameters.IssuerSigningKey = Key;
            });
        }));

    private static string Token(bool includeOid = true, string audience = Audience)
    {
        var claims = new List<Claim> { new("sub", "caller"), new("tid", "a6b72a33-ade4-45d3-a2a7-52b88d96f5eb"), new("scp", "access_as_user"), new("azp", ClientId) };
        if (includeOid) claims.Add(new("oid", "11111111-1111-1111-1111-111111111111"));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            Issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.RsaSha256)));
    }

    [Fact]
    public async Task Mine_WithSignedBearer_ReturnsOnlyCallersPersonalLists()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var app = WithBearer(factory);
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = Token();
        var ownerId = factory.UsersApiState.EnsureUser(token);
        var personal = new PurchaseList("Mine", ownerId);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShoppingListDbContext>();
            db.PurchaseList.AddRange(personal,
                new PurchaseList("Other owner", Guid.NewGuid()),
                new PurchaseList("Shared", ownerId, Guid.NewGuid()));
            await db.SaveChangesAsync();
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:5173");

        // Client-supplied owner/group selectors must not override the authenticated identity.
        var response = await client.GetAsync($"/api/v1/purchaselists/mine?ownerId={Guid.NewGuid()}&groupId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin"));
        var lists = await response.Content.ReadFromJsonAsync<List<PurchaseListDTO>>();
        Assert.Equal(personal.Id, Assert.Single(lists!).Id);
        Assert.Contains("/api/v1/users/me/access-status", factory.UsersApiState.GetRequests());
    }

    [Fact]
    public async Task Mine_WithNoLists_ReturnsEmptyArray()
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var app = WithBearer(factory);
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token());
        var response = await client.GetAsync("/api/v1/purchaselists/mine");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<PurchaseListDTO>>())!);
    }

    [Theory]
    [InlineData(false, Audience)]
    [InlineData(true, "api://wrong-audience")]
    [InlineData(true, null)]
    public async Task Mine_WithoutValidIdentity_Returns401(bool includeOid, string? audience)
    {
        using var factory = new CustomWebApplicationFactory<ShoppingListApiProgram>();
        using var app = WithBearer(factory);
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        if (audience is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(includeOid, audience));
        var response = await client.GetAsync("/api/v1/purchaselists/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(factory.UsersApiState.GetRequests());
    }
}
