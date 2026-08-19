using FluentValidation;
using Vitabu.Modules.Deals.Contracts;
using Vitabu.Modules.Deals.Domain;

namespace Vitabu.Modules.Deals.Validation;

public sealed class CreateInterestRequestValidator : AbstractValidator<CreateInterestRequest>
{
    public CreateInterestRequestValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Message).MaximumLength(2000).When(x => x.Message is not null);
        RuleFor(x => x.HandoffMode).IsInEnum();
        RuleFor(x => x.MtaaniAgentId)
            .NotNull()
            .When(x => x.HandoffMode == HandoffMode.PickupMtaani)
            .WithMessage("mtaani_agent_id is required for pickup_mtaani handoff.");
        RuleFor(x => x.MtaaniAgentId)
            .Null()
            .When(x => x.HandoffMode == HandoffMode.Meetup)
            .WithMessage("mtaani_agent_id is only allowed for pickup_mtaani handoff.");
    }
}
