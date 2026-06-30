using BlogAPI.Application.Posts;
using BlogAPI.Application.Posts.Dtos;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.UnitTests.Mapping;

public class PostMappersTests
{
    [Fact]
    public void ToDto_MapsSimpleFieldsAndAuthor()
    {
        //Arrange
        var authorId = Guid.NewGuid();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Slug = "title",
            Excerpt = "excerpt",
            Content = "content",
            CreatedAt = new DateTime(2025, 1, 1),
            UpdatedAt = new DateTime(2025, 1, 2),
            UserProfile = new UserProfile { Id = authorId, Username = "author" }
        };

        //Act
        var dto = post.ToDto();

        //Assert
        Assert.Equal(post.Id, dto.Id);
        Assert.Equal("Title", dto.Title);
        Assert.Equal("title", dto.Slug);
        Assert.Equal("excerpt", dto.Excerpt);
        Assert.Equal("content", dto.Content);
        Assert.Equal(post.CreatedAt, dto.CreatedAt);
        Assert.Equal(post.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(authorId, dto.Author.Id);
        Assert.Equal("author", dto.Author.Username);
    }

    [Fact]
    public void ToDto_ProjectsTagNames()
    {
        //Arrange
        var post = new Post
        {
            UserProfile = new UserProfile { Username = "author" },
            Tags = new List<Tag>
            {
                new() { TagName = "csharp" },
                new() { TagName = "dotnet" }
            }
        };

        //Act
        var dto = post.ToDto();

        //Assert
        Assert.Equal(new[] { "csharp", "dotnet" }, dto.Tags);
    }

    [Fact]
    public void ToDto_WithNoTags_YieldsEmptyList()
    {
        //Arrange
        var authorId = Guid.NewGuid();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Slug = "title",
            Excerpt = "excerpt",
            Content = "content",
            CreatedAt = new DateTime(2025, 1, 1),
            UpdatedAt = new DateTime(2025, 1, 2),
            UserProfile = new UserProfile { Id = authorId, Username = "author" }
        };

        //Act
        var dto = post.ToDto();

        //Assert
        Assert.Empty(dto.Tags);
    }
}
