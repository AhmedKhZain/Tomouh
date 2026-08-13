using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Shared.Kernel.Features;

public interface IEmailSender
{
    Task SendEmailAsync(string to, EmailContent content, bool isHtml = true);
    Task SendEmailAsync(string email, string subject, string htmlMessage, bool isHtml = true);
}
