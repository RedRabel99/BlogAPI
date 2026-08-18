using BlogAPI.Infrastructure.Email;

namespace BlogAPI.Infrastructure.UnitTests.Email;

public class OutboxMessageTests
{
    [Fact]
    public void MarkProcessed_SetsProcessedOnAndClearsError()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var processedDateTime = DateTime.UtcNow;

        // Act
        outboxMessage.MarkProcessed(processedDateTime);

        // Assert
        Assert.Equal(processedDateTime, outboxMessage.ProcessedOn);
        Assert.Null(outboxMessage.Error);
    }

    [Fact]
    public void MarkProcessed_WhenFailedPreviously_ClearsErrorAndPendingRetry()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var failedDateTime = DateTime.UtcNow;
        outboxMessage.MarkFailed("Some error", failedDateTime);
        var processedDateTime = failedDateTime.AddMinutes(1);

        // Act
        outboxMessage.MarkProcessed(processedDateTime);

        // Assert
        Assert.Equal(processedDateTime, outboxMessage.ProcessedOn);
        Assert.Null(outboxMessage.Error);
        Assert.Null(outboxMessage.NextAttemptOn);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 5)]
    [InlineData(3, 25)]
    [InlineData(4, 60)]
    public void MarkFailed_SchedulesNextAttemptCorrectly(int retryCount, int expectedDelayMinutes)
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var firstFailedDateTime = DateTime.UtcNow;
        var lastFailedDateTime = firstFailedDateTime;

        // Act — each failure happens on a different day, so the assert pins the delay
        // to the latest failure rather than to a fixed anchor
        for (int i = 0; i < retryCount; i++)
        {
            lastFailedDateTime = firstFailedDateTime.AddDays(i);
            outboxMessage.MarkFailed("Some error", lastFailedDateTime);
        }

        // Assert
        Assert.Equal(retryCount, outboxMessage.RetryCount);
        Assert.Equal(lastFailedDateTime.AddMinutes(expectedDelayMinutes), outboxMessage.NextAttemptOn);
    }

    [Fact]
    public void MarkFailed_AfterExhaustingRetries_SetsNextAttemptOnToNull()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var failedDateTime = DateTime.UtcNow;

        // Act
        for (int i = 0; i < OutboxMessage.MaxAttempts; i++)
        {
            outboxMessage.MarkFailed("Some error", failedDateTime);
        }

        // Assert
        Assert.Equal(OutboxMessage.MaxAttempts, outboxMessage.RetryCount);
        Assert.Null(outboxMessage.NextAttemptOn);
    }

    [Fact]
    public void MarkFailed_WithOverlongError_TruncatesToColumnLimit()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var overlongError = new string('x', 5000);

        // Act
        outboxMessage.MarkFailed(overlongError, DateTime.UtcNow);

        // Assert — the Error column is varchar(4000); a longer value would fail the batch save
        Assert.Equal(4000, outboxMessage.Error!.Length);
    }

    [Fact]
    public void MarkFailed_DoesNotSetProcessedOn()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Type = "TestType",
            Content = "TestContent"
        };
        var failedDateTime = DateTime.UtcNow;

        // Act
        outboxMessage.MarkFailed("Some error", failedDateTime);

        // Assert
        Assert.Null(outboxMessage.ProcessedOn);
        Assert.Equal("Some error", outboxMessage.Error);
    }
}
