using FluentValidation;
using Vitabu.Modules.Deals.Contracts;

namespace Vitabu.Modules.Deals.Validation;

public sealed class CreateInterestRequestValidator : AbstractValidator<CreateInterestRequest>
{
    public CreateInterestRequestValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Message).MaximumLength(2000).When(x => x.Message is not null);
        RuleFor(x => x.HandoffMode).IsInEnum();
    }
}
