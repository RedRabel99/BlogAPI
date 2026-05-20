using BlogAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogAPI.Infrastructure.Email;

public interface IEmailSender
{
    Task SendEmailAsync(EmailMessage message, CancellationToken ct = default);
}
