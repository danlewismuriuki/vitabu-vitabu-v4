using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class DonateSchoolApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public DonateSchoolApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Donate_listing_dual_confirm_sets_donated()
    {
        var sellerToken = await RegisterAndVerifyAsync($"donate_s_{Guid.NewGuid():N}@example.com");
        var claimerToken = await RegisterAndVerifyAsync($"donate_c_{Guid.NewGuid():N}@example.com");

        using var create = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "Donate School Book",
            grade = "Grade 3",
            subject = "Kiswahili",
            city = "Nairobi",
            intent = "donate_school",
            condition = "good",
            description = "For a nearby primary school drive.",
            cover_image_url = "https://placehold.co/600x800/png?text=Donate"
        });
        var listingRes = await _client.SendAsync(create);
        listingRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var listing = await listingRes.Content.ReadFromJsonAsync<ListingDto>(JsonOptions);
        listing!.Intent.Should().Be("donate_school");
        listing.PriceKes.Should().BeNull();

        var browse = await _client.GetFromJsonAsync<PageDto>("/listings?intent=donate_school&page_size=50", JsonOptions);
        browse!.Items.Should().Contain(i => i.Id == listing.Id);

        using var interestReq = AuthJson(HttpMethod.Post, $"/listings/{listing.Id}/interests", claimerToken, new
        {
            handoff_mode = "meetup",
            city = "Nairobi"
        });
        var interestRes = await _client.SendAsync(interestReq);
        interestRes.EnsureSuccessStatusCode();
        var interest = await interestRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        (await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest!.Id}/accept", sellerToken)))
            .EnsureSuccessStatusCode();
        (await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest.Id}/complete", claimerToken)))
            .EnsureSuccessStatusCode();
        (await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest.Id}/complete", sellerToken)))
            .EnsureSuccessStatusCode();

        var mine = await _client.SendAsync(Auth(HttpMethod.Get, "/me/listings", sellerToken));
        mine.EnsureSuccessStatusCode();
        var page = await mine.Content.ReadFromJsonAsync<PageDto>(JsonOptions);
        page!.Items.Should().Contain(i => i.Id == listing.Id && i.Status == "donated");
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Donate Parent",
            email,
            password = "Password1!",
            city = "Nairobi",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);
        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        var token = auth!.AccessToken;
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";
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

    private sealed record AuthDto(string AccessToken);
    private sealed record OtpDto(string? DevCode);
    private sealed record IdDto(Guid Id);
    private sealed record ListingDto(Guid Id, string Intent, decimal? PriceKes, string Status);
    private sealed record PageDto(IReadOnlyList<ListingDto> Items);
}
