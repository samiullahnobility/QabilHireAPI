using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QabilHire.Api.JobMatches;
using QabilHire.Application.JobMatches;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Api.Controllers;

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/job-matches")]
public sealed class JobMatchesController(ApplicationDbContext db, UserManager<ApplicationUser> users, GroqJobMatchAnalyzer analyzer) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<JobMatchResponse>>> List(CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Unauthorized();
        var items = await db.JobMatches.AsNoTracking().Where(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobMatchResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Unauthorized();
        var item = await db.JobMatches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, cancellationToken);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<JobMatchResponse>> Create(CreateJobMatchRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Unauthorized();
        var profile = await db.CandidateProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        if (profile is null) return BadRequest(new { message = "Complete your profile before analyzing a job match." });
        var resume = await db.Resumes.AsNoTracking().Where(x => x.UserId == user.Id && !x.IsDeleted && !x.IsArchived).OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        var profileContext = $"Headline: {profile.Headline}\nRole: {profile.CurrentRole}\nExperience: {profile.ExperienceLevel}; {profile.ExperienceDuration}\nEducation: {profile.Qualification}; {profile.Institution}\nSkills: {string.Join(", ", profile.Skills)}\nResponsibilities: {profile.Responsibilities}\nCareer goal: {profile.CareerGoal}";
        var resumeContext = resume?.ExtractedJson ?? resume?.OriginalText ?? "No resume evidence supplied.";
        var ai = await analyzer.AnalyzeAsync(profileContext, resumeContext, request.TargetJobTitle.Trim(), request.Company?.Trim(), request.JobDescription.Trim(), cancellationToken);
        if (ai is null) return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Job match analysis is temporarily unavailable. Please try again." });
        var now = DateTime.UtcNow;
        var item = new QabilHire.Domain.Entities.JobMatch { Id = Guid.NewGuid(), UserId = user.Id, TargetJobTitle = request.TargetJobTitle.Trim(), Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim(), JobDescription = request.JobDescription.Trim(), OverallScore = Clamp(ai.OverallScore), MatchLevel = Level(Clamp(ai.OverallScore)), TechnicalScore = Clamp(ai.CategoryScores.Technical), ExperienceScore = Clamp(ai.CategoryScores.Experience), EducationScore = Clamp(ai.CategoryScores.Education), ToolsScore = Clamp(ai.CategoryScores.Tools), SoftSkillsScore = Clamp(ai.CategoryScores.SoftSkills), MatchedSkillsJson = JsonSerializer.Serialize(Clean(ai.MatchedSkills)), MatchedStrengthsJson = JsonSerializer.Serialize(Clean(ai.MatchedStrengths)), GapsJson = JsonSerializer.Serialize(Clean(ai.Gaps)), PrioritiesJson = JsonSerializer.Serialize(Clean(ai.Priorities)), LikelyQuestionsJson = JsonSerializer.Serialize(Clean(ai.LikelyQuestions)), Summary = Trim(ai.Summary, 2000), RecommendedNextStep = Trim(ai.RecommendedNextStep, 500), CreatedAtUtc = now, UpdatedAtUtc = now };
        db.JobMatches.Add(item); await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Unauthorized();
        var item = await db.JobMatches.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, cancellationToken);
        if (item is null) return NotFound(); db.JobMatches.Remove(item); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private static int Clamp(int value) => Math.Clamp(value, 0, 100);
    private static string Level(int score) => score >= 75 ? "Strong Match" : score >= 50 ? "Developing Match" : "Limited Match";
    private static string Trim(string? value, int max) => (value ?? string.Empty).Trim()[..Math.Min((value ?? string.Empty).Trim().Length, max)];
    private static IReadOnlyCollection<string> Clean(IEnumerable<string>? values) => (values ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToArray();
    private static JobMatchResponse ToResponse(QabilHire.Domain.Entities.JobMatch x) => new(x.Id, x.TargetJobTitle, x.Company, x.JobDescription, x.OverallScore, x.MatchLevel, x.TechnicalScore, x.ExperienceScore, x.EducationScore, x.ToolsScore, x.SoftSkillsScore, Read(x.MatchedSkillsJson), Read(x.MatchedStrengthsJson), Read(x.GapsJson), Read(x.PrioritiesJson), Read(x.LikelyQuestionsJson), x.Summary, x.RecommendedNextStep, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static IReadOnlyCollection<string> Read(string json) => JsonSerializer.Deserialize<string[]>(json) ?? [];
}
