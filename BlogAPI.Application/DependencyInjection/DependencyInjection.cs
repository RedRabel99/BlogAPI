using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Auth;
using BlogAPI.Application.UserProfiles;
using BlogAPI.Application.Posts;
using BlogAPI.Application.Services;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Application.Auth.Validators;
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
            services.AddScoped<ICommentService, CommentService>();
            services.AddSingleton<IPagedListFactory, PagedListFactory>();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>(ServiceLifetime.Transient);
            services.AddSingleton<ISlugHelper, SlugHelper>();
            return services;
        }
    }
}
