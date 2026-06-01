using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Validators.Auth;

namespace BlogAPI.Application.UnitTests.Validators;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidDto_IsValid()
    {
        //Act
        var dto = new RegisterDto()
        {
            Username = "validuser",
            DisplayName = "Valid Name",
            Password = "Passw0rd!",
            Email = "user@example.com"
        };

        var result = _validator.Validate(dto);

        //Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("-foo")]
    [InlineData("_foo")]
    public void Validate_WithUsernameStartingWithDashOrUnderscore_IsInvalid(string username)
    {
        //Arrange
        var dto = new RegisterDto()
        {
            Username = username,
            DisplayName = "Valid Name",
            Password = "Passw0rd!",
            Email = "user@example.com"
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.Username));
    }

    [Theory]
    [InlineData("ab")]            // too short (< 3)
    [InlineData("bad name")]      // space not allowed
    [InlineData("bad!name")]      // special char not allowed
    public void Validate_WithInvalidUsernameShape_IsInvalid(string username)
    {
        //Arrange
        var dto = new RegisterDto()
        {
            Username = username,
            DisplayName = "Valid Name",
            Password = "Passw0rd!",
            Email = "user@example.com"
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.Username));
    }

    [Theory]
    [InlineData("Short1!")]       // 7 chars, below min 8
    [InlineData("password1!")]    // no uppercase
    [InlineData("Password!!")]    // no digit
    [InlineData("Password11")]    // no special char
    public void Validate_WithPasswordMissingRule_IsInvalid(string password)
    {
        //Arrange
        var dto = new RegisterDto()
        {
            Username = "validuser",
            DisplayName = "Valid Name",
            Password = password,
            Email = "user@example.com"
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.Password));
    }

    [Fact]
    public void Validate_WithEmptyDisplayName_IsInvalid()
    {
        //Arrange
        var dto = new RegisterDto()
        {
            Username = "validuser",
            DisplayName = "", // empty display name
            Password = "Passw0rd!",
            Email = "user@example.com"
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterDto.DisplayName));
    }
}
