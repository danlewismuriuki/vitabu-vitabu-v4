using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class WishlistApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public WishlistApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Add_list_status_and_remove_wishlist()
    {
        var sellerToken = await RegisterAndVerifyAsync($"wl_seller_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAsync($"wl_buyer_{Guid.NewGuid():N}@example.com");

        using var createListing = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "Wishlist Test Book",
            grade = "Grade 4",
            subject = "Mathematics",
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 300,
            description = "Book for wishlist test.",
            cover_image_url = "https://placehold.co/600x800/png?text=WL"
        });
        var listingRes = await _client.SendAsync(createListing);
        listingRes.EnsureSuccessStatusCode();
        var listing = await listingRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        (await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{listing!.Id}/wishlist", buyerToken)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Idempotent re-add
        (await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{listing.Id}/wishlist", buyerToken)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var statusRes = await _client.SendAsync(Auth(HttpMethod.Get, $"/listings/{listing.Id}/wishlist", buyerToken));
        statusRes.EnsureSuccessStatusCode();
        var status = await statusRes.Content.ReadFromJsonAsync<StatusDto>(JsonOptions);
        status!.OnWishlist.Should().BeTrue();

        var listRes = await _client.SendAsync(Auth(HttpMethod.Get, "/me/wishlist", buyerToken));
        listRes.EnsureSuccessStatusCode();
        var page = await listRes.Content.ReadFromJsonAsync<WishlistPageDto>(JsonOptions);
        page!.Items.Should().ContainSingle(i => i.ListingId == listing.Id);

        // Own listing blocked
        var own = await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{listing.Id}/wishlist", sellerToken));
        own.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await _client.SendAsync(Auth(HttpMethod.Delete, $"/listings/{listing.Id}/wishlist", buyerToken)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await _client.SendAsync(Auth(HttpMethod.Get, $"/listings/{listing.Id}/wishlist", buyerToken));
        after.EnsureSuccessStatusCode();
        (await after.Content.ReadFromJsonAsync<StatusDto>(JsonOptions))!.OnWishlist.Should().BeFalse();
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
            display_name = "Wishlist Parent",
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
    private sealed record StatusDto(bool OnWishlist);
    private sealed record WishlistPageDto(IReadOnlyList<ItemDto> Items);
    private sealed record ItemDto(Guid ListingId);
}
