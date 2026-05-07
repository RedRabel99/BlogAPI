using BlogAPI.Application.DTOs.Comments;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.Comments;

public class CommentIntegrationTests : BaseIntegrationTest
{
    public CommentIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    // ---------------------------------------------------------------------------
    // GET /posts/{postId}/comments
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetCommentsByPostId_WithExistingPost_ReturnsComments()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);

        // Act
        var response = await HttpClient.GetAsync($"/posts/{post.Id}/comments");
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<CommentDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEmpty(body.Items);
        Assert.All(body.Items, c => Assert.Equal(post.Id, c.PostId));
    }

    [Fact]
    public async Task GetCommentsByPostId_WithNonExistingPost_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/posts/{Guid.NewGuid()}/comments");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCommentsByPostId_WithInvalidSortColumn_ReturnsBadRequest()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var query = QueryString.Create("sortColumn", "invalid-column");

        // Act
        var response = await HttpClient.GetAsync($"/posts/{post.Id}/comments" + query);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCommentsByPostId_FilteredByAuthor_ReturnsOnlyThatAuthorsComments()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var user0 = DataSeeder.GetApplicationUser(0);
        var query = QueryString.Create("author", user0.UserName);

        // Act
        var response = await HttpClient.GetAsync($"/posts/{post.Id}/comments" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<CommentDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.All(body.Items, c => Assert.Equal(user0.UserName, c.Author.Username));
    }

    // ---------------------------------------------------------------------------
    // GET /comments/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetCommentById_WithExistingId_ReturnsComment()
    {
        // Arrange
        var comment = DataSeeder.GetComment(0);

        // Act
        var response = await HttpClient.GetAsync($"/comments/{comment.Id}");
        var body = await response.Content.ReadFromJsonAsync<CommentDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(comment.Id, body.Id);
        Assert.Equal(comment.Content, body.Content);
        Assert.Equal(comment.PostId, body.PostId);
    }

    [Fact]
    public async Task GetCommentById_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/comments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // POST /posts/{postId}/comments
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CreateComment_AuthenticatedWithValidDto_ReturnsCreatedComment()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var dto = new CreateCommentDto { Content = "Integration test comment" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/posts/{post.Id}/comments", dto);
        var body = await response.Content.ReadFromJsonAsync<CommentDto>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(dto.Content, body.Content);
        Assert.Equal(post.Id, body.PostId);
        Assert.Equal(user.UserName, body.Author.Username);
    }

    [Fact]
    public async Task CreateComment_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var post = DataSeeder.GetPost(0);
        var dto = new CreateCommentDto { Content = "Great post!" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/posts/{post.Id}/comments", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateComment_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var post = DataSeeder.GetPost(0);
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var dto = new CreateCommentDto { Content = "" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/posts/{post.Id}/comments", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateComment_OnNonExistingPost_ReturnsNotFound()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var dto = new CreateCommentDto { Content = "Hi" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/posts/{Guid.NewGuid()}/comments", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // PATCH /comments/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task UpdateComment_AuthenticatedAsAuthorWithValidData_ReturnsUpdatedComment()
    {
        // Arrange
        // comment 0 owned by user 0
        var comment = DataSeeder.GetComment(0);
        var author = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(author.Email, DataSeeder.DefaultPassword);

        var dto = new UpdateCommentDto { Content = "Updated content" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/comments/{comment.Id}", dto);
        var body = await response.Content.ReadFromJsonAsync<CommentDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(dto.Content, body.Content);
    }

    [Fact]
    public async Task UpdateComment_AuthenticatedAsNonAuthor_ReturnsForbidden()
    {
        // Arrange
        // comment 0 owned by user 0
        var comment = DataSeeder.GetComment(0);
        var nonAuthor = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(nonAuthor.Email, DataSeeder.DefaultPassword);

        var dto = new UpdateCommentDto { Content = "Hijacked" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/comments/{comment.Id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var comment = DataSeeder.GetComment(0);

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/comments/{comment.Id}",
            new UpdateCommentDto { Content = "x" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_AuthenticatedWithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var comment = DataSeeder.GetComment(0);
        var author = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(author.Email, DataSeeder.DefaultPassword);

        var dto = new UpdateCommentDto { Content = "" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"/comments/{comment.Id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/comments/{Guid.NewGuid()}",
            new UpdateCommentDto { Content = "New content" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // DELETE /comments/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task DeleteComment_AuthenticatedAsAuthor_ReturnsNoContent()
    {
        // Arrange
        // comment 1 owned by user 1 — pick distinct from update test target
        var comment = DataSeeder.GetComment(1);
        var author = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(author.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"/comments/{comment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_AuthenticatedAsNonAuthor_ReturnsForbidden()
    {
        // Arrange
        // comment 0 owned by user 0
        var comment = DataSeeder.GetComment(0);
        var nonAuthor = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(nonAuthor.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"/comments/{comment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var comment = DataSeeder.GetComment(0);

        // Act
        var response = await HttpClient.DeleteAsync($"/comments/{comment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var user = DataSeeder.GetApplicationUser(0);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        // Act
        var response = await HttpClient.DeleteAsync($"/comments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
