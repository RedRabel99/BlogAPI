using System;

namespace BlogAPI.Infrastructure.Email;

public class SmtpOptions
{
    public string Host { get; init; } = "";
    public int Port { get; init; }
    public string FromAddress { get; init; } = "";
    public bool UseSsl { get; init; }
}