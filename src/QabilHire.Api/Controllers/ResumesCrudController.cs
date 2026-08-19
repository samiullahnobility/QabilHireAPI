using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QabilHire.Application.Resumes;
using QabilHire.Domain.Entities;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Api.Controllers;

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/resumes")]
public sealed class ResumesCrudController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResumeResponse>>> List(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resumes = await dbContext.Resumes.AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsDeleted)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
        return Ok(resumes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResumeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resume = await dbContext.Resumes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && !x.IsDeleted, cancellationToken);
        return resume is null ? NotFound() : Ok(ToResponse(resume));
    }

    [HttpPut("{id:guid}/extracted-data")]
    public async Task<ActionResult<ResumeResponse>> UpdateExtractedData(Guid id, ResumeExtractedDataRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && !x.IsDeleted, cancellationToken);
        if (resume is null) return NotFound();
        resume.ExtractedJson = request.ExtractedJson;
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(resume));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && !x.IsDeleted, cancellationToken);
        if (resume is null) return NotFound();
        resume.IsDeleted = true;
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/metadata")]
    public async Task<ActionResult<ResumeResponse>> UpdateMetadata(Guid id, ResumeMetadataRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && !x.IsDeleted, cancellationToken);
        if (resume is null) return NotFound();
        resume.DisplayName = request.DisplayName.Trim();
        resume.TargetRole = string.IsNullOrWhiteSpace(request.TargetRole) ? null : request.TargetRole.Trim();
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(resume));
    }

    [HttpPut("{id:guid}/active")]
    public async Task<ActionResult<ResumeResponse>> SetActive(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resumes = await dbContext.Resumes.Where(x => x.UserId == user.Id && !x.IsDeleted).ToListAsync(cancellationToken);
        var resume = resumes.SingleOrDefault(x => x.Id == id);
        if (resume is null) return NotFound();
        foreach (var item in resumes) item.IsActive = item.Id == id;
        resume.IsArchived = false;
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(resume));
    }

    [HttpPut("{id:guid}/archive")]
    public async Task<ActionResult<ResumeResponse>> ToggleArchive(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var resume = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && !x.IsDeleted, cancellationToken);
        if (resume is null) return NotFound();
        resume.IsArchived = !resume.IsArchived;
        if (resume.IsArchived) resume.IsActive = false;
        resume.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(resume));
    }

    private static ResumeResponse ToResponse(Resume resume) =>
        new(resume.Id, resume.FileName, resume.DisplayName, resume.TargetRole, resume.StorageBucket, resume.StoragePath, resume.ContentType, resume.SizeBytes, resume.Extension, resume.Status, resume.OriginalText, resume.ExtractedJson, resume.AnalysisJson, resume.Score, resume.IsActive, resume.IsArchived, resume.ParserVersion, resume.CreatedAtUtc, resume.UpdatedAtUtc);
}
