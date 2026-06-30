using BlogAPI.Application.Posts.Dtos;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Application.Posts.Validators;
using Microsoft.Extensions.Options;

namespace BlogAPI.Application.UnitTests.Validators;

public class PostQueryParametersValidatorTests
{
    private readonly PostQueryParametersValidator _validator;

    public PostQueryParametersValidatorTests()
    {
        var options = Options.Create(new PaginationOptions
        {
            DefaultPage = 1,
            DefaultPageSize = 10,
            MinPageSize = 1,
            MaxPageSize = 50
        });
        _validator = new PostQueryParametersValidator(options);
    }

    [Fact]
    public void Validate_WithDefaultQuery_IsValid()
    {
        //Act
        var result = _validator.Validate(new PostQueryParametersDto());

        //Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]    // below MinPageSize
    [InlineData(51)]   // above MaxPageSize
    public void Validate_WithPageSizeOutOfBounds_Fails(int pageSize)
    {
        //Arrange
        var dto = new PostQueryParametersDto { PageSize = pageSize };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.PageSize));
    }

    [Fact]
    public void Validate_WithPageBelowOne_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto { Page = 0 };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.Page));
    }

    [Theory]
    [InlineData("title")]
    [InlineData("username")]
    [InlineData("CreatedAt")]   // case-insensitive
    [InlineData("UPDATEDAT")]
    public void Validate_WithKnownSortColumn_Passes(string column)
    {
        //Arrange
        var dto = new PostQueryParametersDto { SortColumn = column };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("DESC")]        // case-insensitive
    public void Validate_WithKnownSortingOrder_Passes(string order)
    {
        //Arrange
        var dto = new PostQueryParametersDto { SortingOrder = order };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithUnknownSortColumn_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto { SortColumn = "bogus" };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.SortColumn));
    }

    [Fact]
    public void Validate_WithUnknownSortingOrder_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto { SortingOrder = "upwards" };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.SortingOrder));
    }

    [Fact]
    public void Validate_WithFromAfterTo_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto
        {
            From = new DateOnly(2025, 1, 10),
            To = new DateOnly(2025, 1, 1)
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.From));
    }

    [Fact]
    public void Validate_WithFromInFuture_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto
        {
            From = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1)
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostQueryParametersDto.From));
    }

    [Fact]
    public void Validate_WithUsernameStartingWithDash_Fails()
    {
        //Arrange
        var dto = new PostQueryParametersDto { Usernames = ["-bad"] };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith(nameof(PostQueryParametersDto.Usernames)));
    }
}
