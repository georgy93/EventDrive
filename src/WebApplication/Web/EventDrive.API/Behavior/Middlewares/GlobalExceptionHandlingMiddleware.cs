namespace EventDrive.API.Behavior.Middlewares;

using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Settings;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Utils.Extensions;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IOptionsMonitor<ErrorHandlingSettings> _errorHandlingSettings;
    private readonly IActionResultExecutor<ObjectResult> _executor;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next,
                                             IOptionsMonitor<ErrorHandlingSettings> errorHandlingSettings,
                                             IActionResultExecutor<ObjectResult> executor,
                                             ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _errorHandlingSettings = errorHandlingSettings;
        _executor = executor;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Path: {Path}", context.Request.Path);

        ReThrowIfResponseHasStarted(context, exception);

        var errorResponse = CreateDefaultErrorResponse(exception);

        await context.WriteResultAsync(_executor, errorResponse, StatusCodes.Status500InternalServerError);
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