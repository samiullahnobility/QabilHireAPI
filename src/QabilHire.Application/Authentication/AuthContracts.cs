using System.ComponentModel.DataAnnotations;

namespace QabilHire.Application.Authentication;

public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128)] string Password);

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress, StringLength(254)] string Email);

public sealed record ResetPasswordRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required] string Token,
    [Required, StringLength(128, MinimumLength = 8)] string NewPassword);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserResponse User, string? Message = null, bool EmailEnabled = true);
public sealed record UserResponse(Guid Id, string FullName, string Email, IReadOnlyCollection<string> Roles, bool ProfileComplete);
