using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class MessagingApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public MessagingApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Open_thread_send_and_list_inbox()
    {
        var sellerToken = await RegisterAndVerifyAsync($"msg_s_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAndVerifyAsync($"msg_b_{Guid.NewGuid():N}@example.com");

        using var create = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "Messaging Test Book",
            grade = "Grade 5",
            subject = "English",
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 400,
            description = "Book for messaging test.",
            cover_image_url = "https://placehold.co/600x800/png?text=Msg"
        });
        var listingRes = await _client.SendAsync(create);
        listingRes.EnsureSuccessStatusCode();
        var listing = await listingRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        var open = await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{listing!.Id}/threads", buyerToken));
        open.StatusCode.Should().Be(HttpStatusCode.OK);
        var thread = await open.Content.ReadFromJsonAsync<ThreadDto>(JsonOptions);
        thread!.ListingId.Should().Be(listing.Id);

        var send = await _client.SendAsync(AuthJson(HttpMethod.Post, $"/threads/{thread.Id}/messages", buyerToken, new
        {
            body = "Hi, is this still available?"
        }));
        send.EnsureSuccessStatusCode();

        var sellerInbox = await _client.SendAsync(Auth(HttpMethod.Get, "/me/threads", sellerToken));
        sellerInbox.EnsureSuccessStatusCode();
        var page = await sellerInbox.Content.ReadFromJsonAsync<ThreadPageDto>(JsonOptions);
        page!.Items.Should().Contain(t => t.Id == thread.Id);

        var detail = await _client.SendAsync(Auth(HttpMethod.Get, $"/threads/{thread.Id}", sellerToken));
        detail.EnsureSuccessStatusCode();
        var full = await detail.Content.ReadFromJsonAsync<ThreadDetailDto>(JsonOptions);
        full!.Messages.Should().ContainSingle(m => m.Body.Contains("available"));

        // Own listing blocked
        var own = await _client.SendAsync(Auth(HttpMethod.Post, $"/listings/{listing.Id}/threads", sellerToken));
        own.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Msg Parent",
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
    private sealed record ThreadDto(Guid Id, Guid ListingId);
    private sealed record ThreadPageDto(IReadOnlyList<ThreadDto> Items);
    private sealed record ThreadDetailDto(IReadOnlyList<MessageDto> Messages);
    private sealed record MessageDto(string Body);
}
