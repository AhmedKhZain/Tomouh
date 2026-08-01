using Common.Models;

namespace Common.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string to, EmailContent content, bool isHtml = true);
    Task SendEmailAsync(string email, string subject, string htmlMessage, bool isHtml = true);
}