using System.Text;

namespace QabilHire.Api.Logging;

public sealed class DailyFileLoggerProvider : ILoggerProvider
{
    private readonly string directory;
    private readonly string filePrefix;
    private readonly object writeLock = new();

    public DailyFileLoggerProvider(string contentRootPath, IConfiguration configuration)
    {
        var configuredPath = configuration["Logging:File:Path"] ?? "logs/qabilhire.log";
        var fullPath = Path.GetFullPath(configuredPath, contentRootPath);
        directory = Path.GetDirectoryName(fullPath) ?? contentRootPath;
        filePrefix = Path.GetFileNameWithoutExtension(fullPath);
        Directory.CreateDirectory(directory);
    }

    public ILogger CreateLogger(string categoryName) => new DailyFileLogger(this, categoryName);
    public void Dispose() { }

    internal void Write(LogLevel level, string category, string message, Exception? exception)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var filePath = Path.Combine(directory, $"{filePrefix}-{timestamp:yyyyMMdd}.log");
        var entry = new StringBuilder()
            .Append(timestamp.ToString("O"))
            .Append(" [").Append(level).Append("] ")
            .Append(category).Append(": ")
            .AppendLine(message);

        if (exception is not null)
        {
            entry.AppendLine(exception.ToString());
        }

        lock (writeLock)
        {
            File.AppendAllText(filePath, entry.ToString(), Encoding.UTF8);
        }
    }

    private sealed class DailyFileLogger(DailyFileLoggerProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                provider.Write(logLevel, category, formatter(state, exception), exception);
            }
        }
    }
}
