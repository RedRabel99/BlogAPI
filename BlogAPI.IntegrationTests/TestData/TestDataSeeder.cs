using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure;
using BlogAPI.Infrastructure.Identity;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.IntegrationTests.TestData;

public class TestDataSeeder
{
    private readonly AppDbContext _appDbContext;
    private readonly IUserManager _userManager;
    private readonly Faker _faker;

    private readonly List<UserProfile> _userProfiles = new();
    private readonly List<ApplicationUser> _applicationUsers = new();
    private readonly List<Tag> _tags = new();
    private readonly List<Post> _posts = new();
    private readonly List<Comment> _comments = new();

    public TestDataSeeder(AppDbContext appDbContext, IUserManager userManager)
    {
        _appDbContext = appDbContext;
        _userManager = userManager;
        _faker = new Faker();
    }

    public async Task SeedAsync()
    {
        await SeedUsers();
        await SeedTags();
        await SeedPosts();
        await _appDbContext.SaveChangesAsync();
        await SeedComments();
        await _appDbContext.SaveChangesAsync();
    }

    private async Task SeedComments()
    {
        var post0 = _posts[0];
        var user0Profile = GetApplicationUser(0).UserProfile;
        var user1Profile = GetApplicationUser(1).UserProfile;

        var definitions = new[]
        {
            new { Content = "First seeded comment by user 0", PostId = post0.Id, AuthorId = user0Profile.Id },
            new { Content = "Second seeded comment by user 1", PostId = post0.Id, AuthorId = user1Profile.Id }
        };

        foreach (var def in definitions)
        {
            var existing = await _appDbContext.Comments
                .FirstOrDefaultAsync(c => c.PostId == def.PostId && c.Content == def.Content);

            if (existing is not null)
            {
                _comments.Add(existing);
                continue;
            }

            var comment = new Comment
            {
                Content = def.Content,
                PostId = def.PostId,
                UserProfileId = def.AuthorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _appDbContext.Comments.Add(comment);
            _comments.Add(comment);
        }
    }

    private async Task SeedTags()
    {
        var tagNames = new[] { "dotnet", "csharp", "aspnet" };

        foreach (var tagName in tagNames)
        {
            var existing = await _appDbContext.Tags
                .FirstOrDefaultAsync(t => t.TagName == tagName);

            if (existing is not null)
            {
                _tags.Add(existing);
                continue;
            }

            var tag = new Tag { TagName = tagName };
            _appDbContext.Tags.Add(tag);
            _tags.Add(tag);
        }
    }

    private async Task SeedPosts()
    {
        var user0 = GetApplicationUser(0);
        var user1 = GetApplicationUser(1);

        var definitions = new[]
        {
        new { Title = "First Post",  Slug = "first-post",  OwnerId = user0.UserProfile.Id, PostTags = new[] { Tags.Dotnet, Tags.Csharp } },
        new { Title = "Second Post", Slug = "second-post", OwnerId = user0.UserProfile.Id, PostTags = new[] { Tags.Dotnet } },
        new { Title = "Third Post",  Slug = "third-post",  OwnerId = user1.UserProfile.Id, PostTags = new[] { Tags.Aspnet } },
    };

        foreach (var def in definitions)
        {
            var existing = await _appDbContext.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Slug == def.Slug);

            if (existing is not null)
            {
                _posts.Add(existing);
                continue;
            }

            var post = new Post
            {
                Title = def.Title,
                Slug = def.Slug,
                Content = _faker.Lorem.Paragraphs(3),
                Excerpt = _faker.Lorem.Sentence(),
                UserProfileId = def.OwnerId,
                Tags = def.PostTags.Select(t => GetTag(t)).ToList()
            };

            _appDbContext.Posts.Add(post);
            _posts.Add(post);
        }
    }

    private async Task SeedUsers(int count = 10)
    {
        var userFaker = new Faker<RegisterDto>()
            .CustomInstantiator(f => new RegisterDto
            {
                Username = f.Internet.UserName().Replace('.','_'),
                Email = f.Internet.Email(),
                DisplayName = f.Internet.UserName(),
                Password = DefaultPassword
            });
        
        var registerDtos = userFaker.Generate(count);

        foreach(var dto in registerDtos)
        {
            await _userManager.CreateUserAsync(dto);
        }

        _applicationUsers.AddRange(_appDbContext.Users.Include(u => u.UserProfile).ToList());
        _userProfiles.AddRange(_appDbContext.UserProfiles.ToList());
    }
    // Helper methods to get seeded entities
    public UserProfile GetUserProfile(int index = 0) => _userProfiles[index];
    public ApplicationUser GetApplicationUser(int index = 0) => _applicationUsers[index];
    public Post GetPost(int index = 0) => _posts[index];
    public Comment GetComment(int index = 0) => _comments[index];
    public int GetCommentsLength() => _comments.Count;
    public Tag GetTag(Tags tag = Tags.Dotnet) => (tag) switch
    {
        Tags.Dotnet => _tags[0],
        Tags.Csharp => _tags[1],
        Tags.Aspnet => _tags[2],
        _ => _tags[0]
    };

    public string DefaultPassword => "Password123!";

    public int GetUserProfilesLength() => _userProfiles.Count;
    public int GetApplicationUsersLength() => _applicationUsers.Count;
    public int GetPostsLength() => _posts.Count;
}

public enum Tags
{
   Dotnet,
   Csharp,
   Aspnet
}