using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QabilHire.Api.Authentication;
using QabilHire.Application.Authentication;
using QabilHire.Infrastructure.Identity;

namespace QabilHire.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService tokenService) : ControllerBase
{
    private const string CandidateRole = "Candidate";

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
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

        return Ok(await CreateResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(await CreateResponse(user));
    }

    private async Task<AuthResponse> CreateResponse(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = tokenService.Create(user, roles.ToArray());
        return new AuthResponse(token, expiresAtUtc, new UserResponse(user.Id, user.FullName, user.Email!));
    }
}
