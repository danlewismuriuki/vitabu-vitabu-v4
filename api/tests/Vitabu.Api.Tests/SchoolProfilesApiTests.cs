using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class SchoolProfilesApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public SchoolProfilesApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_schools_admin_create_and_donate_with_school_id()
    {
        var list = await _client.GetFromJsonAsync<SchoolPageDto>("/schools?page_size=50", JsonOptions);
        list.Should().NotBeNull();
        list!.Items.Should().NotBeEmpty();
        var seedSchool = list.Items.First();

        var adminLogin = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "admin@vitabu.local",
            password = "AdminPassword1!"
        }, JsonOptions);
        adminLogin.EnsureSuccessStatusCode();
        var adminAuth = await adminLogin.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);

        using var createSchool = AuthJson(HttpMethod.Post, "/admin/schools", adminAuth!.AccessToken, new
        {
            name = $"Test Academy {Guid.NewGuid():N}",
            city = "Nakuru",
            contact_name = "Head teacher"
        });
        var schoolRes = await _client.SendAsync(createSchool);
        schoolRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await schoolRes.Content.ReadFromJsonAsync<SchoolDetailDto>(JsonOptions);
        created!.IsVerified.Should().BeTrue();
        created.City.Should().Be("Nakuru");

        var after = await _client.GetFromJsonAsync<SchoolPageDto>("/schools?page_size=50", JsonOptions);
        after!.Items.Should().Contain(s => s.Id == created.Id);

        var sellerToken = await RegisterAndVerifyAsync($"s9_seller_{Guid.NewGuid():N}@example.com");
        using var createListing = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "S9 Donate Targeted",
            grade = "Grade 4",
            subject = "Mathematics",
            city = "Nairobi",
            intent = "donate_school",
            condition = "good",
            school_id = seedSchool.Id,
            description = "Targeted school donate listing.",
            cover_image_url = "https://placehold.co/600x800/png?text=S9"
        });
        var listingRes = await _client.SendAsync(createListing);
        listingRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var listing = await listingRes.Content.ReadFromJsonAsync<ListingDetailDto>(JsonOptions);
        listing!.School.Should().NotBeNull();
        listing.School!.Id.Should().Be(seedSchool.Id);
        listing.School.Name.Should().Be(seedSchool.Name);

        var filtered = await _client.GetFromJsonAsync<ListingPageDto>(
            $"/listings?intent=donate_school&school_id={seedSchool.Id}&page_size=50",
            JsonOptions);
        filtered!.Items.Should().Contain(i => i.Id == listing.Id && i.SchoolId == seedSchool.Id);

        using var saleWithSchool = AuthJson(HttpMethod.Post, "/listings", sellerToken, new
        {
            title = "S9 Sale Bad School",
            grade = "Grade 4",
            subject = "Mathematics",
            city = "Nairobi",
            intent = "sale",
            condition = "good",
            price_kes = 200,
            school_id = seedSchool.Id,
            description = "Should reject school_id.",
            cover_image_url = "https://placehold.co/600x800/png?text=Bad"
        });
        var bad = await _client.SendAsync(saleWithSchool);
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> RegisterAndVerifyAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "S9 Parent",
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

    private sealed record AuthDto(string AccessToken, UserDto? User = null);
    private sealed record UserDto(bool IsStaff);
    private sealed record OtpDto(string? DevCode);
    private sealed record SchoolCardDto(Guid Id, string Name, string City);
    private sealed record SchoolDetailDto(Guid Id, string Name, string City, bool IsVerified);
    private sealed record SchoolPageDto(IReadOnlyList<SchoolCardDto> Items);
    private sealed record SchoolSnippetDto(Guid Id, string Name, string City);
    private sealed record ListingDetailDto(Guid Id, SchoolSnippetDto? School);
    private sealed record ListingCardDto(Guid Id, Guid? SchoolId);
    private sealed record ListingPageDto(IReadOnlyList<ListingCardDto> Items);
}
