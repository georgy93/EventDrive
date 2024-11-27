namespace EventDrive.API.Validations;

using DTOs;
using DTOs.Commands;
using FluentValidation;

public class AddItemsCommandValidator : BaseValidator<AddItemsCommand>
{
    public AddItemsCommandValidator()
    {
        RuleFor(command => command.Items)
            .NotNull()
            .NotEmpty()
            .WithMessage($"the {nameof(AddItemsCommand.Items)} collection must not be null or empty");

        RuleForEach(command => command.Items)
            .SetValidator(new MyDtoValidator());
    }

    private sealed class MyDtoValidator : AbstractValidator<MyDto>
    {
        public MyDtoValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty()
                .WithMessage("the list of items contains invalid ID");

            RuleFor(e => e.Name)
                .NotEmpty()
                .WithMessage("the list of items contains invalid name");
        }
    }
}