namespace EventDrive.API.Validations
{
    using DTOs.Commands;
    using EventDrive.DTOs;
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
                .SetValidator(new MyDTOValidator());
        }

        private class MyDTOValidator : AbstractValidator<MyDTO>
        {
            public MyDTOValidator()
            {
                RuleFor(e => e.Id)
                    .NotEmpty()
                    .WithMessage("the list of items contains invalid ID"); ;
            }
        }
    }
}