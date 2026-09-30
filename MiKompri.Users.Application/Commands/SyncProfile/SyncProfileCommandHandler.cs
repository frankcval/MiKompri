using MediatR;
using MiKompri.Users.Domain.Users;

namespace MiKompri.Users.Application.Commands.SyncProfile
{
	public class SyncProfileCommandHandler
		: IRequestHandler<SyncProfileCommand, (Guid UserId, bool Created)>
	{
		private readonly IUserRepository _userRepository;

		public SyncProfileCommandHandler(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<(Guid UserId, bool Created)> Handle(
			SyncProfileCommand request,
			CancellationToken cancellationToken)
		{
			var hasCanonical = !string.IsNullOrWhiteSpace(request.TenantId)
				&& !string.IsNullOrWhiteSpace(request.ObjectId);
			var hasSub = !string.IsNullOrWhiteSpace(request.ExternalUserId);

			User? user = null;
			var associated = false;

			if (hasCanonical)
			{
				// Fase 1 (lazy): 1) por (tid, oid)
				user = await _userRepository.GetByCanonicalIdentityAsync(
					request.TenantId!, request.ObjectId!, cancellationToken);

				// 2) si no, por sub legacy y asociar (tid, oid) al mismo UserId
				if (user is null && hasSub)
				{
					user = await _userRepository.GetByExternalIdAsync(
						request.IdentityProvider, request.ExternalUserId, cancellationToken);

					if (user is not null)
						associated = user.AssociateCanonicalIdentity(request.TenantId!, request.ObjectId!);
				}
			}
			else
			{
				user = await _userRepository.GetByExternalIdAsync(
					request.IdentityProvider,
					request.ExternalUserId,
					cancellationToken);
			}

			if (user is null)
			{
				// C4: email null si ausente, nunca cadena vacia
				var email = string.IsNullOrEmpty(request.Email) ? null : request.Email;

				// 3) usuario nuevo: solo con (tid, oid) y ExternalUserId = null
				user = hasCanonical
					? User.CreateFromCanonicalIdentity(
						request.DisplayName ?? string.Empty,
						email,
						request.IdentityProvider,
						request.TenantId!,
						request.ObjectId!)
					: new User(
						request.DisplayName ?? string.Empty,
						email,
						request.IdentityProvider,
						request.ExternalUserId);

				await _userRepository.AddAsync(user, cancellationToken);
				return (user.Id, Created: true);
			}

			// C5: guardar solo si hubo un cambio real (idempotencia)
			var previousUpdatedAt = user.UpdatedAt;
			user.SyncClaims(request.DisplayName, request.Email);

			if (associated || user.UpdatedAt != previousUpdatedAt)
				await _userRepository.UpdateAsync(user, cancellationToken);

			return (user.Id, Created: false);
		}
	}
}