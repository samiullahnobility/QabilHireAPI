using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace QabilHire.Api.Resumes;

public sealed class ResumeTextExtractor : IResumeTextExtractor
{
    public async Task<string> ExtractAsync(Stream stream, string extension, CancellationToken cancellationToken)
    {
        extension = extension.ToLowerInvariant();
        return extension switch
        {
            ".docx" => await ExtractDocxAsync(stream, cancellationToken),
            ".pdf" => await ExtractPdfAsync(stream, cancellationToken),
            _ => throw new NotSupportedException("Only PDF and DOCX resumes are supported.")
        };
    }

    private static async Task<string> ExtractDocxAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.GetEntry("word/document.xml") ?? throw new InvalidOperationException("The DOCX file is invalid.");
        await using var xmlStream = entry.Open();
        using var reader = new StreamReader(xmlStream, Encoding.UTF8);
        var xml = await reader.ReadToEndAsync(cancellationToken);
        return CleanupXmlText(xml);
    }

    private static async Task<string> ExtractPdfAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var seekableStream = new MemoryStream();
        await stream.CopyToAsync(seekableStream, cancellationToken);
        seekableStream.Position = 0;
        using var document = PdfDocument.Open(seekableStream);
        return CleanupPlainText(string.Join("\n\n", document.GetPages().Select(page =>
        {
            var words = page.GetWords().OrderByDescending(word => word.BoundingBox.Bottom).ThenBy(word => word.BoundingBox.Left).ToArray();
            var lines = new List<List<UglyToad.PdfPig.Content.Word>>();
            foreach (var word in words)
            {
                var line = lines.FirstOrDefault(candidate => Math.Abs(candidate[0].BoundingBox.Bottom - word.BoundingBox.Bottom) <= 2.5);
                if (line is null) lines.Add([word]); else line.Add(word);
            }
            return string.Join('\n', lines.Select(line => string.Join(' ', line.OrderBy(word => word.BoundingBox.Left).Select(word => word.Text))));
        })));
    }

    private static string CleanupXmlText(string xml)
    {
        var text = Regex.Replace(xml, @"<w:tab/>", "\t");
        text = Regex.Replace(text, @"<w:br/>", "\n");
        text = Regex.Replace(text, @"</w:p>", "\n");
        text = Regex.Replace(text, @"<[^>]+>", " ");
        return CleanupPlainText(text);
    }

    private static string CleanupPlainText(string text)
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        text = Regex.Replace(text, @"[^\S\n]+", " ");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return string.Join('\n', text.Split('\n').Select(line => line.Trim())).Trim();
    }
}
