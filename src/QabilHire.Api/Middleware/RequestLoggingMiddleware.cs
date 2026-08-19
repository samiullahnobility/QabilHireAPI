using System.Diagnostics;

namespace QabilHire.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);

            stopwatch.Stop();
            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "HTTP {Method} {Path} failed after {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.Elapsed.TotalMilliseconds,
                context.TraceIdentifier);
            throw;
        }
    }
}
