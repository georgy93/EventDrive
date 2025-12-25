namespace EventDrive.API.Behavior.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class AutoFluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AutoFluentValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var (_, arg) in context.ActionArguments.Where(a => a.Value is not null))
        {
            if (TryGetValidator(arg, out var validator))
                ValidateAndThrow(arg, validator);
        }

        await next();
    }

    private bool TryGetValidator(object arg, out IValidator validator)
    {
        validator = null;

        var argType = arg.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(argType);

        var validatorObj = _serviceProvider.GetService(validatorType);
        if (validatorObj is null)
            return false;

        validator = (IValidator)validatorObj;

        return true;
    }

    private static void ValidateAndThrow(object arg, IValidator validator)
    {
        var validationContext = new ValidationContext<object>(arg);

        var result = validator.Validate(validationContext);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
}