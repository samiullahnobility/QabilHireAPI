using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Authentication;

public sealed class RefreshTokenService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
{
    private const string LoginProvider = "QabilHire";
    private const string TokenName = "RefreshToken";

    public async Task<string> IssueAsync(ApplicationUser user)
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiresAtUtc = DateTime.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7));
        var storedValue = $"{Hash(secret)}|{expiresAtUtc.Ticks}";
        var result = await userManager.SetAuthenticationTokenAsync(user, LoginProvider, TokenName, storedValue);
        EnsureSucceeded(result);
        return $"{user.Id:N}.{secret}";
    }

    public async Task<ApplicationUser?> RotateAsync(string? refreshToken)
    {
        var parsed = Parse(refreshToken);
        if (parsed is null)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(parsed.Value.UserId.ToString());
        if (user is null || !await IsValidAsync(user, parsed.Value.Secret))
        {
            return null;
        }

        var removeResult = await userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
        EnsureSucceeded(removeResult);
        return user;
    }

    public async Task RevokeAsync(string? refreshToken)
    {
        var parsed = Parse(refreshToken);
        if (parsed is null)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(parsed.Value.UserId.ToString());
        if (user is null)
        {
            return;
        }

        var result = await userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
        EnsureSucceeded(result);
    }

    public async Task RevokeUserAsync(ApplicationUser user)
    {
        var result = await userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);
        EnsureSucceeded(result);
    }

    private async Task<bool> IsValidAsync(ApplicationUser user, string secret)
    {
        var storedValue = await userManager.GetAuthenticationTokenAsync(user, LoginProvider, TokenName);
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return false;
        }

        var parts = storedValue.Split('|', 2);
        if (parts.Length != 2 || parts[0].Length != 64 || !long.TryParse(parts[1], out var expiresAtTicks) || expiresAtTicks <= DateTime.UtcNow.Ticks)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(parts[0]),
            Convert.FromHexString(Hash(secret)));
    }

    private static (Guid UserId, string Secret)? Parse(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var parts = refreshToken.Split('.', 2);
        return parts.Length == 2 && Guid.TryParseExact(parts[0], "N", out var userId) && parts[1].Length > 0
            ? (userId, parts[1])
            : null;
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unable to update the authentication session.");
        }
    }
}
