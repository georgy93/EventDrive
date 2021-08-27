namespace EventDrive.API.Validations
{
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Mvc;

    public abstract class BaseValidator<TModel> : AbstractValidator<TModel>, IValidatorInterceptor
    {
        protected BaseValidator()
        {
            CascadeMode = CascadeMode.Stop;
        }

        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    error.PropertyName = error.ErrorCode.EndsWith("Validator")
                        ? "InvalidProperty"
                        : error.ErrorCode;
                }
            }

            return result;
        }
    }
}
