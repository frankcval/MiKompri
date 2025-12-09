using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Commands.AddMemberToGroup
{
    public class AddMemberToGroupCommandValidator : AbstractValidator<AddMemberToGroupCommand>
    {
        public AddMemberToGroupCommandValidator()
        {
            RuleFor(x => x.GroupId)
                .NotEmpty().WithMessage("El Id del grupo es obligatorio.");

            RuleFor(x => x.MemberUserId)
                .NotEmpty().WithMessage("El Id del usuario a añadir es obligatorio.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("El rol especificado no es válido.");

            // Regla opcional: normalmente no tiene sentido crear otro Owner desde aquí
            // RuleFor(x => x.Role)
            //     .Equal(GroupRole.Member).WithMessage("Solo se permiten miembros estándar desde este comando.");
        }
    }
}
