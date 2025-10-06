using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using BlogAPI.Infrastructure.Identity;
using BlogAPI.Infrastructure.Repositories;
using BlogAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAPI.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
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
