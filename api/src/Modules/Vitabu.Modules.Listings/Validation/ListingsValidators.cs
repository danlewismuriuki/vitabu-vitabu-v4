using FluentValidation;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Listings.Validation;

public sealed class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Term).MaximumLength(40).When(x => x.Term is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CoverImageUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PriceKes)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Intent == ListingIntent.Sale)
            .WithMessage("Sale listings require a price in KES.");
        RuleFor(x => x.PriceKes)
            .Null()
            .When(x => x.Intent is ListingIntent.Free or ListingIntent.Exchange or ListingIntent.DonateSchool)
            .WithMessage("Free, exchange, and donate_school listings must not include a price.");
    }
}

public sealed class UpdateListingRequestValidator : AbstractValidator<UpdateListingRequest>
{
    public UpdateListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Term).MaximumLength(40).When(x => x.Term is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CoverImageUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PriceKes)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Intent == ListingIntent.Sale)
            .WithMessage("Sale listings require a price in KES.");
        RuleFor(x => x.PriceKes)
            .Null()
            .When(x => x.Intent is ListingIntent.Free or ListingIntent.Exchange or ListingIntent.DonateSchool)
            .WithMessage("Free, exchange, and donate_school listings must not include a price.");
    }
}
