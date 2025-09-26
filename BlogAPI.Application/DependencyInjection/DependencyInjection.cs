using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
