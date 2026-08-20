using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QabilHire.Api.Authentication;
using QabilHire.Api.Email;
using QabilHire.Api.RateLimiting;
using QabilHire.Application.Authentication;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace QabilHire.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService tokenService,
    RefreshTokenService refreshTokenService,
    IQabilHireEmailSender emailSender,
    IConfiguration configuration,
    ILogger<AuthController> logger,
    ApplicationDbContext dbContext) : ControllerBase
{
    private const string CandidateRole = "Candidate";
    private const string RefreshTokenCookie = "qabilhire_refresh_token";

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicyNames.Registration)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(
                result.Errors.GroupBy(error => error.Code)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray())));
        }

        var roleResult = await userManager.AddToRoleAsync(user, CandidateRole);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ValidationProblem(new ValidationProblemDetails(
                roleResult.Errors.GroupBy(error => error.Code)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray())));
        }

        try
        {
            await emailSender.SendWelcomeAsync(user, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to deliver the welcome email. TraceId: {TraceId}", HttpContext.TraceIdentifier);
        }

        return Ok(await CreateSessionResponse(user));
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicyNames.Login)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(await CreateSessionResponse(user));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicyNames.Session)]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        Request.Cookies.TryGetValue(RefreshTokenCookie, out var currentToken);
        var user = await refreshTokenService.RotateAsync(currentToken);
        if (user is null)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(new { message = "The session is invalid or has expired." });
        }

        return Ok(await CreateSessionResponse(user));
    }

    [HttpPost("logout")]
    [EnableRateLimiting(RateLimitPolicyNames.Session)]
    public async Task<IActionResult> Logout()
    {
        Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken);
        await refreshTokenService.RevokeAsync(refreshToken);
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimitPolicyNames.PasswordRecovery)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var frontendBaseUrl = configuration["Frontend:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Frontend base URL is not configured.");
            var resetLink = $"{frontendBaseUrl}/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            try
            {
                await emailSender.SendPasswordResetAsync(user, resetLink, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to deliver a password-reset email. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            }
        }

        return Accepted(new { message = "If an account exists for that email, a password-reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting(RateLimitPolicyNames.PasswordRecovery)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return BadRequest(new { message = "The password-reset link is invalid or has expired." });
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "The password-reset link is invalid or has expired." });
        }

        await refreshTokenService.RevokeUserAsync(user);
        DeleteRefreshTokenCookie();
        try
        {
            await emailSender.SendPasswordChangedAsync(user, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to deliver the password-changed email. TraceId: {TraceId}", HttpContext.TraceIdentifier);
        }
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await CreateUserResponse(user));
    }

    private async Task<AuthResponse> CreateResponse(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = tokenService.Create(user, roles.ToArray());
        return new AuthResponse(token, expiresAtUtc, await CreateUserResponse(user, roles.ToArray()));
    }

    private async Task<AuthResponse> CreateSessionResponse(ApplicationUser user)
    {
        var refreshToken = await refreshTokenService.IssueAsync(user);
        Response.Cookies.Append(RefreshTokenCookie, refreshToken, CreateRefreshTokenCookieOptions());
        return await CreateResponse(user);
    }

    private CookieOptions CreateRefreshTokenCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/api/auth",
        Expires = DateTimeOffset.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7))
    };

    private void DeleteRefreshTokenCookie() =>
        Response.Cookies.Delete(RefreshTokenCookie, CreateRefreshTokenCookieOptions());

    private async Task<UserResponse> CreateUserResponse(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return await CreateUserResponse(user, roles.ToArray());
    }

    private async Task<UserResponse> CreateUserResponse(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var profileComplete = await dbContext.CandidateProfiles.AnyAsync(x => x.UserId == user.Id && x.IsComplete);
        return new(user.Id, user.FullName, user.Email!, roles, profileComplete);
    }
}
