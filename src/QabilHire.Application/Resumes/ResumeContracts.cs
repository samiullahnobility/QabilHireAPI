using System.ComponentModel.DataAnnotations;

namespace QabilHire.Application.Resumes;

public sealed record ResumeUploadRequest(
    [Required] string FileName,
    [Required] string ContentType,
    long SizeBytes);

public sealed record ResumeResponse(
    Guid Id,
    string FileName,
    string DisplayName,
    string? TargetRole,
    string StorageBucket,
    string StoragePath,
    string ContentType,
    long SizeBytes,
    string Extension,
    string Status,
    string? OriginalText,
    string? ExtractedJson,
    string? AnalysisJson,
    int? Score,
    bool IsActive,
    bool IsArchived,
    int ParserVersion,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record ResumeExtractedDataRequest(string ExtractedJson);
public sealed record ResumeMetadataRequest([Required, MaxLength(120)] string DisplayName, [MaxLength(120)] string? TargetRole);
