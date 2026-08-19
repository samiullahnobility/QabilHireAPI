namespace QabilHire.Api.Storage;

public interface ISupabaseStorageService
{
    Task<string> UploadResumeAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream> DownloadResumeAsync(string bucket, string path, CancellationToken cancellationToken);
}
