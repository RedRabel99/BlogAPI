using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.UnitTests.Abstractions;

public class ResultTests
{
    private static readonly Error SomeError = Error.NotFound("X.NotFound", "missing");

    [Fact]
    public void Success_IsSuccessfulWithNoError()
    {
        //Act
        var result = Result.Success();

        //Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal(Error.None, result.Error);
        Assert.Empty(result.SubErrors);
    }

    [Fact]
    public void Failure_IsNotSuccessfulWithError()
    {
        //Act
        var result = Result.Failure(SomeError);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void GenericSuccess_HasValue()
    {
        var result = Result<string>.Success("payload");

        Assert.True(result.IsSuccess);
        Assert.Equal("payload", result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void GenericFailure_HasDefaultValueWithError()
    {
        //Act
        var result = Result<string>.Failure(SomeError);

        //Assert
        Assert.True(result.IsError);
        Assert.Null(result.Value);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void Failure_WithSubErrors_PreservesThem()
    {
        //Arrange
        var subErrors = new List<SubError> { new("Field", "bad") };

        //Act
        var result = Result.Failure(SomeError, subErrors);

        //Assert
        Assert.NotNull(result.Error);
        Assert.Single(result.SubErrors);
        Assert.Equal("Field", result.SubErrors[0].Name);
    }

    [Fact]
    public void Failure_WithNoError_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void Failure_WithNoErrorAndSubErrors_ThrowsException()
    {
        var subErrors = new List<SubError> { new("Field", "bad") };

        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None, subErrors));
    }

    [Fact]
    public void GenericFailure_WithNoError_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(Error.None));
    }

    [Fact]
    public void GenericFailure_WithSubErrors_PreservesThem()
    {
        //Arrange
        var subErrors = new List<SubError> { new("Field", "bad") };

        //Act
        var result = Result<string>.Failure(SomeError, subErrors);

        //Assert
        Assert.True(result.IsError);
        Assert.Null(result.Value);
        Assert.Single(result.SubErrors);
        Assert.Equal("Field", result.SubErrors[0].Name);
    }
}
