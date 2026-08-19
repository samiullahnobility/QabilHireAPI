using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QabilHire.Api.Storage;
using QabilHire.Application.Resumes;
using QabilHire.Domain.Entities;
using QabilHire.Infrastructure.Identity;
using QabilHire.Infrastructure.Persistence;

namespace QabilHire.Api.Controllers;

[Serializable]
public sealed class ResumeUploadForm
{
    public IFormFile File { get; set; } = default!;
}

[ApiController, Authorize(Roles = "Candidate")]
[Route("api/resumes")]
public sealed class ResumesController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ISupabaseStorageService storage,
    IOptions<SupabaseStorageOptions> storageOptions) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx" };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResumeResponse>> Upload([FromForm] ResumeUploadForm request, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var file = request.File;
        if (file is null || file.Length == 0) return BadRequest(new { message = "Please choose a resume file." });
        if (file.Length > MaxFileSizeBytes) return BadRequest(new { message = "Resume must be 10 MB or smaller." });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension)) return BadRequest(new { message = "Only PDF and DOCX files are allowed." });
        if (!AllowedContentTypes.Contains(file.ContentType)) return BadRequest(new { message = "Only PDF and DOCX files are allowed." });

        var now = DateTime.UtcNow;
        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FileName = Path.GetFileName(file.FileName),
            DisplayName = Path.GetFileNameWithoutExtension(file.FileName),
            StorageBucket = storageOptions.Value.ResumesBucket,
            StoragePath = $"{user.Id}/{Guid.NewGuid()}/{Path.GetFileName(file.FileName)}",
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            Extension = extension.ToLowerInvariant(),
            Status = "Uploaded",
            IsActive = !await dbContext.Resumes.AnyAsync(x => x.UserId == user.Id && x.IsActive && !x.IsDeleted, cancellationToken),
            CreatedAtUtc = now
        };

        await using var stream = file.OpenReadStream();
        await storage.UploadResumeAsync(resume.StorageBucket, resume.StoragePath, stream, file.ContentType, cancellationToken);
        dbContext.Resumes.Add(resume);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(resume));
    }

    private static ResumeResponse ToResponse(Resume resume) =>
        new(resume.Id, resume.FileName, resume.DisplayName, resume.TargetRole, resume.StorageBucket, resume.StoragePath, resume.ContentType, resume.SizeBytes, resume.Extension, resume.Status, resume.OriginalText, resume.ExtractedJson, resume.AnalysisJson, resume.Score, resume.IsActive, resume.IsArchived, resume.ParserVersion, resume.CreatedAtUtc, resume.UpdatedAtUtc);
}
