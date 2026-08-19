namespace QabilHire.Api.Storage;

public sealed class SupabaseStorageOptions
{
    public string Url { get; init; } = string.Empty;
    public string ServiceRoleKey { get; init; } = string.Empty;
    public string ResumesBucket { get; init; } = "resumes";
}
