namespace QabilHire.Domain.Entities;

public sealed class CandidateProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Company { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;
    public string Achievement { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string GraduationYear { get; set; } = string.Empty;
    public string ExperienceDuration { get; set; } = string.Empty;
    public string SkillLevel { get; set; } = string.Empty;
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string TargetRole { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> InterviewPreferences { get; set; } = [];
    public string CareerGoal { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
