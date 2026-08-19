using System.Text.RegularExpressions;

namespace QabilHire.Api.Resumes;

public sealed class ResumeStructuredExtractor
{
    private static readonly Dictionary<string, string[]> SectionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["summary"] = ["summary", "profile", "professional summary", "career summary", "executive summary", "professional profile", "personal profile", "objective", "career objective", "about me", "overview"],
        ["skills"] = ["skills", "core skills", "key skills", "professional skills", "technical skills", "soft skills", "core competencies", "areas of expertise", "expertise", "competencies", "technologies", "tech stack", "tools", "tools and technologies"],
        ["experience"] = ["experience", "work experience", "professional experience", "relevant experience", "career experience", "employment", "employment history", "work history", "career history", "professional background", "internships", "freelance experience"],
        ["education"] = ["education", "academic background", "academic history", "education and training", "qualifications", "academic qualifications", "degrees", "coursework"],
        ["projects"] = ["projects", "project highlights", "selected projects", "key projects", "professional projects", "personal projects", "portfolio", "case studies"],
        ["certifications"] = ["certifications", "certificates", "licenses", "licenses and certifications", "professional development", "courses", "training", "training and certifications"],
        ["languages"] = ["languages", "language proficiency", "language skills", "spoken languages"],
        ["awards"] = ["awards", "honors", "honors and awards", "achievements", "accomplishments", "publications", "research", "volunteering", "volunteer experience", "community involvement", "professional memberships", "affiliations", "interests", "work style", "services", "best fit for", "references", "additional information"]
    };

    public object Extract(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var sections = SplitSections(text);
        var email = Regex.Match(text, @"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}").Value;
        var phone = Regex.Match(text, @"(?<!\d)(?:\+?\d[\d ().-]{7,}\d)").Value.Trim();
        var linkedIn = Regex.Match(text, @"(?:https?://)?(?:www\.)?linkedin\.com/in/[\w-]+", RegexOptions.IgnoreCase).Value;
        var website = Regex.Match(text, @"https?://(?![^\s]*linkedin\.com)[^\s]+", RegexOptions.IgnoreCase).Value.TrimEnd('.', ',');

        return new
        {
            contact = new { name = GuessName(lines), email, phone, linkedIn, website },
            summary = Join(sections, "summary"),
            skills = Items(sections, "skills"),
            experience = Entries(sections, "experience"),
            education = Entries(sections, "education"),
            projects = Entries(sections, "projects"),
            certifications = Items(sections, "certifications"),
            languages = Items(sections, "languages"),
            additional = Entries(sections, "awards")
        };
    }

    private static Dictionary<string, List<string>> SplitSections(string text)
    {
        var result = SectionNames.Keys.ToDictionary(key => key, _ => new List<string>());
        var aliases = SectionNames.SelectMany(section => section.Value.Select(alias => new
        {
            Section = section.Key,
            Heading = alias.ToUpperInvariant()
        })).OrderByDescending(item => item.Heading.Length).ToArray();
        var headingPattern = string.Join('|', aliases.Select(item => Regex.Escape(item.Heading)).Distinct());
        var matches = Regex.Matches(text, headingPattern, RegexOptions.CultureInvariant);

        for (var index = 0; index < matches.Count; index++)
        {
            var match = matches[index];
            var section = aliases.First(item => item.Heading == match.Value).Section;
            var end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
            var content = text[(match.Index + match.Length)..end].Trim(' ', ':', '-', '\r', '\n');
            content = Regex.Replace(content, @"(?<!^)(Backend|Frontend|Database|Tools|Integrations):", "\n$1:");
            result[section].AddRange(content.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        }
        return result;
    }

    private static string GuessName(string[] lines)
    {
        var first = lines.FirstOrDefault() ?? string.Empty;
        var compactName = Regex.Match(first, @"^([A-Z][a-z]+(?:\s+[A-Z][a-z]+){0,3})(?=[A-Z])");
        if (compactName.Success) return compactName.Groups[1].Value;
        return lines.FirstOrDefault(line => line.Length is >= 3 and <= 60 && !line.Contains('@') && !Regex.IsMatch(line, @"\d{3}")) ?? string.Empty;
    }

    private static string Join(Dictionary<string, List<string>> sections, string key) => string.Join(" ", sections[key]);

    private static string[] Items(Dictionary<string, List<string>> sections, string key) => sections[key]
        .SelectMany(line => Regex.Split(line.TrimStart('-', '•', ' '), @"\s*[,;|•]\s*"))
        .Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string[] Entries(Dictionary<string, List<string>> sections, string key) => sections[key]
        .Select(line => line.TrimStart('-', '•', ' ')).Where(line => line.Length > 1).ToArray();
}
