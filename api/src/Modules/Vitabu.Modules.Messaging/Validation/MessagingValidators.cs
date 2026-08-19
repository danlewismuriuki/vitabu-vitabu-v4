using FluentValidation;
using Vitabu.Modules.Messaging.Contracts;

namespace Vitabu.Modules.Messaging.Validation;

public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}
