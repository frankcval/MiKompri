using MediatR;
using MiKompri.Users.Application.Dtos;

namespace MiKompri.Users.Application.Queries.GetMyProfile
{
    /// <summary>
    /// Devuelve el perfil del usuario autenticado actual.
    /// El UserId se obtiene de ICurrentUserService, no requiere parámetros.
    /// </summary>
    public sealed record GetMyProfileQuery() : IRequest<UserProfileDto>;
}
