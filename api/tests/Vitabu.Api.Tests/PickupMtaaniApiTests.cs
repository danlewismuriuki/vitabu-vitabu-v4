using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class PickupMtaaniApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public PickupMtaaniApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Dev_stub_agents_and_mtaani_interest()
    {
        var agents = await _client.GetFromJsonAsync<List<AgentDto>>("/mtaani/agents?search=Nairobi", JsonOptions);
        agents.Should().NotBeNull();
        agents!.Should().NotBeEmpty();
        var agent = agents.First();

        var locations = await _client.GetFromJsonAsync<List<LocationDto>>("/mtaani/locations", JsonOptions);
        locations!.Should().NotBeEmpty();

        var sellerToken = await RegisterAndVerifyAsync($"s10_s_{Guid.NewGuid():N}@example.com");
        var buyerToken = await RegisterAndVerifyAsync($"s10_b_{Guid.NewGuid():N}@example.com");

        using var createListing = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "S10 Mtaani Book",
            grade = "Grade 5",
            subject = "English",
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 300,
            description = "For Mtaani arrange test.",
            cover_image_url = "https://placehold.co/600x800/png?text=S10"
        });
        var listingRes = await _client.SendAsync(createListing);
        listingRes.EnsureSuccessStatusCode();
        var listing = await listingRes.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        using var mtaaniInterest = AuthJson(HttpMethod.Post, $"/listings/{listing!.Id}/interests", buyerToken, new
        {
            handoff_mode = "pickup_mtaani",
            city = "Nairobi",
            mtaani_agent_id = agent.Id,
            message = "Can drop Thursday"
        });
        var interestRes = await _client.SendAsync(mtaaniInterest);
        interestRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var interest = await interestRes.Content.ReadFromJsonAsync<InterestDto>(JsonOptions);
        interest!.HandoffMode.Should().Be("pickup_mtaani");
        interest.MtaaniAgent.Should().NotBeNull();
        interest.MtaaniAgent!.Id.Should().Be(agent.Id);
        interest.MtaaniAgent.BusinessName.Should().Be(agent.BusinessName);

        var buyer2 = await RegisterAndVerifyAsync($"s10_b2_{Guid.NewGuid():N}@example.com");
        using var missingAgent = AuthJson(HttpMethod.Post, $"/listings/{listing.Id}/interests", buyer2, new
        {
            handoff_mode = "pickup_mtaani",
            city = "Nairobi"
        });
        (await _client.SendAsync(missingAgent)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var meetupWithAgent = AuthJson(HttpMethod.Post, $"/listings/{listing.Id}/interests", buyer2, new
        {
            handoff_mode = "meetup",
            city = "Nairobi",
            mtaani_agent_id = agent.Id
        });
        (await _client.SendAsync(meetupWithAgent)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "S10 Parent",
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
    private sealed record AgentDto(int Id, string BusinessName);
    private sealed record LocationDto(int Id, string Name);
    private sealed record AgentSnippetDto(int Id, string BusinessName);
    private sealed record InterestDto(string HandoffMode, AgentSnippetDto? MtaaniAgent);
}
