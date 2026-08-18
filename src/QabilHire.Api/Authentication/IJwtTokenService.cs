using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Authentication;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) Create(ApplicationUser user, IReadOnlyCollection<string> roles);
}
