using Common.Infrastructure.OptionsModels;
using Common.Models;
using Common.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Common.Infrastructure.Features.Email;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailSettings;

    public EmailSender(IOptions<EmailOptions> emailSettingsOptions)
    {
        _emailSettings = emailSettingsOptions.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage, bool isHtml = true)
    {
        var massege = new MailMessage
        {
            From = new MailAddress(_emailSettings.From),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = isHtml
        };
        massege.To.Add(new MailAddress(email));
        var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port);
        client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
        client.EnableSsl = _emailSettings.EnableSsl;


        await client.SendMailAsync(massege);
    }
    public async Task SendEmailAsync(string to, EmailContent content, bool isHtml = true)
    {
        var massege = new MailMessage
        {
            From = new MailAddress(_emailSettings.From),
            Subject = content.Subject,
            Body = content.Body,
            IsBodyHtml = isHtml
        };
        massege.To.Add(new MailAddress(to));
        var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port);
        client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
        client.EnableSsl = _emailSettings.EnableSsl;


        await client.SendMailAsync(massege);
    }
}