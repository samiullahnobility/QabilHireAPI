using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QabilHire.Api.Resumes;
using QabilHire.Application.Resumes;
using QabilHire.Domain.Entities;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Api.Controllers;

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/resumes")]
public sealed class ResumesAnalysisController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ResumeAnalysisService analysisService) : ControllerBase
{
    [HttpPost("{id:guid}/analyze")]
    public async Task<ActionResult<ResumeResponse>> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, cancellationToken);
        if (resume is null) return NotFound();

        var analysis = analysisService.Analyze(resume.OriginalText);
        resume.Score = analysis.Score;
        resume.AnalysisJson = JsonSerializer.Serialize(analysis);
        resume.Status = "Completed";
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(resume));
    }

    private static ResumeResponse ToResponse(Resume resume) =>
        new(resume.Id, resume.FileName, resume.DisplayName, resume.TargetRole, resume.StorageBucket, resume.StoragePath, resume.ContentType, resume.SizeBytes, resume.Extension, resume.Status, resume.OriginalText, resume.ExtractedJson, resume.AnalysisJson, resume.Score, resume.IsActive, resume.IsArchived, resume.ParserVersion, resume.CreatedAtUtc, resume.UpdatedAtUtc);
}
