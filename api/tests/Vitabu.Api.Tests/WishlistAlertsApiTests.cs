using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class WishlistAlertsApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public WishlistAlertsApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Similar_listing_and_unavailable_alerts_respect_prefs()
    {
        var sellerToken = await RegisterAndVerifyAsync($"wa_seller_{Guid.NewGuid():N}@example.com");
        var wishlisterToken = await RegisterAsync($"wa_wish_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAndVerifyAsync($"wa_buyer_{Guid.NewGuid():N}@example.com");

        var seedListingId = await CreateListingAsync(sellerToken, "Seed Math Book", "Grade 4", "Mathematics");
        (await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{seedListingId}/wishlist", wishlisterToken)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var similarId = await CreateListingAsync(sellerToken, "Another Math Book", "Grade 4", "Mathematics");
        var similarNotifs = await ListNotificationsAsync(wishlisterToken);
        similarNotifs.Should().Contain(n => n.Type == "wishlist_similar_listing" && n.RelatedEntityId == similarId);

        // Opt out — no further similar alerts
        using var prefsOff = AuthJson(HttpMethod.Patch, "/auth/me/notification-prefs", wishlisterToken,
            new { wishlist_alerts_enabled = false });
        var prefsRes = await _client.SendAsync(prefsOff);
        prefsRes.EnsureSuccessStatusCode();
        var profile = await prefsRes.Content.ReadFromJsonAsync<ProfileDto>(JsonOptions);
        profile!.WishlistAlertsEnabled.Should().BeFalse();

        var ignoredId = await CreateListingAsync(sellerToken, "Ignored Math Book", "Grade 4", "Mathematics");
        var afterOff = await ListNotificationsAsync(wishlisterToken);
        afterOff.Should().NotContain(n => n.RelatedEntityId == ignoredId);

        // Re-enable and wishlist a listing that will be reserved
        using var prefsOn = AuthJson(HttpMethod.Patch, "/auth/me/notification-prefs", wishlisterToken,
            new { wishlist_alerts_enabled = true });
        (await _client.SendAsync(prefsOn)).EnsureSuccessStatusCode();

        var targetId = await CreateListingAsync(sellerToken, "Will Reserve Book", "Grade 5", "English");
        (await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{targetId}/wishlist", wishlisterToken)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var interestReq = AuthJson(HttpMethod.Post, $"/listings/{targetId}/interests", buyerToken, new
        {
            handoff_mode = "meetup",
            city = "Nairobi",
            message = "Ready"
        });
        var interestRes = await _client.SendAsync(interestReq);
        interestRes.EnsureSuccessStatusCode();
        var interest = await interestRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        (await _client.SendAsync(Auth(HttpMethod.Post, $"/interests/{interest!.Id}/accept", sellerToken)))
            .EnsureSuccessStatusCode();

        var unavailable = await ListNotificationsAsync(wishlisterToken);
        unavailable.Should().Contain(n =>
            n.Type == "wishlist_listing_unavailable" && n.RelatedEntityId == targetId);
    }

    private async Task<Guid> CreateListingAsync(string token, string title, string grade, string subject)
    {
        using var req = AuthJson(HttpMethod.Post, "/listings", token, new
        {
            title,
            grade,
            subject,
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 250,
            description = "Wishlist alert test listing.",
            cover_image_url = "https://placehold.co/600x800/png?text=WA"
        });
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var listing = await res.Content.ReadFromJsonAsync<IdDto>(JsonOptions);
        return listing!.Id;
    }

    private async Task<List<NotifDto>> ListNotificationsAsync(string token)
    {
        var res = await _client.SendAsync(Auth(HttpMethod.Get, "/me/notifications?page_size=50", token));
        res.EnsureSuccessStatusCode();
        var page = await res.Content.ReadFromJsonAsync<NotifPageDto>(JsonOptions);
        return page!.Items.ToList();
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var token = await RegisterAsync(email);
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";
        using var otpReq = AuthJson(HttpMethod.Post, "/auth/phone/request-otp", token, new { phone_e164 = phone });
        var otpRes = await _client.SendAsync(otpReq);
        otpRes.EnsureSuccessStatusCode();
        var otp = await otpRes.Content.ReadFromJsonAsync<OtpDto>(JsonOptions);
        using var verifyReq = AuthJson(HttpMethod.Post, "/auth/phone/verify-otp", token, new { code = otp!.DevCode });
        (await _client.SendAsync(verifyReq)).EnsureSuccessStatusCode();
        return token;
    }

    private async Task<string> RegisterAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Wishlist Alerts Parent",
            email,
            password = "Password1!",
            city = "Nairobi",
            accept_terms = true,
            confirm_parent_guardian = true
        }, JsonOptions);
        register.EnsureSuccessStatusCode();
        var auth = await register.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        return auth!.AccessToken;
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
    private sealed record ProfileDto(bool WishlistAlertsEnabled);
    private sealed record NotifDto(string Type, Guid? RelatedEntityId);
    private sealed record NotifPageDto(IReadOnlyList<NotifDto> Items);
}
