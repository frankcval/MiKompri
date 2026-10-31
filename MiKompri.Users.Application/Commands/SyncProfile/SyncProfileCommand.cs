using MediatR;

namespace MiKompri.Users.Application.Commands.SyncProfile
{
    /// <summary>
    /// Sincroniza (o crea) el perfil local a partir de los claims del IdP.
    /// Retorna el UserId interno y si el perfil fue recién creado.
    /// </summary>
    public sealed record SyncProfileCommand(
        string IdentityProvider,
        string TenantId,
        string ObjectId,
        string? ExternalUserId,
        string? DisplayName,
        string? Email,
        bool AllowProvision = true,
        bool AllowLegacyFallback = false,
        string? GivenName = null,
        string? FamilyName = null
    ) : IRequest<(Guid UserId, bool Created)>;
}
