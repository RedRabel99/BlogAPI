using BlogAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogAPI.Application.Interfaces;

public interface IEmailQueue 
{
    Task AddToOuboxAsync(EmailMessage message, CancellationToken ct = default);
}
