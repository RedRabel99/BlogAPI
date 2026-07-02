using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BlogAPI.Infrastructure.Email;

public sealed class OutboxEmailProcessor : BackgroundService
{
    private const int BatchSize = 100;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxEmailProcessor> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private int _consecutiveFailures;
    private DateTime _pausedUntil;
    private const int FailureThreshold = 5;
    private static readonly TimeSpan PauseDuration = TimeSpan.FromMinutes(2);


    public OutboxEmailProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxEmailProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailMessages(stoppingToken);
            }
            catch(OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; 
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Outbox processor iteration failed");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessEmailMessages(CancellationToken ct)
    {
        if (DateTime.UtcNow < _pausedUntil)
        {
            return; //returning while in pause caused by consecutive failures
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        await using var tx = await context.Database.BeginTransactionAsync(ct);

        var emailType = typeof(EmailMessage).FullName!;

        var outboxMessages = await context.OutboxMessages.FromSql(
            $"""
            SELECT *
            FROM "OutboxMessages"
            WHERE "ProcessedOn" IS NULL
              AND "RetryCount" < {OutboxMessage.MaxAttempts}
              AND "Type" = {emailType}
              AND ("NextAttemptOn" IS NULL OR "NextAttemptOn" <= {DateTime.UtcNow})
            ORDER BY "OccurredOn"
            LIMIT {BatchSize}
            FOR UPDATE SKIP LOCKED
            """).ToListAsync(ct);

        if (outboxMessages.Count == 0)
        {
            await tx.RollbackAsync(ct);
            return;
        }

        foreach(var outboxMessage in outboxMessages)
        {
            try
            {
                var email = JsonSerializer.Deserialize<EmailMessage>(outboxMessage.Content)
                    ?? throw new InvalidOperationException(
                        $"Outbox message {outboxMessage.Id} deserialized to null EmailMessage.");
                await sender.SendEmailAsync(email, ct);

                outboxMessage.MarkProcessed(DateTime.UtcNow);
                _consecutiveFailures = 0;
            }
            catch(Exception ex) when (ex is not OperationCanceledException)
            {
                outboxMessage.MarkFailed(ex.Message, DateTime.UtcNow);

                _consecutiveFailures++;

                if(_consecutiveFailures >= FailureThreshold)
                {
                    _pausedUntil = DateTime.UtcNow.Add(PauseDuration);
                    _logger.LogWarning("Outbox email processor paused for {PauseDuration} due to {ConsecutiveFailures} consecutive failures.",
                        PauseDuration, _consecutiveFailures);
                    break; //stop iterating through messages, will resume after pause
                }

                _logger.LogError(ex,
                    "Failed to send outbox email {Id} (attempt {Attempt}/{Max}): {Error}",
                    outboxMessage.Id, outboxMessage.RetryCount, OutboxMessage.MaxAttempts, outboxMessage.Error);
            }
        }
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);   
    }
}
