using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Email;

public interface IQabilHireEmailSender
{
    Task SendWelcomeAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(ApplicationUser user, string resetLink, CancellationToken cancellationToken);
    Task SendPasswordChangedAsync(ApplicationUser user, CancellationToken cancellationToken);
}
