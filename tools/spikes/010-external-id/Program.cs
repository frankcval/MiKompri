using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MiKompri.Users.Domain.Users;
using MiKompri.Users.Infrastructure.Persistence;

// Isolated spike: not referenced by the solution, production API or Web.
if (args is ["jwt-controls"])
{
    JwtControls();
    return;
}
if (args is ["benchmark"])
{
    await Benchmark();
    return;
}
if (args is ["validate-external-token"])
{
    await ValidateExternalToken();
    return;
}
if (args is ["auth-harness"])
{
    await AuthHarness.Run();
    return;
}
if (args is ["g3-timing-controls"])
{
    const long now = 10000;
    var cases = new (string Name, long? Auth, long? Start, bool Expected)[]
    {
        ("age_exactly_300", now-300, now-300, true),
        ("age_301_rejected_without_extending_by_skew", now-301, now-301, false),
        ("operation_exactly_600", now-300, now-600, true),
        ("operation_601_rejected", now-300, now-601, false),
        ("future_30_within_skew", now+30, now, true),
        ("future_31_rejected", now+31, now, false),
        ("before_challenge_30_within_skew", now-30, now, true),
        ("before_challenge_31_rejected", now-31, now, false),
        ("missing_auth_time_rejected", null, now, false),
        ("missing_challenge_rejected", now, null, false)
    };
    var results = cases.Select(c => new { c.Name, c.Expected, Accepted = AuthHarness.WithinTimestampBounds(c.Auth,c.Start,now) }).ToArray();
    Emit(new { Kind = "OFFLINE_TIMESTAMP_BOUNDARIES_NOT_PROVIDER_G3_EVIDENCE", Cases = results,
        Passed = results.Count(r => r.Expected == r.Accepted), Total = results.Length });
    if (results.Any(r => r.Expected != r.Accepted)) Environment.ExitCode = 1;
    return;
}
Console.Error.WriteLine("Usage: ExternalIdSpike jwt-controls | g3-timing-controls | benchmark | auth-harness | validate-external-token (stdin)");
Environment.ExitCode = 2;

static void Emit(object value) => Console.WriteLine(JsonSerializer.Serialize(value,
    new JsonSerializerOptions { WriteIndented = true }));

static async Task ValidateExternalToken()
{
    // Token arrives in process memory/stdin, never in arguments, a file, logs or output.
    var tenant = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_TENANT");
    var audience = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_AUDIENCE");
    if (!Guid.TryParse(tenant, out _) || !Guid.TryParse(audience, out _))
        throw new InvalidOperationException("Missing public QA tenant/resource configuration.");
    var authority = $"https://mikompri010qa20261030.ciamlogin.com/{tenant}/v2.0";
    var manager = new ConfigurationManager<OpenIdConnectConfiguration>(
        authority + "/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
    var metadata = await manager.GetConfigurationAsync(CancellationToken.None);
    // Observed QA metadata uses the tenant GUID as issuer host, not the discovery subdomain.
    var expectedIssuer = $"https://{tenant}.ciamlogin.com/{tenant}/v2.0";
    if (metadata.Issuer.TrimEnd('/') != expectedIssuer)
        throw new InvalidOperationException("QA metadata issuer mismatch; refusing token validation.");
    var token = (await Console.In.ReadToEndAsync()).Trim();
    try
    {
        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token,
            new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = metadata.Issuer,
                ValidateAudience = true, ValidAudience = audience,
                ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                IssuerSigningKeys = metadata.SigningKeys, ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
            }, out _);
        if (principal.FindFirst("tid")?.Value != tenant
            || !Guid.TryParse(principal.FindFirst("oid")?.Value, out _)
            || !(principal.FindFirst("scp")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("access_as_user", StringComparer.Ordinal) ?? false))
            throw new SecurityTokenValidationException("Required QA tenant/object/scope contract not satisfied.");
        string? Digest(string claim) => principal.FindFirst(claim)?.Value is { } value
            ? Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant() : null;
        long? Timestamp(string claim) => long.TryParse(principal.FindFirst(claim)?.Value, out var value) ? value : null;
        Emit(new { Kind = "REAL_ISSUED_TOKEN_VALIDATION", Accepted = true,
            IssuerSha256 = Digest("iss"), TenantSha256 = Digest("tid"), ObjectSha256 = Digest("oid"),
            SubjectSha256 = Digest("sub"), AudienceSha256 = Digest("aud"), RequiredScopePresent = true,
            AuthTime = Timestamp("auth_time"), IssuedAt = Timestamp("iat"), ExpiresAt = Timestamp("exp"),
            ProvesG3 = false });
    }
    catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
    {
        // Exception.Message may contain JWT data/identifiers. Emit only a type, never message/stack.
        Emit(new { Kind = "REAL_ISSUED_TOKEN_VALIDATION", Accepted = false,
            FailureType = exception.GetType().Name, ProvesG3 = false });
        Environment.ExitCode = 1;
    }
}

