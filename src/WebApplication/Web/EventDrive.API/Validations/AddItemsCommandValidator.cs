namespace EventDrive.API.Validations
{
    using DTOs.Commands;
    using FluentValidation;

    public class AddItemsCommandValidator : BaseValidator<AddItemsCommand>
    {
        public AddItemsCommandValidator()
        {
            RuleFor(command => command.Items)
                .NotEmpty()
                .WithMessage("Please fill all items");

            RuleForEach(command => command.Items)
                .NotEmpty()
                .WithMessage("Null elements are not allowed");

        }
    }
}