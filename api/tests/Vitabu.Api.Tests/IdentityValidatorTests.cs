using FluentAssertions;
using Vitabu.Modules.Identity.Contracts;
using Vitabu.Modules.Identity.Validation;

namespace Vitabu.Api.Tests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    [Fact]
    public async Task Valid_request_passes()
    {
        var result = await _sut.ValidateAsync(new RegisterRequest(
            "Amina Wanjiku",
            "amina@example.com",
            "Password1!",
            "Nairobi",
            true,
            true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Rejects_when_terms_not_accepted()
    {
        var result = await _sut.ValidateAsync(new RegisterRequest(
            "Amina",
            "amina@example.com",
            "Password1!",
            "Nairobi",
            false,
            true));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AcceptTerms");
    }

    [Fact]
    public async Task Rejects_short_password()
    {
        var result = await _sut.ValidateAsync(new RegisterRequest(
            "Amina",
            "amina@example.com",
            "short",
            "Nairobi",
            true,
            true));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}

public class RequestPhoneOtpValidatorTests
{
    private readonly RequestPhoneOtpRequestValidator _sut = new();

    [Theory]
    [InlineData("+254712345678", true)]
    [InlineData("+25471234567", false)]
    [InlineData("0712345678", false)]
    [InlineData("+1234567890", false)]
    public async Task Validates_kenya_mobile(string phone, bool expected)
    {
        var result = await _sut.ValidateAsync(new RequestPhoneOtpRequest(phone));
        result.IsValid.Should().Be(expected);
    }
}
