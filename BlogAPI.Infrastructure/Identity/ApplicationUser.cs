using BlogAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace BlogAPI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public UserProfile? userProfile { get; set; }
}
