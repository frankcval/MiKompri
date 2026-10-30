using MediatR;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Commands.SyncProfile;

public sealed class SyncProfileCommandHandler(IUserRepository users)
    : IRequestHandler<SyncProfileCommand, (Guid UserId, bool Created)>
{
    public async Task<(Guid UserId, bool Created)> Handle(SyncProfileCommand request, CancellationToken ct)
    {
        var provider = Required(request.IdentityProvider, 100);
        var tid = Required(request.TenantId, 100);
        var oid = Required(request.ObjectId, 100);
        var user = await users.GetByCanonicalIdentityAsync(provider, tid, oid, ct);
        if (user is not null) return (user.Id, false); // Resolve: never sync or move identity.

        // Every retired/reserved tuple remains blocked, including sub bootstrap.
        if (await users.IsIdentityReservedAsync(provider, tid, oid, ct))
            throw new UnauthorizedAccessException("Authentication failed.");

        if (request.AllowLegacyFallback && !string.IsNullOrWhiteSpace(request.ExternalUserId))
        {
            var legacy = await users.GetByExternalIdAsync(provider, request.ExternalUserId, ct);
            if (legacy is not null)
            {
                if (legacy.TenantId is not null || legacy.ObjectId is not null)
                    throw new UnauthorizedAccessException("Authentication failed.");
                legacy.AssignCanonicalIdentity(tid, oid);
                await users.UpdateAsync(legacy, ct);
                return (legacy.Id, false);
            }
        }

        if (!request.AllowProvision) throw new UnauthorizedAccessException("Authentication failed.");
        // Provision only. No correlation by email/name/sub or provider-specific identifier.
        user = new User(string.IsNullOrWhiteSpace(request.DisplayName) ? "Usuario" : Trim(request.DisplayName, 100)!,
            Trim(request.Email, 200), provider, tid, oid,
            provider == "entra" ? Trim(request.ExternalUserId, 200) : null);
        user.InitializeNames(Trim(request.GivenName, 100), Trim(request.FamilyName, 100));
        try
        {
            await users.AddAsync(user, ct);
            return (user.Id, true);
        }
        catch (UserConflictException)
        {
            // Only a unique canonical insert race may converge. Never adopt by legacy sub.
            var concurrent = await users.GetByCanonicalIdentityAsync(provider, tid, oid, ct);
            if (concurrent is not null) return (concurrent.Id, false);
            throw;
        }
    }

    private static string Required(string value, int length) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Length > length
            ? throw new UnauthorizedAccessException("Authentication failed.") : value.Trim();
    private static string? Trim(string? value, int length) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, length)];
}
