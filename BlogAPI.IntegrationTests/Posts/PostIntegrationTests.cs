using BlogAPI.Application.DTOs.Posts;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.Posts;

public class PostIntegrationTests : BaseIntegrationTest
{
    public PostIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    // ---------------------------------------------------------------------------
    // GET /post/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetPostById_WithExistingId_ReturnsPost()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);

        // Act
        var response = await HttpClient.GetAsync($"post/{post.Id}");
        var body = await response.Content.ReadFromJsonAsync<PostDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(post.Id, body.Id);
        Assert.Equal(post.Title, body.Title);
        Assert.Equal(post.Slug, body.Slug);
        Assert.Equal(post.Excerpt, body.Excerpt);
        Assert.Equal(post.Content, body.Content);
        Assert.Equal(post.UserProfile.Username, body.Author.Username);
        Assert.Equal(post.UserProfile.Id, body.Author.Id);
        Assert.Equal(post.Tags.Select(t => t.TagName).Order(), body.Tags.Order());
    }

    [Fact]
    public async Task GetPostById_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"post/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // GET /post
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetPostList_WithDefaultQuery_ReturnsAllPosts()
    {
        // Arrange
        var query = QueryString.Create("pageSize", DataSeeder.GetPostsLength().ToString());

        // Act
        var response = await HttpClient.GetAsync("post" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<PostDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(DataSeeder.GetPostsLength(), body.Items.Count);
    }

    [Fact]
    public async Task GetPostList_FilteredByTitle_ReturnsMatchingPosts()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var query = QueryString.Create("title", post.Title);

        // Act
        var response = await HttpClient.GetAsync("post" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<PostDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.All(body.Items, p => Assert.Contains(post.Title, p.Title));
    }

    [Fact]
    public async Task GetPostList_FilteredByTag_ReturnsOnlyPostsWithThatTag()
    {
        // Arrange
        var query = QueryString.Create("tags", "dotnet");

        // Act
        var response = await HttpClient.GetAsync("post" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<PostDto>>();
        var text = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.All(body.Items, p => Assert.Contains("dotnet", p.Tags));
    }

    [Fact]
    public async Task GetPostList_FilteredByUsername_ReturnsOnlyThatUsersPost()
    {
        // Arrange
        var user0 = DataSeeder.GetApplicationUser(0);
        var query = QueryString.Create("usernames", user0.UserName);

        // Act
        var response = await HttpClient.GetAsync("post" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<PostDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.All(body.Items, p => Assert.Equal(user0.UserName, p.Author.Username));
    }

    [Fact]
    public async Task GetPostList_WithSecondPage_ReturnsCorrectPagedResult()
    {
        // Arrange
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["page"] = "2",
            ["pageSize"] = "2"
        });

        // Act
        var response = await HttpClient.GetAsync("post" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<PostDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        //checking wether page 2 has items or not instead of checking if it equals 1, cause other tests might add more
        Assert.NotEmpty(body.Items);
        Assert.Equal(2, body.Page);
        Assert.False(body.HasNextPage);
        Assert.True(body.HasPreviousPage);
    }

    [Fact]
    public async Task GetPostList_WithInvalidSortColumn_ReturnsBadRequest()
    {
        // Arrange
        var query = QueryString.Create("sortColumn", "invalid-column");

        // Act
        var response = await HttpClient.GetAsync("post" + query);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // POST /post
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CreatePost_AuthenticatedWithValidDto_ReturnsCreatedPost()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var dto = new CreatePostDto
        {
            Title = "Integration Test Post",
            Slug = "integration-test-post",
            Excerpt = "An excerpt",
            Content = "Some content",
            Tags = new List<string> { "integration", "test" }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("post", dto);
        var body = await response.Content.ReadFromJsonAsync<PostDto>();
        var text = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(dto.Title, body.Title);
        Assert.Equal(dto.Slug, body.Slug);
        Assert.Equal(dto.Excerpt, body.Excerpt);
        Assert.Equal(dto.Content, body.Content);
        Assert.Equal(user.UserName, body.Author.Username);
        Assert.Equal(dto.Tags.Order(), body.Tags.Order());
    }

    [Fact]
    public async Task CreatePost_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        var dto = new CreatePostDto
        {
            Title = "Post",
            Content = "Content",
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("post", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var dto = new CreatePostDto
        {
            Title = "",
            Content = "Some content"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("post", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithDuplicateTags_ReturnsBadRequest()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var dto = new CreatePostDto
        {
            Title = "Post",
            Content = "Content",
            Tags = new List<string> { "tag", "tag" }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("post", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // PATCH /post/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task UpdatePost_AuthenticatedAsOwner_ReturnsUpdatedPost()
    {
        // Arrange
        // post 2 is owned by user 1
        var post = DataSeeder.GetPost(2);
        var owner = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(owner.Email, DataSeeder.DefaultPassword);

        var dto = new UpdatePostDto
        {
            Title = "Updated Title",
            Tags = new List<string> { "updated" }
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"post/{post.Id}", dto);
        var body = await response.Content.ReadFromJsonAsync<PostDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(dto.Title, body.Title);
        Assert.Equal(new[] { "updated" }, body.Tags.Order());
    }

    [Fact]
    public async Task UpdatePost_AuthenticatedAsNonOwner_ReturnsForbidden()
    {
        // Arrange
        // post 0 is owned by user 0
        var post = DataSeeder.GetPost(0);
        var nonOwner = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(nonOwner.Email, DataSeeder.DefaultPassword);

        var dto = new UpdatePostDto { Title = "Hijacked Title" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"post/{post.Id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePost_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var post = DataSeeder.GetPost(0);

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"post/{post.Id}", new UpdatePostDto { Title = "Title" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePost_WithNoFieldsProvided_ReturnsBadRequest()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var owner = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(owner.Email, DataSeeder.DefaultPassword);

        var dto = new UpdatePostDto();

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"post/{post.Id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // DELETE /post/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task DeletePost_AuthenticatedAsOwner_ReturnsNoContent()
    {
        // Arrange
        var post = DataSeeder.GetPost(1);
        var owner = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(owner.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"post/{post.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletePost_AuthenticatedAsNonOwner_ReturnsForbidden()
    {
        // Arrange
        // post 0 owned by user 0
        var post = DataSeeder.GetPost(0);
        var nonOwner = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(nonOwner.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"post/{post.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletePost_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var post = DataSeeder.GetPost(0);

        // Act
        var response = await HttpClient.DeleteAsync($"post/{post.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeletePost_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var owner = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(owner.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"post/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
