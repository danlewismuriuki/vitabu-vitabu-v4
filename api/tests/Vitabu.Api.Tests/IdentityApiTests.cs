using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class IdentityApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public IdentityApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_login_me_and_phone_otp_flow()
    {
        var email = $"parent_{Guid.NewGuid():N}@example.com";
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";

        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Test Parent",
            email,
            password = "Password1!",
            city = "Nairobi",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);

        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.User.PhoneVerified.Should().BeFalse();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var me = await _client.GetAsync("/auth/me");
        var meBody = await me.Content.ReadAsStringAsync();
        me.StatusCode.Should().Be(HttpStatusCode.OK, "me body: {0}; token prefix: {1}", meBody, auth.AccessToken[..Math.Min(20, auth.AccessToken.Length)]);

        var otpRes = await _client.PostAsJsonAsync("/auth/phone/request-otp", new
        {
            phone_e164 = phone
        }, JsonOptions);
        otpRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var otp = await otpRes.Content.ReadFromJsonAsync<OtpDto>(JsonOptions);
        otp!.DevCode.Should().NotBeNullOrWhiteSpace();

        var verify = await _client.PostAsJsonAsync("/auth/phone/verify-otp", new
        {
            code = otp.DevCode
        }, JsonOptions);
        verify.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await verify.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        profile!.PhoneVerified.Should().BeTrue();
        profile.PhoneE164.Should().Be(phone);

        _client.DefaultRequestHeaders.Authorization = null;
        var login = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Password1!"
        }, JsonOptions);
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_rejects_bad_password()
    {
        var email = $"bad_{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Bad Login",
            email,
            password = "Password1!",
            city = "Kisumu",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);
        register.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "WrongPassword!"
        }, JsonOptions);

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record AuthDto(string AccessToken, UserDto User);
    private sealed record UserDto(bool PhoneVerified, string? PhoneE164);
    private sealed record OtpDto(string? DevCode);
}
