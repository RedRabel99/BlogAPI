namespace BlogAPI.Infrastructure.Email;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public required string Type { get; init; }
    public required string Content { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; private set; }
    public DateTime? NextAttemptOn { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public const int MaxAttempts = 5;
    private static readonly TimeSpan[] RetrySchedule = new[]
    {
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(25),
        TimeSpan.FromHours(1),
    };

    public void MarkProcessed(DateTime dateTime)
    {
        ProcessedOn = dateTime;
        Error = null;
    }

    public void MarkFailed(string error, DateTime dateTime)
    {
        Error = error;
        RetryCount++;

        NextAttemptOn = RetryCount < MaxAttempts ? dateTime + RetrySchedule[RetryCount - 1] : null;
    }
}
