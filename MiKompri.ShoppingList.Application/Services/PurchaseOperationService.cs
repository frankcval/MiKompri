using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Services;

public sealed class PurchaseOperationService(IPurchaseOperationStore store)
{
    public static string Fingerprint(Guid paidBy, Guid? purchasedBy, decimal amount, string? currency, IEnumerable<Guid> participants, Guid? marketId, DateOnly? date)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            paidBy, purchasedBy, amount = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            currency = (currency ?? "EUR").Trim().ToUpperInvariant(), participants = participants.OrderBy(x => x).ToArray(), marketId, date
        }))));

    public static void Validate(decimal amount, string? currency)
    {
        if (amount <= 0 || amount > 9999999999.99m || decimal.Round(amount, 2) != amount)
            throw new InvalidOperationException("Introduce un importe total positivo con un máximo de dos decimales.");
        if (!string.Equals(currency?.Trim() ?? "EUR", "EUR", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La moneda operativa es EUR.");
    }

    public async Task<Guid?> ReplayAsync(Guid? id, Guid actor, Guid list, Guid item, string hash, CancellationToken ct)
    {
        if (!id.HasValue) return null; // Legacy shared consumers remain compatible.
        if (id == Guid.Empty) throw new InvalidOperationException("La operación de compra no es válida.");
        var previous = await store.FindAsync(id.Value, ct);
        if (previous is null) return null;
        if (previous.ActorId != actor || previous.ListId != list || previous.ItemId != item || previous.RequestHash != hash)
            throw new ResourceConflictException("Esta operación ya se utilizó con otros datos. Revisa los gastos antes de continuar.");
        if (!await store.ExpenseExistsAsync(previous.ExpenseId, ct))
            throw new ResourceConflictException("El gasto de esta operación se eliminó. No se volverá a registrar automáticamente.");
        return previous.ExpenseId;
    }

    public Task RecordAsync(Guid? id, Guid actor, Guid list, Guid item, Guid expense, string hash, CancellationToken ct)
        => id.HasValue ? store.AddAsync(new PurchaseOperation(id.Value, actor, list, item, expense, hash), ct) : Task.CompletedTask;
}
