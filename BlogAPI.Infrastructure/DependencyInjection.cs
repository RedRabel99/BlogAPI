using BlogAPI.Domain.Interfaces;
using BlogAPI.Infrastructure.Identity;
using BlogAPI.Infrastructure.Repositories;
using BlogAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => {
            options.UseInMemoryDatabase("AppDb");
        });
        services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequiredLength = 6;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireUppercase = true;
            opt.Password.RequireLowercase = true;

            opt.User.RequireUniqueEmail = true;

        }
        ).AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<IUserManager, IdentityUserManager>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddHttpContextAccessor();
        return services;
    }
}
