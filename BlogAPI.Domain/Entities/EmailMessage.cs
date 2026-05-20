namespace BlogAPI.Domain.Entities;

public sealed record EmailMessage(string To,
    string Subject,
    string Body,
    bool IsHtml = false
);
