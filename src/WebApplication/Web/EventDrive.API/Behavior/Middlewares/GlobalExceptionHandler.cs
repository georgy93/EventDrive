namespace EventDrive.API.Behavior.Middlewares;

using Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Settings;
using System.Runtime.ExceptionServices;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IOptionsMonitor<ErrorHandlingSettings> _errorHandlingSettings;

    public GlobalExceptionHandler(IOptionsMonitor<ErrorHandlingSettings> errorHandlingSettings, ILogger<GlobalExceptionHandler> logger)
    {
        _errorHandlingSettings = errorHandlingSettings;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Path: {Path}", httpContext.Request.Path);

        ReThrowIfResponseHasStarted(httpContext, exception);

        var errorResponse = CreateDefaultErrorResponse(exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }

    private void ReThrowIfResponseHasStarted(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning($"Cannot handle error. The response has already started.");

            ExceptionDispatchInfo.Throw(exception);
        }
    }

    private ErrorResponse CreateDefaultErrorResponse(Exception exception) => new()
    {
        ErrorCode = "InternalServerError",
        Description = "InternalServerError",
        Exception = _errorHandlingSettings.CurrentValue.ShowDetails ? exception : null
    };
}