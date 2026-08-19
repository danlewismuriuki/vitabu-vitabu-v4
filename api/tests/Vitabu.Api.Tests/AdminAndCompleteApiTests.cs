using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class AdminAndCompleteApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public AdminAndCompleteApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Dual_confirm_then_rate_and_admin_hide_via_report()
    {
        var sellerToken = await RegisterAndVerifyAsync($"s5_seller_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAndVerifyAsync($"s5_buyer_{Guid.NewGuid():N}@example.com");

        using var createListing = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "S5 Dual Complete Book",
            grade = "Grade 6",
            subject = "Science & Technology",
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 400,
            description = "For dual confirm test.",
            cover_image_url = "https://placehold.co/600x800/png?text=S5"
        });
        var listingRes = await _client.SendAsync(createListing);
        listingRes.EnsureSuccessStatusCode();
        var listing = await listingRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        using var interestReq = AuthJson(HttpMethod.Post, $"/listings/{listing!.Id}/interests", buyerToken, new
        {
            handoff_mode = "meetup",
            city = "Nairobi"
        });
        var interestRes = await _client.SendAsync(interestReq);
        interestRes.EnsureSuccessStatusCode();
        var interest = await interestRes.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);

        (await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest!.Id}/accept", sellerToken)))
            .EnsureSuccessStatusCode();

        var buyerConfirm = await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest.Id}/complete", buyerToken));
        buyerConfirm.StatusCode.Should().Be(HttpStatusCode.OK);
        var mid = await buyerConfirm.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);
        mid!.Status.Should().Be("accepted");
        mid.BuyerCompletedAtUtc.Should().NotBeNull();

        var sellerConfirm = await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest.Id}/complete", sellerToken));
        sellerConfirm.EnsureSuccessStatusCode();
        var done = await sellerConfirm.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);
        done!.Status.Should().Be("completed");

        var rate = await _client.SendAsync(AuthJson(HttpMethod.Post, $"/interests/{interest.Id}/rate", buyerToken, new
        {
            stars = 5,
            comment = "Great meetup"
        }));
        rate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var report = AuthJson(HttpMethod.Post, $"/listings/{listing.Id}/reports", buyerToken, new
        {
            reason = "spam_or_scam",
            details = "test report"
        });
        (await _client.SendAsync(report)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var adminLogin = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "admin@vitabu.local",
            password = "AdminPassword1!"
        }, JsonOptions);
        adminLogin.EnsureSuccessStatusCode();
        var adminAuth = await adminLogin.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        adminAuth!.User.IsStaff.Should().BeTrue();

        using var hide = Auth(HttpMethod.Post, $"/admin/listings/{listing.Id}/hide", adminAuth.AccessToken);
        (await _client.SendAsync(hide)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "S5 Parent",
            email,
            password = "Password1!",
            city = "Nairobi",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);
        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        var token = auth!.AccessToken;

        using var otpReq = AuthJson(HttpMethod.Post, "/auth/phone/request-otp", token, new { phone_e164 = phone });
        var otpRes = await _client.SendAsync(otpReq);
        otpRes.EnsureSuccessStatusCode();
        var otp = await otpRes.Content.ReadFromJsonAsync<OtpDto>(JsonOptions);

        using var verifyReq = AuthJson(HttpMethod.Post, "/auth/phone/verify-otp", token, new { code = otp!.DevCode });
        (await _client.SendAsync(verifyReq)).EnsureSuccessStatusCode();
        return token;
    }

    private static HttpRequestMessage Auth(HttpMethod method, string url, string token)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    private static HttpRequestMessage AuthJson(HttpMethod method, string url, string token, object body)
    {
        var req = Auth(method, url, token);
        req.Content = JsonContent.Create(body, options: JsonOptions);
        return req;
    }

    private sealed record AuthDto(string AccessToken, UserDto User);
    private sealed record UserDto(bool IsStaff);
    private sealed record OtpDto(string? DevCode);
    private sealed record IdDto(Guid Id);
    private sealed record InterestDto(Guid Id, string Status, DateTime? BuyerCompletedAtUtc);
}
