using System.ComponentModel.DataAnnotations;

namespace QabilHire.Application.Profiles;

public sealed record UpsertCandidateProfileRequest(
    [Required, StringLength(120)] string Headline,
    [Required, StringLength(80)] string ExperienceLevel,
    [Required, StringLength(120)] string Education,
    [Required, StringLength(120)] string CurrentRole,
    IReadOnlyCollection<string> Skills,
    [Required, StringLength(120)] string Company,
    [Required, StringLength(2000)] string Responsibilities,
    [Required, StringLength(1000)] string Achievement,
    [Required, StringLength(160)] string Institution,
    [Required, StringLength(160)] string Qualification,
    [Required, StringLength(20)] string GraduationYear,
    [Required, StringLength(60)] string ExperienceDuration,
    [StringLength(40)] string? SkillLevel,
    [OptionalUrl, StringLength(500)] string? LinkedInUrl,
    [OptionalUrl, StringLength(500)] string? PortfolioUrl,
    [Required, StringLength(120)] string TargetRole,
    [Required, StringLength(120)] string Industry,
    [Required, StringLength(120)] string Location,
    IReadOnlyCollection<string> InterviewPreferences,
    [Required, StringLength(1000)] string CareerGoal);

public sealed record CandidateProfileResponse(
    Guid Id, string Headline, string ExperienceLevel, string Education, string CurrentRole,
    IReadOnlyCollection<string> Skills, string Company, string Responsibilities, string Achievement,
    string Institution, string Qualification, string GraduationYear, string ExperienceDuration, string SkillLevel, string? LinkedInUrl, string? PortfolioUrl,
    string TargetRole, string Industry, string Location, IReadOnlyCollection<string> InterviewPreferences,
    string CareerGoal, bool IsComplete, DateTime UpdatedAtUtc);

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class OptionalUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null || value is string { Length: 0 }) return true;
        if (value is not string text || string.IsNullOrWhiteSpace(text)) return true;
        return Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https" or "ftp";
    }
}
