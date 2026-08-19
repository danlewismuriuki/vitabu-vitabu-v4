using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class DealsApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public DealsApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Interest_accept_reserves_and_unlocks_phones()
    {
        var sellerToken = await RegisterAndVerifyAsync($"seller_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAndVerifyAsync($"buyer_{Guid.NewGuid():N}@example.com");

        using var createListing = new HttpRequestMessage(HttpMethod.Post, "/listings")
        {
            Content = JsonContent.Create(new
            {
                title = "Deal Test Book",
                grade = "Grade 5",
                subject = "English",
                city = "Nairobi",
                intent = "sale",
                condition = "good",
                price_kes = 500,
                description = "Book for deal flow test.",
                cover_image_url = "https://placehold.co/600x800/png?text=Deal"
            }, options: JsonOptions)
        };
        createListing.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sellerToken);
        var listingRes = await _client.SendAsync(createListing);
        listingRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var listing = await listingRes.Content.ReadFromJsonAsync<ListingDto>(JsonOptions);

        using var interestReq = new HttpRequestMessage(HttpMethod.Post, $"/listings/{listing!.Id}/interests")
        {
            Content = JsonContent.Create(new
            {
                handoff_mode = "meetup",
                city = "Nairobi",
                message = "Can meet Saturday"
            }, options: JsonOptions)
        };
        interestReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var interestRes = await _client.SendAsync(interestReq);
        interestRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var interest = await interestRes.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);
        interest!.Status.Should().Be("pending");
        interest.Buyer.PhoneE164.Should().BeNull();

        using var acceptReq = new HttpRequestMessage(HttpMethod.Post, $"/interests/{interest.Id}/accept");
        acceptReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sellerToken);
        var acceptRes = await _client.SendAsync(acceptReq);
        acceptRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var accepted = await acceptRes.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);
        accepted!.Status.Should().Be("accepted");
        accepted.Buyer.PhoneE164.Should().NotBeNullOrWhiteSpace();
        accepted.Seller.PhoneE164.Should().NotBeNullOrWhiteSpace();

        var publicGet = await _client.GetAsync($"/listings/{listing.Id}");
        publicGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var notifReq = new HttpRequestMessage(HttpMethod.Get, "/me/notifications");
        notifReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var notifRes = await _client.SendAsync(notifReq);
        notifRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifs = await notifRes.Content.ReadFromJsonAsync<NotifPageDto>(JsonOptions);
        notifs!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Cannot_interest_own_listing()
    {
        var token = await RegisterAndVerifyAsync($"self_{Guid.NewGuid():N}@example.com");
        using var createListing = new HttpRequestMessage(HttpMethod.Post, "/listings")
        {
            Content = JsonContent.Create(new
            {
                title = "Own Book",
                grade = "Grade 3",
                subject = "Kiswahili",
                city = "Kisumu",
                intent = "free",
                condition = "fair",
                description = "Own listing interest blocked.",
                cover_image_url = "https://placehold.co/600x800/png?text=Own"
            }, options: JsonOptions)
        };
        createListing.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listingRes = await _client.SendAsync(createListing);
        var listing = await listingRes.Content.ReadFromJsonAsync<ListingDto>(JsonOptions);

        using var interestReq = new HttpRequestMessage(HttpMethod.Post, $"/listings/{listing!.Id}/interests")
        {
            Content = JsonContent.Create(new
            {
                handoff_mode = "meetup",
                city = "Kisumu"
            }, options: JsonOptions)
        };
        interestReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var interestRes = await _client.SendAsync(interestReq);
        interestRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Deal Parent",
            email,
            password = "Password1!",
            city = "Nairobi",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);
        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        var token = auth!.AccessToken;

        using var otpReq = new HttpRequestMessage(HttpMethod.Post, "/auth/phone/request-otp")
        {
            Content = JsonContent.Create(new { phone_e164 = phone }, options: JsonOptions)
        };
        otpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var otpRes = await _client.SendAsync(otpReq);
        otpRes.EnsureSuccessStatusCode();
        var otp = await otpRes.Content.ReadFromJsonAsync<OtpDto>(JsonOptions);

        using var verifyReq = new HttpRequestMessage(HttpMethod.Post, "/auth/phone/verify-otp")
        {
            Content = JsonContent.Create(new { code = otp!.DevCode }, options: JsonOptions)
        };
        verifyReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await _client.SendAsync(verifyReq)).EnsureSuccessStatusCode();
        return token;
    }

    private sealed record AuthDto(string AccessToken);
    private sealed record OtpDto(string? DevCode);
    private sealed record ListingDto(Guid Id);
    private sealed record InterestDto(Guid Id, string Status, PartyDto Buyer, PartyDto Seller);
    private sealed record PartyDto(string? PhoneE164);
    private sealed record NotifPageDto(List<object> Items);
}
