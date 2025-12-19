namespace EventDrive.API.Validations;

using FluentValidation;

public abstract class BaseValidator<TModel> : AbstractValidator<TModel>
{
    protected BaseValidator()
    {
    }
}