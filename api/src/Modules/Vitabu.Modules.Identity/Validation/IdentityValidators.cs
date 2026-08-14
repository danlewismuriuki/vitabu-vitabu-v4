using FluentValidation;
using Vitabu.Modules.Identity.Contracts;

namespace Vitabu.Modules.Identity.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.City).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.AcceptTerms).Equal(true).WithMessage("You must accept the Terms and Privacy policy.");
        RuleFor(x => x.ConfirmParentGuardian).Equal(true)
            .WithMessage("You must confirm you are 18+ / a parent or guardian.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class RequestPhoneOtpRequestValidator : AbstractValidator<RequestPhoneOtpRequest>
{
    public RequestPhoneOtpRequestValidator()
    {
        RuleFor(x => x.PhoneE164)
            .NotEmpty()
            .Matches(@"^\+2547\d{8}$")
            .WithMessage("Phone must be a Kenyan mobile in E.164 form, e.g. +254712345678.");
    }
}

public sealed class VerifyPhoneOtpRequestValidator : AbstractValidator<VerifyPhoneOtpRequest>
{
    public VerifyPhoneOtpRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Matches(@"^\d{6}$").WithMessage("Code must be 6 digits.");
    }
}
