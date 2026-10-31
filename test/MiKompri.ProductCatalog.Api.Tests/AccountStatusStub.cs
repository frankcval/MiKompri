using System.Net;
using System.Net.Http.Json;
using MiKompri.Identity;

namespace MiKompri.ProductCatalog.Api.Tests;
public sealed class AccountStatusStub
{
    public int Calls { get; set; }
    public int HttpStatus { get; set; } = 200;
    public bool Suspended { get; set; }
    public bool ThrowNetwork { get; set; }
    public bool Malformed { get; set; }
    public sealed class Handler(AccountStatusStub state) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            state.Calls++;
            Assert.Equal("/api/v1/users/me/access-status", request.RequestUri!.AbsolutePath);
            Assert.NotNull(request.Headers.Authorization);
            if (state.ThrowNetwork) throw new HttpRequestException("Synthetic outage");
            var response = new HttpResponseMessage((HttpStatusCode)state.HttpStatus);
            response.Content = state.Malformed ? new StringContent("{}") : JsonContent.Create(new AccessStatus(Guid.Parse("11111111-1111-1111-1111-111111111111"), state.Suspended, 0, null));
            return Task.FromResult(response);
        }
    }
}
