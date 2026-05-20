using System;
using System.Collections.Generic;
using System.Text;

namespace BlogAPI.Domain.Entities;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.Now;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
