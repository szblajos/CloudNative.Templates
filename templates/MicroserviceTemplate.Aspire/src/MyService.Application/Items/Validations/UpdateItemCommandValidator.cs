using FluentValidation;
using MyService.Application.Items.Commands;

namespace MyService.Application.Items.Validations;

public class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(command => command.Dto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(command => command.Dto.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be a non-negative integer.");
    }
}