static void JwtControls()
{
    const string issuer = "https://issuer.spike.invalid/tenant/v2.0";
    const string audience = "11111111-1111-4111-8111-111111111111";
    const string tenant = "22222222-2222-4222-8222-222222222222";
    const string subject = "33333333-3333-4333-8333-333333333333";
    using var rsa = RSA.Create(2048);
    using var otherRsa = RSA.Create(2048);
    var key = new RsaSecurityKey(rsa);
    var now = DateTime.UtcNow;
    var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
    var parameters = new TokenValidationParameters
    {
        ValidIssuer = issuer, ValidAudience = audience, IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ValidateIssuerSigningKey = true, RequireSignedTokens = true,
        RequireExpirationTime = true, ClockSkew = TimeSpan.Zero,
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
    };
    string Token(string iss = issuer, string aud = audience, string? scp = "access_as_user",
        string? tid = tenant, string? oid = subject, bool expired = false, bool wrongKey = false)
    {
        var claims = new List<Claim> { new("sub", subject) };
        if (scp is not null) claims.Add(new("scp", scp));
        if (tid is not null) claims.Add(new("tid", tid));
        if (oid is not null) claims.Add(new("oid", oid));
        return handler.WriteToken(new JwtSecurityToken(iss, aud, claims,
            now.AddMinutes(-10), expired ? now.AddMinutes(-1) : now.AddMinutes(10),
            new SigningCredentials(wrongKey ? new RsaSecurityKey(otherRsa) : key, SecurityAlgorithms.RsaSha256)));
    }
    bool Accept(string token, TokenValidationParameters policy, bool strict)
    {
        try
        {
            var principal = handler.ValidateToken(token, policy, out _);
            return !strict || (principal.FindFirst("tid")?.Value == tenant
                && Guid.TryParse(principal.FindFirst("oid")?.Value, out _)
                && (principal.FindFirst("scp")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains("access_as_user", StringComparer.Ordinal) ?? false));
        }
        catch (SecurityTokenException) { return false; }
    }
    var cases = new (string Name, string Token, bool Expected)[]
    {
        ("valid_synthetic_access", Token(), true),
        ("google_issuer_synthetic", Token(iss: "https://accounts.google.com"), false),
        ("graph_audience_synthetic", Token(aud: "00000003-0000-0000-c000-000000000000"), false),
        ("id_token_spa_audience_synthetic", Token(aud: "44444444-4444-4444-8444-444444444444", scp: null), false),
        ("wrong_audience", Token(aud: "wrong-resource"), false),
        ("wrong_scope", Token(scp: "other_scope"), false),
        ("missing_scope", Token(scp: null), false),
        ("missing_tid", Token(tid: null), false),
        ("wrong_tid", Token(tid: "55555555-5555-4555-8555-555555555555"), false),
        ("missing_oid", Token(oid: null), false),
        ("expired", Token(expired: true), false),
        ("wrong_signature", Token(wrongKey: true), false)
    };
    var results = cases.Select(c => new { c.Name, Expected = c.Expected,
        Accepted = Accept(c.Token, parameters, true) }).ToArray();
    var existing = new[]
    {
        ("Users", MiKompri.Users.Api.Security.UsersJwtValidation.BuildTokenValidationParameters(issuer, audience, null)),
        ("ShoppingList", MiKompri.ShoppingList.Api.Security.ShoppingListJwtValidation.BuildTokenValidationParameters(issuer, audience, null)),
        ("ProductCatalog", MiKompri.ProductCatalog.Api.Security.ProductCatalogJwtValidation.BuildTokenValidationParameters(issuer, audience, null))
    }.Select(pair =>
    {
        pair.Item2.IssuerSigningKey = key;
        pair.Item2.ClockSkew = TimeSpan.Zero;
        return new { Api = pair.Item1,
            AcceptsWrongScope = Accept(Token(scp: "other_scope"), pair.Item2, false),
            AcceptsNoScope = Accept(Token(scp: null), pair.Item2, false),
            AcceptsNoTidOid = Accept(Token(tid: null, oid: null), pair.Item2, false) };
    }).ToArray();
    Emit(new { Kind = "SYNTHETIC_CONTROLS_NOT_EXTERNAL_ID_EVIDENCE", Cases = results,
        ExistingJwtHelpersOnly = existing, Passed = results.Count(r => r.Expected == r.Accepted), Total = results.Length });
    if (results.Any(r => r.Expected != r.Accepted)) Environment.ExitCode = 1;
}

