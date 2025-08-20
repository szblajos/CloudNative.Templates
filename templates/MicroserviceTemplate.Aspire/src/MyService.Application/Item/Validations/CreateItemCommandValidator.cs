
using FluentValidation;
using MyService.Application.Item.Commands;

namespace MyService.Application.Item.Validations;

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