namespace EventDrive.API.Behavior.Middlewares;

using Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

public class ModelValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var validationError = validationException.Errors.First();

        var response = new ErrorResponse
        {
            ErrorCode = validationError.ErrorCode.EndsWith("Validator")
                ? "InvalidProperty"
                : validationError.ErrorCode,
            Description = validationError.ErrorMessage
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}