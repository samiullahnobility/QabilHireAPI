namespace QabilHire.Domain.Entities;

public sealed class JobMatch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TargetJobTitle { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public int OverallScore { get; set; }
    public string MatchLevel { get; set; } = "Limited Match";
    public int TechnicalScore { get; set; }
    public int ExperienceScore { get; set; }
    public int EducationScore { get; set; }
    public int ToolsScore { get; set; }
    public int SoftSkillsScore { get; set; }
    public string MatchedSkillsJson { get; set; } = "[]";
    public string MatchedStrengthsJson { get; set; } = "[]";
    public string GapsJson { get; set; } = "[]";
    public string PrioritiesJson { get; set; } = "[]";
    public string LikelyQuestionsJson { get; set; } = "[]";
    public string Summary { get; set; } = string.Empty;
    public string RecommendedNextStep { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
