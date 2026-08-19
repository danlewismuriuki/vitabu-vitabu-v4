using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class ListingsWriteApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public ListingsWriteApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_requires_phone_verification()
    {
        var email = $"seller_{Guid.NewGuid():N}@example.com";
        var token = await RegisterAsync(email);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/listings")
        {
            Content = JsonContent.Create(ValidSaleBody(), options: JsonOptions)
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await res.Content.ReadFromJsonAsync<ProblemDto>(JsonOptions);
        problem!.Error.Should().Be("phone_not_verified");
    }

    [Fact]
    public async Task Verified_user_publishes_and_listing_appears_in_browse()
    {
        var email = $"verified_{Guid.NewGuid():N}@example.com";
        var phone = $"+2547{Random.Shared.Next(10000000, 99999999)}";
        var token = await RegisterAndVerifyPhoneAsync(email, phone);

        var titles = await _client.GetFromJsonAsync<TitlePageDto>("/catalog/titles?page_size=1", JsonOptions);
        titles!.Items.Should().NotBeEmpty();
        var cbc = titles.Items[0];

        using var createReq = new HttpRequestMessage(HttpMethod.Post, "/listings")
        {
            Content = JsonContent.Create(new
            {
                cbc_title_id = cbc.Id,
                title = cbc.Title,
                grade = cbc.Grade,
                subject = cbc.Subject,
                term = cbc.Term,
                city = "Nairobi",
                intent = "sale",
                condition = "good",
                price_kes = 450,
                description = "Gently used CBC book from our Grade shelf.",
                cover_image_url = "https://placehold.co/600x800/png?text=TestBook"
            }, options: JsonOptions)
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.SendAsync(createReq);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var detail = await create.Content.ReadFromJsonAsync<DetailDto>(JsonOptions);
        detail!.Status.Should().Be("active");
        detail.Title.Should().Be(cbc.Title);

        var publicGet = await _client.GetAsync($"/listings/{detail.Id}");
        publicGet.StatusCode.Should().Be(HttpStatusCode.OK);

        using var mineReq = new HttpRequestMessage(HttpMethod.Get, "/me/listings");
        mineReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var mine = await _client.SendAsync(mineReq);
        mine.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await mine.Content.ReadFromJsonAsync<PageDto>(JsonOptions);
        page!.Items.Should().Contain(i => i.Id == detail.Id);

        using var pauseReq = new HttpRequestMessage(HttpMethod.Post, $"/listings/{detail.Id}/pause");
        pauseReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var pause = await _client.SendAsync(pauseReq);
        pause.StatusCode.Should().Be(HttpStatusCode.OK);

        var hidden = await _client.GetAsync($"/listings/{detail.Id}");
        hidden.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var resumeReq = new HttpRequestMessage(HttpMethod.Post, $"/listings/{detail.Id}/resume");
        resumeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resume = await _client.SendAsync(resumeReq);
        resume.StatusCode.Should().Be(HttpStatusCode.OK);

        var visible = await _client.GetAsync($"/listings/{detail.Id}");
        visible.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Catalog_titles_search_returns_seeded_rows()
    {
        var res = await _client.GetAsync("/catalog/titles?q=Math&page_size=10");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await res.Content.ReadFromJsonAsync<TitlePageDto>(JsonOptions);
        page!.Items.Should().NotBeEmpty();
    }

    private async Task<string> RegisterAsync(string email)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new
        {
            display_name = "Seller Parent",
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

    private async Task<string> RegisterAndVerifyPhoneAsync(string email, string phone)
    {
        var token = await RegisterAsync(email);

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

    private static object ValidSaleBody() => new
    {
        title = "Test Book",
        grade = "Grade 4",
        subject = "Mathematics",
        city = "Nairobi",
        intent = "sale",
        condition = "good",
        price_kes = 300,
        description = "Test description for listing create.",
        cover_image_url = "https://placehold.co/600x800/png?text=Book"
    };

    private sealed record AuthDto(string AccessToken);
    private sealed record OtpDto(string? DevCode);
    private sealed record ProblemDto(string Error);
    private sealed record DetailDto(Guid Id, string Title, string Status);
    private sealed record PageDto(List<CardDto> Items);
    private sealed record CardDto(Guid Id);
    private sealed record TitlePageDto(List<TitleDto> Items);
    private sealed record TitleDto(Guid Id, string Title, string Grade, string Subject, string Term);
}
