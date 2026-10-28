using MiKompri.ShoppingList.Application.Exceptions;
using MiKompri.ShoppingList.Application.Interfaces;
using MiKompri.ShoppingList.Domain.Entities;

namespace MiKompri.ShoppingList.Application.Policies;

public sealed class PurchaseAccessPolicy(IPurchaseListRepository repository, ICurrentUserService user, IGroupAuthorizationService groups)
{
    public Guid ActorId => user.UserId;

    public void RequireIdentity()
    {
        if (!user.IsAuthenticated || user.UserId == Guid.Empty) throw new UnauthorizedAccessException("Inicia sesión para continuar.");
    }

    public async Task<GroupRole?> RequireGroup(Guid groupId, CancellationToken ct)
    {
        RequireIdentity();
        var membership = await groups.GetMembershipAsync(groupId, user.UserId, ct);
        if (!membership.IsAuthorizedMember || membership.Role is null)
            throw new ForbiddenOperationException("No tienes acceso a este grupo.");
        return membership.Role;
    }

    public async Task<PurchaseList> RequireList(Guid id, bool write, bool manage, Guid? ownItem, CancellationToken ct)
    {
        RequireIdentity();
        var list = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("La lista no está disponible.");
        if (!list.GroupId.HasValue)
        {
            if (list.OwnerId != user.UserId) throw new ForbiddenOperationException("No tienes acceso a esta lista.");
        }
        else
        {
            var role = await RequireGroup(list.GroupId.Value, ct);
            if (manage && role is not (GroupRole.Owner or GroupRole.Admin))
                throw new ForbiddenOperationException("Solo las personas administradoras pueden gestionar esta lista.");
            if (ownItem.HasValue && role == GroupRole.Member)
            {
                var item = list.Items.SingleOrDefault(i => i.Id == ownItem) ?? throw new KeyNotFoundException("El producto no está disponible.");
                if (item.AddedBy != user.UserId) throw new ForbiddenOperationException("Solo puedes editar o eliminar los productos que has añadido.");
            }
            if (write && list.Status != SharedListStatus.Active)
                throw new ResourceConflictException("La lista está cerrada. Puedes consultar su compra y sus gastos.");
        }
        return list;
    }
}
