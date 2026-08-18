using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string CandidateRole = "Candidate";

    private static readonly DemoUser[] DemoUsers =
    [
        new("Demo Candidate", "demo@qabilhire.com", "Demo@12345"),
        new("Hackathon Judge", "judge@qabilhire.com", "Judge@12345"),
        new("Sample Candidate", "candidate@qabilhire.com", "Candidate@12345")
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (!await roleManager.RoleExistsAsync(CandidateRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(CandidateRole));
            EnsureSucceeded(roleResult, "create the Candidate role");
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var demoUser in DemoUsers)
        {
            var user = await userManager.FindByEmailAsync(demoUser.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    FullName = demoUser.FullName,
                    Email = demoUser.Email,
                    UserName = demoUser.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, demoUser.Password);
                EnsureSucceeded(createResult, $"create demo user {demoUser.Email}");
            }

            if (!await userManager.IsInRoleAsync(user, CandidateRole))
            {
                var roleResult = await userManager.AddToRoleAsync(user, CandidateRole);
                EnsureSucceeded(roleResult, $"assign the Candidate role to {demoUser.Email}");
            }
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Unable to {operation}: {errors}");
    }

    private sealed record DemoUser(string FullName, string Email, string Password);
}
