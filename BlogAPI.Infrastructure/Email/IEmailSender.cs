using System;
using System.Collections.Generic;
using System.Text;

namespace BlogAPI.Infrastructure.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string email);
}
