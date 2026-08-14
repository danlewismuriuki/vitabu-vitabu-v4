using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Vitabu.Api.Tests;

[Collection("Api")]
public class ListingsApiTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public ListingsApiTests(VitabuWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Listings_return_seeded_active_cards()
    {
        var res = await _client.GetAsync("/listings?page_size=10");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await res.Content.ReadFromJsonAsync<PageDto>(JsonOptions);
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(i => i.Status == "active");
    }

    [Fact]
    public async Task Listings_filter_by_intent()
    {
        var res = await _client.GetAsync("/listings?intent=free&page_size=50");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await res.Content.ReadFromJsonAsync<PageDto>(JsonOptions);
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(i => i.Intent == "free");
    }

    [Fact]
    public async Task Get_listing_by_id_and_404()
    {
        var list = await _client.GetFromJsonAsync<PageDto>("/listings?page_size=1", JsonOptions);
        var id = list!.Items[0].Id;

        var detail = await _client.GetAsync($"/listings/{id}");
        detail.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await detail.Content.ReadFromJsonAsync<DetailDto>(JsonOptions);
        body!.Seller.DisplayName.Should().NotBeNullOrWhiteSpace();
        body.Description.Should().NotBeNullOrWhiteSpace();

        var missing = await _client.GetAsync($"/listings/{Guid.NewGuid()}");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Catalog_facets_available()
    {
        var res = await _client.GetAsync("/catalog/facets");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var facets = await res.Content.ReadFromJsonAsync<FacetsDto>(JsonOptions);
        facets!.Grades.Should().NotBeEmpty();
        facets.Intents.Should().Contain("sale");
    }

    private sealed record PageDto(List<CardDto> Items, int TotalItems);
    private sealed record CardDto(Guid Id, string Intent, string Status);
    private sealed record DetailDto(string Description, SellerDto Seller);
    private sealed record SellerDto(string DisplayName);
    private sealed record FacetsDto(List<string> Grades, List<string> Intents);
}
