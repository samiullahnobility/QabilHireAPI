namespace QabilHire.Api.Resumes;

public interface IResumeTextExtractor
{
    Task<string> ExtractAsync(Stream stream, string extension, CancellationToken cancellationToken);
}
