using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QabilHire.Api.Resumes;
using QabilHire.Api.Storage;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;
using QabilHire.Application.Resumes;
using QabilHire.Domain.Entities;

namespace QabilHire.Api.Controllers;

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/resumes")]
public sealed class ResumesExtractController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IResumeTextExtractor textExtractor,
    GroqResumeExtractor groqExtractor,
    ISupabaseStorageService storage) : ControllerBase
{
    [HttpPost("{id:guid}/extract")]
    public async Task<ActionResult<ResumeResponse>> Extract(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, cancellationToken);
        if (resume is null) return NotFound();

        var now = DateTime.UtcNow;
        resume.Status = "Processing";
        resume.UpdatedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await using var resumeStream = await storage.DownloadResumeAsync(resume.StorageBucket, resume.StoragePath, cancellationToken);
        var extractedText = await textExtractor.ExtractAsync(resumeStream, resume.Extension, cancellationToken);

        resume.OriginalText = extractedText;
        var aiExtraction = await groqExtractor.ExtractAsync(extractedText, cancellationToken);
        if (aiExtraction is null)
        {
            resume.Status = "Failed";
            resume.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI extraction failed. Please try again in a moment." });
        }

        resume.ExtractedJson = aiExtraction;
        resume.ParserVersion = 2;
        resume.Status = "Completed";
        resume.UpdatedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(resume));
    }

    private static ResumeResponse ToResponse(Resume resume) =>
        new(resume.Id, resume.FileName, resume.DisplayName, resume.TargetRole, resume.StorageBucket, resume.StoragePath, resume.ContentType, resume.SizeBytes, resume.Extension, resume.Status, resume.OriginalText, resume.ExtractedJson, resume.AnalysisJson, resume.Score, resume.IsActive, resume.IsArchived, resume.ParserVersion, resume.CreatedAtUtc, resume.UpdatedAtUtc);
}
