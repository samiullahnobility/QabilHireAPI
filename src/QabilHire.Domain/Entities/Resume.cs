namespace QabilHire.Domain.Entities;

public sealed class Resume
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
    public string StorageBucket { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Extension { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? OriginalText { get; set; }
    public string? ExtractedJson { get; set; }
    public string? AnalysisJson { get; set; }
    public int? Score { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    public int ParserVersion { get; set; } = 1;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
