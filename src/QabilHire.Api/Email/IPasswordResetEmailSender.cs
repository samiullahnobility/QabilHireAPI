using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Email;

public interface IPasswordResetEmailSender
{
    Task SendAsync(ApplicationUser user, string resetLink, CancellationToken cancellationToken);
}
