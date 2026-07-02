using MiKompri.Users.Application.Abstractions;

namespace MiKompri.Users.Api.Services
{
    public class HttpCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUserService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid UserId => _accessor.HttpContext?.Items["UserId"] is Guid id ? id : Guid.Empty;

        public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}
