
using FluentValidation;
using MyService.Application.Items.Commands;

namespace MyService.Application.Items.Validations;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(command => command.Item.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(command => command.Item.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be a non-negative integer.");
    }
}