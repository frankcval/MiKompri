using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Infrastructure.Identity;
using MiKompri.Users.Infrastructure.Persistence;
using Npgsql;

// QA-only administrative entry point; no public account-linking endpoint.
if (args.Length < 2 || !new[] { "reserve", "apply", "rollback" }.Contains(args[0]))
{
    Console.Error.WriteLine("Usage: reserve|apply|rollback <private-QA-config.json>"); return 2;
}
try
{
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var config = JsonSerializer.Deserialize<Settings>(File.ReadAllText(args[1]), options) ?? throw new InvalidOperationException();
    var connection = Environment.GetEnvironmentVariable("MIKOMPRI_MIGRATION_QA_DATABASE") ?? throw new InvalidOperationException();
    var database = new NpgsqlConnectionStringBuilder(connection);
    if (config.Environment != "QA" || database.Database != config.AllowedQaDatabase ||
        !(database.Database?.StartsWith("users010a_", StringComparison.Ordinal) == true || database.Database?.StartsWith("MiKompri010QA", StringComparison.Ordinal) == true))
        throw new InvalidOperationException("Only an explicitly named disposable QA database is allowed.");
    using var graph = new HttpClient { BaseAddress = new Uri("https://graph.microsoft.com/"), Timeout = TimeSpan.FromSeconds(15) };
    if (args[0] != "rollback") graph.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        Environment.GetEnvironmentVariable("MIKOMPRI_MIGRATION_GRAPH_TOKEN") ?? throw new InvalidOperationException());
    var verifier = new FederatedOriginVerifier(graph, config.SourceTenantId, config.TargetTenantId, config.FederationIssuer);
    await using var db = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(connection).Options);
    // Does not apply EF migrations or provision CIAM objects. Prepare schema/resources separately.
    var service = new MicrosoftIdentityMigrationService(db, verifier, TimeProvider.System);
    var migration = args[0] == "reserve"
        ? await service.ReserveAsync(config.UserId, config.TargetTenantId, config.TargetObjectId, config.ApprovalDigest)
        : await service.ChangeAsync(config.MigrationId, config.ExpectedVersion, args[0] == "rollback");
    Console.WriteLine(JsonSerializer.Serialize(new { migration.Id, migration.State, migration.Version }));
    return 0;
}
catch (Exception ex)
{
    // Graph failures can carry personal identifiers. Do not log messages, payloads or tokens.
    Console.Error.WriteLine($"QA migration denied/failed ({ex.GetType().Name}). No identity is inferred from profile/email.");
    return 1;
}

internal sealed record Settings(string Environment, string AllowedQaDatabase, string SourceTenantId,
    string TargetTenantId, string FederationIssuer, Guid UserId, string TargetObjectId,
    string ApprovalDigest, Guid MigrationId, long ExpectedVersion);
