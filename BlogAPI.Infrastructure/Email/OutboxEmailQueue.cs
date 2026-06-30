using BlogAPI.Application.Common.Email;
using BlogAPI.Domain.Entities;
using System.Text.Json;

namespace BlogAPI.Infrastructure.Email;

public sealed class OutboxEmailQueue : IEmailQueue
{
    private readonly AppDbContext _context;
    

    public OutboxEmailQueue(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
    }

    public async Task EnqueueToOutbox(EmailMessage message, CancellationToken ct = default)
    {
        var outboxMessage = new OutboxMessage
        {
            Type = message.GetType().FullName!,
            Content = JsonSerializer.Serialize(message),
            OccurredOn = DateTime.UtcNow
        };

        await _context.OutboxMessages.AddAsync(outboxMessage, ct);

    }
}
