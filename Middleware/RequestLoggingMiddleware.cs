using System.Diagnostics;

namespace API_tester.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
            stopwatch.Stop();
            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                : context.Response.StatusCode >= 400 ? LogLevel.Warning
                : IsImportantEvent(context.Request.Method) ? LogLevel.Information
                : LogLevel.Debug;
            _logger.Log(level,
                "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs} ms for user {UserId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(exception, "Unhandled error during HTTP {Method} {Path} after {ElapsedMs} ms",
                context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static bool IsImportantEvent(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
}
