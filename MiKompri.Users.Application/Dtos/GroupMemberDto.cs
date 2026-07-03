using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Application.Dtos
{
    public sealed class GroupMemberDto
    {
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}
