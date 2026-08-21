using System.ComponentModel.DataAnnotations;

namespace QabilHire.Application.JobMatches;

public sealed record CreateJobMatchRequest(
    [Required, StringLength(160)] string TargetJobTitle,
    [StringLength(160)] string? Company,
    [Required, StringLength(12000, MinimumLength = 40)] string JobDescription);

public sealed record JobMatchResponse(
    Guid Id, string TargetJobTitle, string? Company, string JobDescription, int OverallScore,
    string MatchLevel, int TechnicalScore, int ExperienceScore, int EducationScore, int ToolsScore,
    int SoftSkillsScore, IReadOnlyCollection<string> MatchedSkills, IReadOnlyCollection<string> MatchedStrengths,
    IReadOnlyCollection<string> Gaps, IReadOnlyCollection<string> Priorities,
    IReadOnlyCollection<string> LikelyQuestions, string Summary, string RecommendedNextStep,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public sealed record JobMatchAiResult(
    int OverallScore, string MatchLevel, string Summary, JobMatchCategoryScores CategoryScores,
    IReadOnlyCollection<string> MatchedSkills, IReadOnlyCollection<string> MatchedStrengths,
    IReadOnlyCollection<string> Gaps, IReadOnlyCollection<string> Priorities,
    IReadOnlyCollection<string> LikelyQuestions, string RecommendedNextStep);

public sealed record JobMatchCategoryScores(int Technical, int Experience, int Education, int Tools, int SoftSkills);
