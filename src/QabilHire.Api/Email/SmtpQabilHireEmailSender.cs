using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Email;

public sealed class SmtpQabilHireEmailSender(IConfiguration configuration) : IQabilHireEmailSender
{
    public Task SendWelcomeAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        SendAsync(
            user,
            "Welcome to QabilHire",
            $"<h1>Welcome to QabilHire, {HtmlEncoder.Default.Encode(user.FullName)}!</h1><p>Your account is ready. You can now build your candidate profile, analyze resumes, compare job opportunities, and prepare for interviews.</p><p>We are glad to have you with us.</p>",
            cancellationToken);

    public Task SendPasswordResetAsync(ApplicationUser user, string resetLink, CancellationToken cancellationToken) =>
        SendAsync(
            user,
            "Reset your QabilHire password",
            $"<h1>Reset your password</h1><p>We received a request to reset your QabilHire password.</p><p><a href=\"{HtmlEncoder.Default.Encode(resetLink)}\" style=\"display:inline-block;padding:12px 20px;border-radius:8px;background:#059669;color:#fff;text-decoration:none;font-weight:700\">Reset password</a></p><p>If you did not request this, you can safely ignore this email.</p>",
            cancellationToken);

    public Task SendPasswordChangedAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        SendAsync(
            user,
            "Your QabilHire password was changed",
            "<h1>Password changed</h1><p>Your QabilHire password has been reset successfully.</p><p>If you did not make this change, contact support immediately and secure your email account.</p>",
            cancellationToken);

    private async Task SendAsync(ApplicationUser user, string subject, string body, CancellationToken cancellationToken)
    {
        var host = Required("Email:Smtp:Host");
        var fromAddress = Required("Email:FromAddress");
        var fromName = configuration["Email:FromName"] ?? "QabilHire";
        var username = Required("Email:Smtp:Username");
        var password = Required("Email:Smtp:Password");

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = Wrap(body),
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(user.Email!, user.FullName));

        using var smtpClient = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true),
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private string Required(string key)
    {
        var value = configuration[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{key} is not configured.");
    }

    private static string Wrap(string content) => $"""
        <!doctype html><html><body style="margin:0;background:#f8fafc;font-family:Arial,sans-serif;color:#0f172a">
        <div style="max-width:600px;margin:32px auto;padding:32px;border:1px solid #e2e8f0;border-radius:16px;background:#fff">
        <div style="margin-bottom:24px;color:#059669;font-size:22px;font-weight:800">QabilHire</div>{content}
        <p style="margin-top:28px;color:#64748b;font-size:13px">QabilHire · AI-powered career preparation</p></div></body></html>
        """;
}
