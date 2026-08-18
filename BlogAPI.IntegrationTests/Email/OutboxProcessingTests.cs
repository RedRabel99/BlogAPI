using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Email;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace BlogAPI.IntegrationTests.Email
{
    public class OutboxProcessingTests : BaseIntegrationTest
    {
        private readonly TestEmailSender _testEmailSender;
        private readonly OutboxEmailProcessor _outboxEmailProcessor;

        public OutboxProcessingTests(IntegrationTestFactory factory) : base(factory)
        {
            _testEmailSender = factory.Services.GetRequiredService<TestEmailSender>();
            _testEmailSender.Reset();

            _outboxEmailProcessor = new OutboxEmailProcessor(
                factory.Services.GetRequiredService<IServiceScopeFactory>(),
                NullLogger<OutboxEmailProcessor>.Instance);
        }

        [Fact]
        public async Task ProcessEmailMessages_WhenSendFails_SchedulesRetryAndSkipsOnNextPass()
        {
            // Arrange
            var seeded = await SeedOutboxMessageAsync();
            _testEmailSender.ShouldThrow = true;

            // Act — first pass: send fails, message schedules its retry
            await _outboxEmailProcessor.ProcessEmailMessages(CancellationToken.None);
            var afterFirstPass = await ReloadAsync(seeded.Id);

            // Act — second pass immediately: message is backed off, must not be attempted
            await _outboxEmailProcessor.ProcessEmailMessages(CancellationToken.None);
            var afterSecondPass = await ReloadAsync(seeded.Id);

            // Assert
            Assert.Equal(1, afterFirstPass.RetryCount);
            Assert.NotNull(afterFirstPass.Error);
            Assert.Null(afterFirstPass.ProcessedOn);
            Assert.True(afterFirstPass.NextAttemptOn > DateTime.UtcNow);

            Assert.Equal(1, afterSecondPass.RetryCount);
            Assert.Empty(_testEmailSender.Sent);
        }

        [Fact]
        public async Task ProcessEmailMessages_WhenSendSucceeds_MarksProcessedAndSendsEmail()
        {
            // Arrange
            var seeded = await SeedOutboxMessageAsync();

            // Act
            await _outboxEmailProcessor.ProcessEmailMessages(CancellationToken.None);
            var processed = await ReloadAsync(seeded.Id);

            // Assert
            Assert.NotNull(processed.ProcessedOn);
            Assert.Null(processed.Error);
            Assert.Null(processed.NextAttemptOn);
            Assert.Equal(0, processed.RetryCount);

            var sent = Assert.Single(_testEmailSender.Sent);
            Assert.Equal("to0@mail.com", sent.To);
        }

        [Fact]
        public async Task ProcessEmailMessages_AfterConsecutiveFailuresThreshold_AbandonsBatchAndPausesProcessing()
        {
            // Arrange — one more than the breaker threshold, so tripping it must leave a message unattempted
            var batch = await SeedOutboxMessagesAsync(6);
            var batchIds = batch.Select(m => m.Id).ToList();
            _testEmailSender.ShouldThrow = true;

            // Act — pass 1: five consecutive failures trip the breaker, which abandons the rest of the batch
            await _outboxEmailProcessor.ProcessEmailMessages(CancellationToken.None);

            // a fresh, immediately-eligible message arrives while paused;
            // sender "recovers" so a skipped message can only be explained by the pause
            var seededDuringPause = (await SeedOutboxMessagesAsync(1, wipeExisting: false)).Single();
            _testEmailSender.ShouldThrow = false;

            // Act — pass 2: processor is paused, must not query or send anything
            await _outboxEmailProcessor.ProcessEmailMessages(CancellationToken.None);
            var afterPausedPass = await ReloadAsync(seededDuringPause.Id);

            // Assert — the tripping pass persisted exactly threshold-many failures, and the
            // break left the remaining message of the batch unattempted
            Assert.Equal(5, await AppDbContext.OutboxMessages.AsNoTracking()
                .CountAsync(m => batchIds.Contains(m.Id) && m.RetryCount == 1));
            Assert.Equal(1, await AppDbContext.OutboxMessages.AsNoTracking()
                .CountAsync(m => batchIds.Contains(m.Id) && m.RetryCount == 0));

            // Assert — the eligible message was untouched during the pause
            Assert.Equal(0, afterPausedPass.RetryCount);
            Assert.Null(afterPausedPass.ProcessedOn);
            Assert.Null(afterPausedPass.NextAttemptOn);
            Assert.Empty(_testEmailSender.Sent);
        }

        private async Task<OutboxMessage> SeedOutboxMessageAsync() =>
            (await SeedOutboxMessagesAsync(1)).Single();

        private async Task<List<OutboxMessage>> SeedOutboxMessagesAsync(int count, bool wipeExisting = true)
        {
            if (wipeExisting)
            {
                // wipe rows left by the seeder or sibling tests so counts are exact
                await AppDbContext.OutboxMessages.ExecuteDeleteAsync();
            }

            var messages = new List<OutboxMessage>();
            for (var i = 0; i < count; i++)
            {
                var email = new EmailMessage($"to{i}@mail.com", "Test subject", "Test body");
                messages.Add(new OutboxMessage
                {
                    Type = typeof(EmailMessage).FullName!,
                    Content = JsonSerializer.Serialize(email)
                });
            }

            AppDbContext.OutboxMessages.AddRange(messages);
            await AppDbContext.SaveChangesAsync();
            return messages;
        }

        private Task<OutboxMessage> ReloadAsync(Guid id) =>
            AppDbContext.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == id);
    }
}
