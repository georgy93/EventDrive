namespace EventDrive.API.Behavior.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class AutoFluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AutoFluentValidationFilter(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var (_, arg) in context.ActionArguments)
        {
            if (arg is null)
                continue;

            var argType = arg.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argType);

            var validatorObj = _serviceProvider.GetService(validatorType);
            if (validatorObj is null)
                continue;

            var validator = (IValidator)validatorObj;

            var validationContext = new ValidationContext<object>(arg);
            var result = validator.Validate(validationContext);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        await next();
    }
}
