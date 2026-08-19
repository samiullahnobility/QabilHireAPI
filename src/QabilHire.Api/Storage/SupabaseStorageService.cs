using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace QabilHire.Api.Storage;

public sealed class SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseStorageOptions> options) : ISupabaseStorageService
{
    private readonly SupabaseStorageOptions storageOptions = options.Value;

    public async Task<string> UploadResumeAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageOptions.Url) || string.IsNullOrWhiteSpace(storageOptions.ServiceRoleKey))
        {
            throw new InvalidOperationException("Supabase storage is not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{storageOptions.Url.TrimEnd('/')}/storage/v1/object/{bucket}/{Uri.EscapeDataString(path)}")
        {
            Content = new StreamContent(content)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", storageOptions.ServiceRoleKey);
        request.Headers.TryAddWithoutValidation("apikey", storageOptions.ServiceRoleKey);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? "Unable to upload resume to storage."
                : $"Unable to upload resume to storage: {error}");
        }

        return path;
    }

    public async Task<Stream> DownloadResumeAsync(string bucket, string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageOptions.Url) || string.IsNullOrWhiteSpace(storageOptions.ServiceRoleKey))
        {
            throw new InvalidOperationException("Supabase storage is not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"{storageOptions.Url.TrimEnd('/')}/storage/v1/object/authenticated/{bucket}/{Uri.EscapeDataString(path)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", storageOptions.ServiceRoleKey);
        request.Headers.TryAddWithoutValidation("apikey", storageOptions.ServiceRoleKey);

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? "Unable to download resume from storage."
                : $"Unable to download resume from storage: {error}");
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
