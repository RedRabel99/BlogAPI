using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BlogAPI.Infrastructure.Email;

public sealed class OutboxEmailProcessor : BackgroundService
{
    private const int BatchSize = 100;
    private const int MaxAttempts = 3;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxEmailProcessor> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

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
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        await using var tx = await context.Database.BeginTransactionAsync(ct);

        var outboxMessages = await context.OutboxMessages.FromSql(
            $"""
            SELECT *
            FROM "OutboxMessages"
            WHERE "ProcessedOn" IS NULL AND "RetryCount" < {MaxAttempts}
            ORDER BY "OccuredOn"
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
            outboxMessage.RetryCount += 1;

            if(outboxMessage.Type != nameof(EmailMessage))
            {
                continue;
            }
            try
            {
                var email = JsonSerializer.Deserialize<EmailMessage>(outboxMessage.Content);
                await sender.SendEmailAsync(email, ct);

                outboxMessage.ProcessedOn = DateTime.UtcNow;
                outboxMessage.Error = null;
            }
            catch(Exception ex) when (ex is not OperationCanceledException)
            {
                outboxMessage.Error = ex.Message;
                _logger.LogWarning(
                    $"Failed to send outbox email {outboxMessage.Id}" +
                    $" (attempt {outboxMessage.RetryCount}/{MaxAttempts} because: {outboxMessage.Error} ");
            }
        }
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);   
    }
}
