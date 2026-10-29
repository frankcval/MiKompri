using System.Globalization;
using MiKompri.ProductCatalog.Application.Abstractions;

namespace MiKompri.ProductCatalog.Api.Services;

public sealed class ClientDateTimeProvider(TimeProvider clock, IHttpContextAccessor context) : IDateTimeProvider
{
    public DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    public DateOnly Today
    {
        get
        {
            var minutes = 0;
            if (context.HttpContext?.Request.Headers.TryGetValue("X-Client-Utc-Offset-Minutes", out var value) == true
                && (value.Count != 1 || !int.TryParse(value[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes) || minutes is < -840 or > 840))
                throw new InvalidOperationException("El offset UTC del cliente debe ser un entero entre -840 y 840 minutos.");
            return DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromMinutes(minutes)).DateTime);
        }
    }
}
