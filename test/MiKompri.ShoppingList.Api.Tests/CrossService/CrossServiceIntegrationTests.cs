using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiKompri.ShoppingList.Api;
using MiKompri.ShoppingList.Api.Models.SharedLists;
using MiKompri.ShoppingList.Infrastructure.Persistence;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest.CrossService
{
    /// <summary>
    /// Tests de integración cross-service: validan el flujo completo
    /// ShoppingList → Users.Api REAL (no FakeUsersApiHandler).
    ///
    /// Cubren:
    ///  - Token JWT válido (TestAuthHandler compartido).
    ///  - sub no-GUID (string externo).
    ///  - Resolución al UserId interno.
    ///  - Miembro autorizado → acceso concedido.
    ///  - No miembro → 403.
    ///  - Sin token → 401.
    /// </summary>
    public class CrossServiceIntegrationTests : IAsyncLifetime
    {
        private CrossServiceUsersApiFactory _usersFactory = null!;
        private WebApplicationFactory<ShoppingListApiProgram> _shoppingFactory = null!;
        private HttpClient _shoppingClient = null!;

        private const string OwnerSub = "xs-owner-external-sub";
        private const string MemberSub = "xs-member-external-sub";
        private const string OutsiderSub = "xs-outsider-external-sub";

        private Guid _groupId;

        public async Task InitializeAsync()
        {
            _usersFactory = new CrossServiceUsersApiFactory();

            // ── 1. Provisionar owner en Users.Api real ────────────────────────
            var ownerUsersClient = _usersFactory.CreateClient();
            ownerUsersClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", OwnerSub);

            var syncOwnerResp = await ownerUsersClient.PostAsync("/api/v1/users/me/sync", null);
            syncOwnerResp.EnsureSuccessStatusCode();

            // ── 2. Crear grupo (el owner queda automáticamente como miembro Owner) ──
            var createGroupResp = await ownerUsersClient.PostAsJsonAsync(
                "/api/v1/groups", new { Name = "XS Integration Group" });
            createGroupResp.EnsureSuccessStatusCode();
            var group = await createGroupResp.Content.ReadFromJsonAsync<GroupDto>();
            _groupId = group!.Id;

            // ── 3. Provisionar member y añadirle al grupo ─────────────────────
            var memberUsersClient = _usersFactory.CreateClient();
            memberUsersClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", MemberSub);
            var syncMemberResp = await memberUsersClient.PostAsync("/api/v1/users/me/sync", null);
            syncMemberResp.EnsureSuccessStatusCode();
            var memberProfile = await syncMemberResp.Content.ReadFromJsonAsync<UserProfileDto>();

            var addMemberResp = await ownerUsersClient.PostAsJsonAsync(
                $"/api/v1/groups/{_groupId}/members",
                new { UserId = memberProfile!.Id, Role = "Member" });
            addMemberResp.EnsureSuccessStatusCode();

            // ── 4. Crear ShoppingList factory wired a Users.Api real ──────────
            var usersApiForwardClient = _usersFactory.CreateClient();

            _shoppingFactory = new ShoppingListCrossServiceFactory(usersApiForwardClient);
            _shoppingClient = _shoppingFactory.CreateClient();
        }

        public Task DisposeAsync()
        {
            _shoppingClient.Dispose();
            _shoppingFactory.Dispose();
            _usersFactory.Dispose();
            return Task.CompletedTask;
        }

        // ── Tests ────────────────────────────────────────────────────────────

        [Fact]
        public async Task CrossService_ShouldReturn401_WhenNoToken()
        {
            var response = await _shoppingClient.GetAsync($"/api/v1/shared-lists/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CrossService_NonGuidSub_ShouldResolveToInternalUserId_And_AllowAccess()
        {
            // Owner con sub no-GUID crea y consulta una shared list
            _shoppingClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", OwnerSub);

            var createResp = await _shoppingClient.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "XS non-GUID sub list",
                GroupId = _groupId
            });
            var body = await createResp.Content.ReadAsStringAsync();
            Assert.True(createResp.IsSuccessStatusCode, $"Create failed: {body}");

            var listId = await createResp.Content.ReadFromJsonAsync<Guid>();
            var getResp = await _shoppingClient.GetAsync($"/api/v1/shared-lists/{listId}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task CrossService_ActiveMember_ShouldHaveAccess()
        {
            // El owner crea la lista
            _shoppingClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", OwnerSub);

            var createResp = await _shoppingClient.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "XS member access list",
                GroupId = _groupId
            });
            createResp.EnsureSuccessStatusCode();
            var listId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // El member accede a la lista
            _shoppingClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", MemberSub);
            var getResp = await _shoppingClient.GetAsync($"/api/v1/shared-lists/{listId}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        }

        [Fact]
        public async Task CrossService_NonMember_ShouldReturn403()
        {
            // El owner crea la lista
            _shoppingClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", OwnerSub);

            var createResp = await _shoppingClient.PostAsJsonAsync("/api/v1/shared-lists", new CreateSharedListRequest
            {
                Name = "XS outsider list",
                GroupId = _groupId
            });
            createResp.EnsureSuccessStatusCode();
            var listId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // El outsider intenta acceder (no es miembro del grupo, no está provisionado)
            _shoppingClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", OutsiderSub);
            var getResp = await _shoppingClient.GetAsync($"/api/v1/shared-lists/{listId}");
            Assert.Equal(HttpStatusCode.Forbidden, getResp.StatusCode);
        }

        // ── Factory ShoppingList wired al Users.Api test server ───────────────

        private sealed class ShoppingListCrossServiceFactory : WebApplicationFactory<ShoppingListApiProgram>
        {
            private readonly HttpClient _usersApiClient;
            private readonly string _dbName = $"ShoppingListXS_{Guid.NewGuid()}";

            public ShoppingListCrossServiceFactory(HttpClient usersApiClient)
            {
                _usersApiClient = usersApiClient;
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar DbContext
                    var toRemove = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<ShoppingListDbContext>))
                        .ToList();
                    foreach (var d in toRemove) services.Remove(d);

                    var inMemoryProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.AddDbContext<ShoppingListDbContext>(opts =>
                    {
                        opts.UseInMemoryDatabase(_dbName);
                        opts.UseInternalServiceProvider(inMemoryProvider);
                    });

                    // Reemplazar Auth con TestAuthHandler (lee "Authorization: Test {sub}")
                    services.AddAuthentication(opts =>
                    {
                        opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                    // Reemplazar HttpClient("UsersApi") con forwarding al servidor real de Users
                    services.AddHttpClient("UsersApi")
                        .ConfigurePrimaryHttpMessageHandler(
                            _ => new CrossServiceForwardingHandler(_usersApiClient));

                    builder.UseEnvironment("Development");
                });
            }
        }
    }
}
