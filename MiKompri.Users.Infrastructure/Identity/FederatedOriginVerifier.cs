using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace MiKompri.Users.Infrastructure.Identity;
public interface IFederatedOriginVerifier
{
    Task<string> VerifyAsync(string sourceTenant, string sourceObject, string targetTenant, string targetObject, CancellationToken ct);
}

// Used only by the administrative migration runner. Never registered in business requests.
public sealed class FederatedOriginVerifier(HttpClient graph, string trustedSourceTenant,
    string trustedTargetTenant, string trustedFederationIssuer) : IFederatedOriginVerifier
{
    public async Task<string> VerifyAsync(string sourceTenant, string sourceObject, string targetTenant, string targetObject, CancellationToken ct)
    {
        if (graph.BaseAddress != new Uri("https://graph.microsoft.com/") || sourceTenant != trustedSourceTenant || targetTenant != trustedTargetTenant ||
            !Guid.TryParse(sourceObject, out _) || !Guid.TryParse(targetObject, out _) || string.IsNullOrWhiteSpace(trustedFederationIssuer))
            throw new InvalidOperationException("Origen federado no acreditado.");
        using var org = await graph.GetFromJsonAsync<JsonDocument>("v1.0/organization?$select=id", ct);
        var organizations = org!.RootElement.GetProperty("value");
        if (organizations.GetArrayLength() != 1 || organizations[0].GetProperty("id").GetString() != targetTenant)
            throw new InvalidOperationException("Tenant Graph no autorizado.");
        using var target = await graph.GetFromJsonAsync<JsonDocument>($"v1.0/users/{targetObject}?$select=id,identities,accountEnabled", ct);
        var root = target!.RootElement;
        if (root.GetProperty("id").GetString() != targetObject || !root.GetProperty("accountEnabled").GetBoolean())
            throw new InvalidOperationException("Destino no acreditado.");
        var matching = root.GetProperty("identities").EnumerateArray().Count(x =>
            x.GetProperty("signInType").GetString() == "federated" &&
            x.GetProperty("issuer").GetString() == trustedFederationIssuer &&
            x.GetProperty("issuerAssignedId").GetString() == sourceObject);
        if (matching != 1) throw new InvalidOperationException("El objeto CIAM no acredita el origen Microsoft reservado.");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{sourceTenant}\0{sourceObject}\0{targetTenant}\0{targetObject}\0{trustedFederationIssuer}"))).ToLowerInvariant();
    }
}
