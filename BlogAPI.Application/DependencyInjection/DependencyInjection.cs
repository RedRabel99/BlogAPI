using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Services;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Validators.Auth;
using BlogAPI.Application.Validators.UserProfile;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<ITagService, TagService>();
            services.AddSingleton<IPagedListFactory, PagedListFactory>();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>(ServiceLifetime.Transient);
            return services;
        }
    }
}
