using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

internal static class AuthHarness
{
    internal static bool WithinTimestampBounds(long? authenticatedAt, long? started, long now) =>
        authenticatedAt.HasValue && started.HasValue && now - authenticatedAt <= 300
        && authenticatedAt <= now + 30 && authenticatedAt >= started - 30 && now - started <= 600;

    public static async Task Run()
    {
        var tenant = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_TENANT");
        var api = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_AUDIENCE");
        var spa = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_SPA");
        var diagnosticSpa = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_NEGATIVE_SPA");
        var googleClient = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_GOOGLE_CLIENT_ID");
        var g3Context = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_G3_CONTEXT");
        if (g3Context is not null && (!g3Context.StartsWith('c') || !int.TryParse(g3Context[1..], out var contextNumber)
            || contextNumber is < 1 or > 99))
            throw new InvalidOperationException("Invalid isolated QA authentication context.");
        var msal = Environment.GetEnvironmentVariable("MIKOMPRI_SPIKE_MSAL_JS");
        if (!Guid.TryParse(tenant, out _) || !Guid.TryParse(api, out _) || !Guid.TryParse(spa, out _)
            || !File.Exists(msal) || Path.GetFileName(msal) != "msal-browser.min.js")
            throw new InvalidOperationException("Missing public isolated QA configuration or installed MSAL bundle.");
        var authority = $"https://mikompri010qa20261030.ciamlogin.com/{tenant}";
        var issuer = $"https://{tenant}.ciamlogin.com/{tenant}/v2.0";
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:55412");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = authority + "/v2.0";
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = issuer, ValidateAudience = true, ValidAudience = api,
                ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
            };
        });
        builder.Services.AddAuthorization(options => options.AddPolicy("QaApi", policy => policy
            .RequireAuthenticatedUser().RequireClaim("tid", tenant!)
            .RequireAssertion(context => Guid.TryParse(context.User.FindFirst("oid")?.Value, out _)
                && (context.User.FindFirst("scp")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains("access_as_user", StringComparer.Ordinal) ?? false))));
        var attempts = new ConcurrentDictionary<Guid, DateTimeOffset>();
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            await next(context);
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/", () => Results.Content(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "auth.html")), "text/html"));
        app.MapGet("/negative.html", () => Results.Content(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "auth.html")), "text/html"));
        app.MapGet("/google-direct.html", () => Results.Content(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "google-direct.html")), "text/html"));
        app.MapGet("/spike/msal.js", () => Results.File(msal!, "application/javascript"));
        app.MapGet("/spike/config", () => Results.Json(new { clientId = spa, diagnosticClientId = diagnosticSpa,
            googleClientId = googleClient, g3ContextId = g3Context, authority, scope = $"api://{api}/access_as_user" }));
        app.MapPost("/spike/start", () =>
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var old in attempts.Where(a => now - a.Value > TimeSpan.FromMinutes(10))) attempts.TryRemove(old.Key, out _);
            var id = Guid.NewGuid(); attempts[id] = now;
            return Results.Json(new { attempt = id, startedAt = now.ToUnixTimeSeconds() });
        });
        app.MapPost("/spike/claims", (HttpContext context) =>
        {
            string? Digest(string name) => context.User.FindFirst(name)?.Value is { } value
                ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant() : null;
            long? Timestamp(string name) => long.TryParse(context.User.FindFirst(name)?.Value, out var value) ? value : null;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long? started = Guid.TryParse(context.Request.Headers["X-Spike-Attempt"], out var id)
                && attempts.TryRemove(id, out var start) ? start.ToUnixTimeSeconds() : null;
            var authenticatedAt = Timestamp("auth_time");
            return Results.Json(new { kind = "REAL_QA_API_ACCESS_TOKEN_VALIDATED", accepted = true,
                issuerSha256 = Digest("iss"), tenantSha256 = Digest("tid"), objectSha256 = Digest("oid"),
                subjectSha256 = Digest("sub"), audienceSha256 = Digest("aud"), requiredScopePresent = true,
                authTime = authenticatedAt, issuedAt = Timestamp("iat"), expiresAt = Timestamp("exp"),
                configuredContextId = g3Context,
                contextClaimPresent = context.User.HasClaim(c => c.Type == "acrs"),
                configuredContextClaimPresent = g3Context is not null && context.User.HasClaim("acrs", g3Context),
                authenticationMethodClaimPresent = context.User.HasClaim(c => c.Type == "amr"),
                mfaReferencePresent = context.User.HasClaim("amr", "mfa"),
                challengeStartedAt = started, observedAt = now,
                withinG3TimestampBounds = WithinTimestampBounds(authenticatedAt, started, now),
                provesG3 = false }); // Timestamp bounds alone do not prove a fresh provider challenge.
        }).RequireAuthorization("QaApi");
        // Isolated diagnostic surface: verifies signature/aud/issuer/lifetime, reads no domain data.
        app.MapPost("/spike/scope-observation", (HttpContext context) =>
        {
            var scopes = context.User.FindFirst("scp")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
            var oid = context.User.FindFirst("oid")?.Value;
            return Results.Json(new { authenticated = true,
                tenantMatchesQa = context.User.FindFirst("tid")?.Value == tenant,
                objectIsGuid = Guid.TryParse(oid, out _),
                objectSha256 = oid is null ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(oid))).ToLowerInvariant(),
                requiredScopePresent = scopes.Contains("access_as_user", StringComparer.Ordinal),
                diagnosticScopePresent = scopes.Contains("diagnostic_only", StringComparer.Ordinal), scopeCount = scopes.Length });
        }).RequireAuthorization();
        // Negative-test diagnostics only. Does not establish an API principal or grant domain access.
        app.MapPost("/spike/google-negative-observation", async (HttpContext context) =>
        {
            if (string.IsNullOrEmpty(googleClient)) return Results.StatusCode(503);
            var header = context.Request.Headers.Authorization.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.Ordinal)) return Results.BadRequest();
            try
            {
                var manager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    "https://accounts.google.com/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                var metadata = await manager.GetConfigurationAsync(context.RequestAborted);
                var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(header[7..],
                    new TokenValidationParameters { ValidateIssuer = true, ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
                        ValidateAudience = true, ValidAudience = googleClient, ValidateLifetime = true,
                        RequireExpirationTime = true, ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                        IssuerSigningKeys = metadata.SigningKeys, ValidAlgorithms = [SecurityAlgorithms.RsaSha256] }, out _);
                var sub = principal.FindFirst("sub")?.Value;
                return Results.Json(new { googleSignatureAndClientContractValid = true,
                    subjectSha256 = sub is null ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sub))).ToLowerInvariant(),
                    authTimeClaimPresent = principal.HasClaim(c => c.Type == "auth_time"),
                    domainAccessGrantedByThisDiagnostic = false, provesG3 = false });
            }
            catch (SecurityTokenException exception)
            {
                return Results.Json(new { googleSignatureAndClientContractValid = false,
                    failureType = exception.GetType().Name, domainAccessGrantedByThisDiagnostic = false });
            }
        });
        await app.RunAsync();
    }
}
