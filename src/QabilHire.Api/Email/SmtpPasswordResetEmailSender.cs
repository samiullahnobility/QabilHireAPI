using System.Net;
using System.Net.Mail;
using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Email;

public sealed class SmtpPasswordResetEmailSender(IConfiguration configuration) : IPasswordResetEmailSender
{
    public async Task SendAsync(ApplicationUser user, string resetLink, CancellationToken cancellationToken)
    {
        var host = configuration["Email:Smtp:Host"]
            ?? throw new InvalidOperationException("SMTP host is not configured.");
        var fromAddress = configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Email sender address is not configured.");

        using var message = new MailMessage(fromAddress, user.Email!)
        {
            Subject = "Reset your QabilHire password",
            Body = $"Use this secure link to reset your QabilHire password:\n\n{resetLink}\n\nIf you did not request this, you can ignore this email.",
            IsBodyHtml = false
        };

        using var smtpClient = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true)
        };

        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            smtpClient.Credentials = new NetworkCredential(username, password);
        }

        await smtpClient.SendMailAsync(message, cancellationToken);
    }
}
