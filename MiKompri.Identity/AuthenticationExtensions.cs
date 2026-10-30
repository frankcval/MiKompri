using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiKompri.Identity;
public static class AuthenticationExtensions
{
    public const string ProviderItem = "MiKompri.ValidatedIdentityProvider";
    public const string RouteItem = "MiKompri.ValidatedTrustRoute";
    public static IServiceCollection AddMiKompriJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var routes = new Lazy<TrustRoute[]>(() => ReadRoutes(configuration));
        var authentication = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddPolicyScheme(JwtBearerDefaults.AuthenticationScheme, "MiKompri issuer selection", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // Untrusted issuer only chooses a configured handler; it never grants trust.
                    var bearer = context.Request.Headers.Authorization.ToString();
                    try
                    {
                        if (bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && bearer.Length < 32768)
                        {
                            var issuer = new JwtSecurityTokenHandler().ReadJwtToken(bearer[7..]).Issuer;
                            return "MiKompri-" + (routes.Value.FirstOrDefault(r => r.Issuers.Contains(issuer, StringComparer.Ordinal))?.Name ?? "legacy");
                        }
                    }
                    catch (Exception ex) when (ex is ArgumentException or Microsoft.IdentityModel.Tokens.SecurityTokenException or FormatException or System.Text.Json.JsonException) { }
                    return "MiKompri-legacy";
                };
            });
        foreach (var name in new[] { "legacy", "ciam" })
        {
            authentication.AddJwtBearer("MiKompri-" + name, options =>
            {
                var route = routes.Value.FirstOrDefault(value => value.Name == name);
                if (route is null)
                {
                    options.Events = new JwtBearerEvents { OnMessageReceived = context => { context.NoResult(); return Task.CompletedTask; } };
                    return; // Disabled CIAM never participates in issuer selection.
                }
                options.MapInboundClaims = false;
                options.Authority = route.Authority;
                options.TokenValidationParameters = route.BuildParameters();
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        try
                        {
                            route.ValidatePrincipal(context.Principal!);
                            context.HttpContext.Items[ProviderItem] = route.IdentityProvider;
                            context.HttpContext.Items[RouteItem] = route;
                        }
                        catch (InvalidScopeException)
                        {
                            context.HttpContext.Items["MiKompri.ScopeFailure"] = true;
                            context.Fail("Invalid delegated scope.");
                        }
                        catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException) { context.Fail("Authentication failed."); }
                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        var status = context.HttpContext.Items.ContainsKey("MiKompri.ScopeFailure") ? 403 : 401;
                        context.Response.StatusCode = status;
                        if (status == 401) context.Response.Headers.WWWAuthenticate = "Bearer";
                        await context.Response.WriteAsJsonAsync(new { status, error = "Authentication failed.", traceId = context.HttpContext.TraceIdentifier });
                    }
                };
            });
        }
        return services;
    }

    private static TrustRoute[] ReadRoutes(IConfiguration configuration)
    {
        var legacyAuthority = configuration["Authentication:Authority"] ?? throw new InvalidOperationException("Authentication:Authority required.");
        var legacyTenant = configuration["Authentication:TenantId"] ?? new Uri(legacyAuthority).Segments.Skip(1).FirstOrDefault()?.Trim('/') ?? "";
        var legacyIssuers = new List<string> { legacyAuthority.TrimEnd('/'), legacyAuthority.TrimEnd('/') + "/" };
        if (new Uri(legacyAuthority).Host == "login.microsoftonline.com" && Guid.TryParse(legacyTenant, out _))
            legacyIssuers.Add($"https://sts.windows.net/{legacyTenant}/");
        var legacyAudiences = Split(configuration["Authentication:ValidAudiences"]).ToList();
        var audience = configuration["Authentication:Audience"];
        if (!string.IsNullOrWhiteSpace(audience))
        {
            legacyAudiences.Add(audience);
            if (audience.StartsWith("api://", StringComparison.Ordinal)) legacyAudiences.Add(audience[6..]);
        }
        var routes = new List<TrustRoute> { new("legacy", "entra", legacyAuthority, legacyTenant,
            legacyIssuers.Distinct().ToArray(), legacyAudiences.Distinct().ToArray(),
            configuration["Authentication:Scope"] ?? "access_as_user", Split(configuration["Authentication:ClientIds"])) };
        var external = configuration.GetSection("Authentication:ExternalId");
        if (external.GetValue<bool>("Enabled"))
        {
            string Required(string name) => external[name] is { Length: > 0 } value ? value : throw new InvalidOperationException($"ExternalId:{name} required.");
            var authority = Required("Authority"); var issuer = Required("Issuer");
            if (new Uri(authority).Scheme != "https" || new Uri(issuer).Scheme != "https" || !Guid.TryParse(Required("TenantId"), out _))
                throw new InvalidOperationException("ExternalId trust must be explicitly configured.");
            var clients = Split(Required("ClientIds"));
            if (clients.Length == 0 || Split(Required("Audience")).Length == 0)
                throw new InvalidOperationException("ExternalId audience and clients cannot be empty.");
            routes.Add(new("ciam", "ciam", authority, Required("TenantId"), [issuer],
                Split(Required("Audience")), external["Scope"] ?? "access_as_user", clients));
        }
        return routes.ToArray();
    }

    public static string[] Split(string? value) => value?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
}
