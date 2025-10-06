using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogAPI.Infrastructure;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserProfile>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.UserProfile)
            .HasForeignKey(e => e.UserProfileId)
            .IsRequired();
        builder.Entity<UserProfile>()
            .HasMany(e => e.Comments)
            .WithOne(e => e.UserProfile)
            .HasForeignKey(e => e.UserProfileId)
            .IsRequired();
        builder.Entity<Post>()
            .HasMany(e => e.Comments)
            .WithOne(e => e.Post)
            .HasForeignKey(e => e.PostId)
            .IsRequired();
        builder.Entity<Post>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.Posts)
            .UsingEntity("PostTag");

        
    }

}
