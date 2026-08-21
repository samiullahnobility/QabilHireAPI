using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QabilHire.Application.Profiles;
using QabilHire.Domain.Entities;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Api.Controllers;

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/profile")]
public sealed class ProfileController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CandidateProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var profile = await dbContext.CandidateProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        return profile is null ? NotFound() : Ok(ToResponse(profile));
    }

    [HttpPut]
    public async Task<ActionResult<CandidateProfileResponse>> Upsert(UpsertCandidateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var profile = await dbContext.CandidateProfiles.SingleOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        var now = DateTime.UtcNow;
        if (profile is null)
        {
            profile = new CandidateProfile { Id = Guid.NewGuid(), UserId = user.Id, CreatedAtUtc = now };
            dbContext.CandidateProfiles.Add(profile);
        }
        Apply(profile, request, now);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(profile));
    }

    private static void Apply(CandidateProfile p, UpsertCandidateProfileRequest r, DateTime now)
    {
        p.Headline=r.Headline.Trim(); p.ExperienceLevel=r.ExperienceLevel.Trim(); p.Education=r.Education.Trim(); p.CurrentRole=r.CurrentRole.Trim();
        p.Skills=r.Skills.Select(x=>x.Trim()).Where(x=>x.Length>0).Distinct(StringComparer.OrdinalIgnoreCase).Take(50).ToList();
        p.Company=r.Company.Trim(); p.Responsibilities=r.Responsibilities.Trim(); p.Achievement=r.Achievement.Trim(); p.Institution=r.Institution.Trim(); p.Qualification=r.Qualification.Trim(); p.GraduationYear=r.GraduationYear.Trim(); p.ExperienceDuration=r.ExperienceDuration.Trim(); p.SkillLevel=r.SkillLevel?.Trim() ?? string.Empty;
        p.LinkedInUrl=Clean(r.LinkedInUrl); p.PortfolioUrl=Clean(r.PortfolioUrl); p.TargetRole=r.TargetRole.Trim(); p.Industry=r.Industry.Trim(); p.Location=r.Location.Trim();
        p.InterviewPreferences=r.InterviewPreferences.Select(x=>x.Trim()).Where(x=>x.Length>0).Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToList();
        p.CareerGoal=r.CareerGoal.Trim(); p.IsComplete=true; p.UpdatedAtUtc=now;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static CandidateProfileResponse ToResponse(CandidateProfile p) => new(p.Id,p.Headline,p.ExperienceLevel,p.Education,p.CurrentRole,p.Skills,p.Company,p.Responsibilities,p.Achievement,p.Institution,p.Qualification,p.GraduationYear,p.ExperienceDuration,p.SkillLevel,p.LinkedInUrl,p.PortfolioUrl,p.TargetRole,p.Industry,p.Location,p.InterviewPreferences,p.CareerGoal,p.IsComplete,p.UpdatedAtUtc);
}
