namespace MiKompri.ShoppingList.Application.Tests.IntegrationTest.CrossService
{
    /// <summary>
    /// Handler que intercepta las llamadas salientes de ShoppingList hacia "UsersApi"
    /// y las reenvía al servidor de test real de Users.Api.
    /// </summary>
    public sealed class CrossServiceForwardingHandler : HttpMessageHandler
    {
        private readonly HttpClient _usersApiClient;

        public CrossServiceForwardingHandler(HttpClient usersApiClient)
        {
            _usersApiClient = usersApiClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Construir nueva petición con la ruta relativa hacia el servidor de test de Users
            var relativeUri = request.RequestUri?.PathAndQuery
                              ?? throw new InvalidOperationException("RequestUri is null.");

            var forwarded = new HttpRequestMessage(request.Method, relativeUri);

            // Reenviar headers (incluida Authorization)
            foreach (var header in request.Headers)
                forwarded.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content is not null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                forwarded.Content = new ByteArrayContent(contentBytes);
                foreach (var header in request.Content.Headers)
                    forwarded.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return await _usersApiClient.SendAsync(forwarded, cancellationToken);
        }
    }
}
