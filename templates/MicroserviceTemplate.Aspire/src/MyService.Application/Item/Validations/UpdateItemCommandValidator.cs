using FluentValidation;
using MyService.Application.Item.Commands;

namespace MyService.Application.Item.Validations;

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
