using BlogAPI.Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogAPI.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserContext(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public bool IsAuthenticated => 
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string UserId => 
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public async Task<IUserInfo> GetCurrentUserAsync()
        {
            if (!IsAuthenticated) return null;

            var user = await _userManager.FindByIdAsync(UserId);
            return user != null ? new UserInfoAdapter(user) : null;
        }
    }
}
