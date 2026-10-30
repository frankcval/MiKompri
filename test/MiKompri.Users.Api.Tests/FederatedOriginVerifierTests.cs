using System.Net;
using System.Text;
using System.Text.Json;
using MiKompri.Users.Infrastructure.Identity;

namespace MiKompri.Users.Api.Tests;
public class FederatedOriginVerifierTests
{
    private const string Source = "11111111-1111-4111-8111-111111111111";
    private const string Target = "22222222-2222-4222-8222-222222222222";
    private const string SourceObject = "33333333-3333-4333-8333-333333333333";
    private const string TargetObject = "44444444-4444-4444-8444-444444444444";
    private const string Issuer = "https://login.microsoftonline.com/sourceqa.onmicrosoft.com/v2.0";

    [Theory]
    [InlineData("valid", true)]
    [InlineData("other-tenant", false)]
    [InlineData("other-object", false)]
    [InlineData("other-issuer", false)]
    [InlineData("same-email-only", false)]
    [InlineData("disabled", false)]
    [InlineData("duplicate-proof", false)]
    public async Task RequiresExactImmutableOrigin_NotProfileOrEmail(string scenario, bool accepted)
    {
        using var http = new HttpClient(new Handler(scenario)) { BaseAddress = new Uri("https://graph.microsoft.com/") };
        var verifier = new FederatedOriginVerifier(http, Source, Target, Issuer);
        if (accepted)
        {
            var digest = await verifier.VerifyAsync(Source, SourceObject, Target, TargetObject, default);
            Assert.Matches("^[a-f0-9]{64}$", digest);
        }
        else await Assert.ThrowsAsync<InvalidOperationException>(() => verifier.VerifyAsync(Source, SourceObject, Target, TargetObject, default));
    }
    private sealed class Handler(string scenario) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            object data;
            if (request.RequestUri!.AbsolutePath.EndsWith("organization")) data = new { value = new[] { new { id = scenario == "other-tenant" ? Source : Target } } };
            else
            {
                var proof = new { signInType = "federated", issuer = scenario == "other-issuer" ? "https://attacker.invalid/" : Issuer, issuerAssignedId = scenario == "other-object" ? TargetObject : SourceObject };
                data = new { id = TargetObject, accountEnabled = scenario != "disabled", mail = "same@example.invalid", identities = scenario == "same-email-only" ? [] : scenario == "duplicate-proof" ? new[] { proof, proof } : new[] { proof } };
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json") });
        }
    }
}
