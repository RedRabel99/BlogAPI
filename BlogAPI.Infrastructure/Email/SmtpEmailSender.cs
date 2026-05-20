using BlogAPI.Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlogAPI.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        mimeMessage.To.Add(new MailboxAddress(null, message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new TextPart(message.IsHtml ? "html" : "plain")
        {
            Text = message.Body
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, _smtpOptions.UseSsl, ct);
            await client.SendAsync(mimeMessage);
            

        } catch (Exception ex)
        {
            _logger.LogError(ex, $"Smtp send failed: To={message.To}, Subject={message.Subject}");
            throw; //rethrow for processor 
        }
        finally
        {
            if(client.IsConnected)
            {
                await client.DisconnectAsync(true, ct);
            }
        }
    }
}
