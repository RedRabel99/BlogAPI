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

    public static int MaxAttempts => RetrySchedule.Length + 1;

    //must match the Error column's HasMaxLength in OutboxMessageTypeConfiguration
    private const int MaxErrorLength = 4000;

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
        NextAttemptOn = null; //a processed message has no pending retry
    }

    public void MarkFailed(string error, DateTime dateTime)
    {
        //a longer message would be rejected by the database and roll back the whole batch
        Error = error.Length > MaxErrorLength ? error[..MaxErrorLength] : error;
        RetryCount++;

        NextAttemptOn = RetryCount < MaxAttempts ? dateTime + RetrySchedule[RetryCount - 1] : null;
    }
}
