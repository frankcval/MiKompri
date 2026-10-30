using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MiKompri.Users.Infrastructure.Persistence;
internal static class IdentityTupleLock
{
    public static async Task AcquireAsync(UsersDbContext db, CancellationToken ct, params (string Provider, string Tenant, string Object)[] tuples)
    {
        if (db.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL") return;
        // Shared by reservations/cutover/provisioning. Lock held until transaction commit.
        var keys = tuples.Select(t => BinaryPrimitives.ReadInt64BigEndian(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{t.Provider}\0{t.Tenant}\0{t.Object}")))).Distinct().Order();
        foreach (var key in keys) await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", ct);
    }
}
