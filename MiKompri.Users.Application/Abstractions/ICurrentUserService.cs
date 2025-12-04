using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Abstractions
{
    //¿Quién es el usuario autenticado que está haciendo ESTA petición? Dame el UserId del usuario actual. No quiero saber cómo lo obtienes
    //ICurrentUserService sirve para obtener el UserId interno de MiKompri del usuario autenticado SIN acoplar Application ni Domain a JWT, HTTP, Claims o IdP.
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
