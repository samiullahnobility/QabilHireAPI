using System.Text.RegularExpressions;

namespace QabilHire.Api.Resumes;

public sealed class ResumeAnalysisService
{
    public ResumeAnalysisResult Analyze(string? text)
    {
        text ??= string.Empty;
        var lower = text.ToLowerInvariant();
        var hasContact = Regex.IsMatch(text, @"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}");
        var hasMetrics = Regex.IsMatch(text, @"\b\d+(?:\.\d+)?%|[$£€]\s?\d|\b\d+\+\b");
        var hasActionLanguage = Regex.IsMatch(lower, @"\b(achieved|built|created|delivered|designed|developed|improved|increased|launched|led|managed|reduced|resolved|saved|implemented|coordinated|trained|supported)\b");
        var hasExperience = lower.Contains("experience") || lower.Contains("work history") || lower.Contains("employment");
        var hasEducation = lower.Contains("education") || lower.Contains("degree") || lower.Contains("university") || lower.Contains("college");
        var hasSkills = lower.Contains("skills") || lower.Contains("competencies") || lower.Contains("expertise");
        var hasSummary = lower.Contains("summary") || lower.Contains("profile") || lower.Contains("objective");
        var usefulLength = text.Length is >= 600 and <= 12000;
        var score = 20 + (hasContact ? 10 : 0) + (hasExperience ? 18 : 0) + (hasEducation ? 10 : 0)
            + (hasSkills ? 12 : 0) + (hasSummary ? 8 : 0) + (hasMetrics ? 12 : 0)
            + (hasActionLanguage ? 5 : 0) + (usefulLength ? 5 : 0);
        score = Math.Clamp(score, 0, 100);

        var strengths = new List<string>();
        if (hasMetrics) strengths.Add("Includes measurable impact.");
        if (hasExperience) strengths.Add("Shows work experience context.");
        if (hasActionLanguage) strengths.Add("Uses action-oriented achievement language.");
        if (hasSkills) strengths.Add("Provides a dedicated skills or competencies section.");

        var suggestions = new List<string>();
        if (!hasContact) suggestions.Add("Add clear contact information, including a professional email address.");
        if (!hasMetrics) suggestions.Add("Add quantified results to your experience bullets.");
        if (!hasSkills) suggestions.Add("Add a dedicated skills or competencies section relevant to your profession.");
        if (!hasSummary) suggestions.Add("Add a concise professional summary tailored to your target role.");
        if (!hasActionLanguage) suggestions.Add("Start achievement bullets with clear action verbs.");

        return new ResumeAnalysisResult(
            score,
            strengths,
            Array.Empty<string>(),
            suggestions,
            new { hasContact, hasMetrics, hasActionLanguage, hasExperience, hasEducation, hasSkills, hasSummary, usefulLength });
    }
}

public sealed record ResumeAnalysisResult(
    int Score,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<string> MissingKeywords,
    IReadOnlyCollection<string> Suggestions,
    object Signals);
