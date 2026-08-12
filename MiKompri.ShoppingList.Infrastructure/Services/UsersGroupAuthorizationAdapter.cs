using MiKompri.ShoppingList.Application.Interfaces;

namespace MiKompri.ShoppingList.Infrastructure.Services
{
    // Adapter placeholder: reemplazar la lógica interna por integración efectiva con Users API
    // sin acceso directo a BD de Users.
    public class UsersGroupAuthorizationAdapter : IGroupAuthorizationService
    {
        public Task<GroupAuthorizationResult> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        {
            // Implementación conservadora por defecto: no autorizado.
            // Esto evita abrir acceso por error hasta integrar el contrato real.
            return Task.FromResult(new GroupAuthorizationResult(false, false, null));
        }
    }
}
