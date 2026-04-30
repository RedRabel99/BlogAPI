using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Services;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Validators.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Slugify;

namespace BlogAPI.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IPostService, PostService>();
            services.AddSingleton<IPagedListFactory, PagedListFactory>();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>(ServiceLifetime.Transient);
            services.AddSingleton<ISlugHelper, SlugHelper>();
            return services;
        }
    }
}
