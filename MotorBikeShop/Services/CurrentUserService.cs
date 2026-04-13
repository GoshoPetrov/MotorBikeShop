namespace MotorBikeShop.Services
{
    using System.Security.Claims;

    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? UserId =>
            _contextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