static async Task Benchmark()
{
    var connection = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_PG")
        ?? throw new InvalidOperationException("Missing isolated QA connection environment variable.");
    var cs = new Npgsql.NpgsqlConnectionStringBuilder(connection);
    // Refuse accidental access to any existing MiKompri database or remote host.
    if (cs.Host != "127.0.0.1" || cs.Port != 55410 || cs.Database != "mikompri_010_spike")
        throw new InvalidOperationException("Only the documented isolated local QA database is permitted.");
    var options = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(connection).Options;
    await using (var seed = new UsersDbContext(options))
    {
        // Creates current Users model on a NEW disposable database; no EF migration or production change.
        if (!await seed.Database.EnsureCreatedAsync())
            throw new InvalidOperationException("QA database is not empty; refusing to reuse it.");
        seed.Users.AddRange(Enumerable.Range(0, 10000).Select(i =>
            new User("Synthetic QA", null, "spike", "qa-tenant", $"qa-object-{i}", null)));
        await seed.SaveChangesAsync();
        await seed.Database.ExecuteSqlRawAsync("ANALYZE \"Users\"");
    }
    async Task<bool> Lookup()
    {
        await using var db = new UsersDbContext(options);
        return await db.Users.AsNoTracking().Where(u => u.IdentityProvider == "spike"
            && u.TenantId == "qa-tenant" && u.ObjectId == "qa-object-5000")
            .Select(u => u.Id).SingleOrDefaultAsync() != Guid.Empty;
    }
    var builder = WebApplication.CreateBuilder(Array.Empty<string>());
    builder.Logging.ClearProviders(); // No headers, SQL parameters or tokens logged.
    builder.WebHost.UseUrls("http://127.0.0.1:55411");
    var app = builder.Build();
    using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:55411"), Timeout = TimeSpan.FromSeconds(1) };
    app.MapGet("/spike/baseline", () => Results.Ok(new { allowed = true }));
    app.MapGet("/spike/lookup", async () => await Lookup()
        ? Results.Ok(new { allowed = true }) : Results.StatusCode(403));
    app.MapGet("/spike/consumer", async () =>
    {
        // One extra loopback HTTP hop plus one actual Users index lookup; not production /access-status.
        using var response = await client.GetAsync("/spike/lookup");
        return response.IsSuccessStatusCode ? Results.Ok(new { allowed = true }) : Results.StatusCode(503);
    });
    await app.StartAsync();
    try
    {
        async Task Http(string path)
        {
            using var result = await client.GetAsync(path);
            result.EnsureSuccessStatusCode();
        }
        async Task<object> Measure(string name, Func<Task> operation, int concurrency)
        {
            for (var i = 0; i < 100; i++) await operation();
            const int samples = 1000;
            var timings = new double[samples];
            var wall = Stopwatch.StartNew();
            await Parallel.ForEachAsync(Enumerable.Range(0, samples),
                new ParallelOptions { MaxDegreeOfParallelism = concurrency }, async (i, _) =>
                {
                    var timer = Stopwatch.StartNew();
                    await operation();
                    timings[i] = timer.Elapsed.TotalMilliseconds;
                });
            wall.Stop();
            Array.Sort(timings);
            return new { Name = name, Samples = samples, Warmup = 100, Concurrency = concurrency,
                P50Ms = Math.Round(timings[499], 3), P95Ms = Math.Round(timings[949], 3),
                P99Ms = Math.Round(timings[989], 3), MeanMs = Math.Round(timings.Average(), 3),
                MaxMs = Math.Round(timings[^1], 3), WallMs = Math.Round(wall.Elapsed.TotalMilliseconds, 3) };
        }
        var measures = new List<object>();
        foreach (var concurrency in new[] { 1, 8 })
        {
            measures.Add(await Measure("users_canonical_lookup_ef", async () => { if (!await Lookup()) throw new Exception("Missing fixture."); }, concurrency));
            measures.Add(await Measure("http_baseline", () => Http("/spike/baseline"), concurrency));
            measures.Add(await Measure("http_users_lookup", () => Http("/spike/lookup"), concurrency));
            measures.Add(await Measure("http_consumer_extra_users_hop", () => Http("/spike/consumer"), concurrency));
        }
        await using var explain = new Npgsql.NpgsqlConnection(connection);
        await explain.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand("EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) SELECT \"Id\" FROM \"Users\" WHERE \"IdentityProvider\"='spike' AND \"TenantId\"='qa-tenant' AND \"ObjectId\"='qa-object-5000'", explain);
        using var plan = JsonDocument.Parse((string)(await command.ExecuteScalarAsync())!);
        var root = plan.RootElement[0];
        var node = root.GetProperty("Plan");
        Emit(new { Kind = "LOCAL_PROXY_NOT_PRODUCTION_D1_ACCEPTANCE", FixtureRows = 10000,
            Runtime = Environment.Version.ToString(), Measures = measures,
            PostgreSql = new { NodeType = node.GetProperty("Node Type").GetString(),
                PlanningMs = root.GetProperty("Planning Time").GetDouble(), ExecutionMs = root.GetProperty("Execution Time").GetDouble(),
                ActualRows = node.GetProperty("Actual Rows").GetDouble(), SharedHitBlocks = node.GetProperty("Shared Hit Blocks").GetInt32() } });
    }
    finally { await app.StopAsync(); await app.DisposeAsync(); }
}
