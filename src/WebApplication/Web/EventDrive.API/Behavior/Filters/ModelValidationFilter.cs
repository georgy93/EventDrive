namespace EventDrive.API.Behavior.Filters;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class ModelValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            await next();
            return;
        }

        var validationError = context.ModelState.First(x => x.Value.ValidationState == ModelValidationState.Invalid);

        context.Result = new BadRequestObjectResult(new ErrorResponse
        {
            ErrorCode = validationError.Key,
            Description = validationError.Value.Errors[0].ErrorMessage
        });
    }
}