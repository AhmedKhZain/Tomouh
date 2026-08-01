using Common.Models;
using Common.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Email;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailSettings;

    public EmailSender(IOptions<EmailOptions> emailSettingsOptions)
    {
        _emailSettings = emailSettingsOptions.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage, bool isHtml = true)
    {
        return SendEmailAsync(email, new EmailContent(subject, htmlMessage), isHtml);
    }

    public async Task SendEmailAsync(string to, EmailContent content, bool isHtml = true)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.From),
            Subject = content.Subject,
            Body = content.Body,
            IsBodyHtml = isHtml
        };
        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
        {
            Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl
        };

        await client.SendMailAsync(message);
    }
}