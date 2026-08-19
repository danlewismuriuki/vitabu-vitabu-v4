using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Abstractions;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Entities;
using Vitabu.Modules.Listings.Persistence;

namespace Vitabu.Modules.Listings.Services;

public interface IListingsWriteService
{
    Task<ListingDetail> CreateAsync(Guid sellerUserId, CreateListingRequest request, CancellationToken ct = default);
    Task<ListingPage> ListMineAsync(Guid sellerUserId, int page, int pageSize, CancellationToken ct = default);
    Task<ListingDetail> GetMineAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default);
    Task<ListingDetail> UpdateAsync(Guid sellerUserId, Guid listingId, UpdateListingRequest request, CancellationToken ct = default);
    Task<ListingDetail> PauseAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default);
    Task<ListingDetail> ResumeAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default);
    ImageStubResponse CreateImageStub(ImageStubRequest request);
}

public sealed class ListingsWriteService(
    IListingsDbContext listingsDb,
    IIdentityDbContext identityDb,
    ICatalogDbContext catalogDb,
    IWishlistAlertService wishlistAlerts) : IListingsWriteService
{
    public async Task<ListingDetail> CreateAsync(
        Guid sellerUserId,
        CreateListingRequest request,
        CancellationToken ct = default)
    {
        var seller = await RequirePhoneVerifiedSellerAsync(sellerUserId, ct);
        var fields = await ResolveFieldsAsync(request.CbcTitleId, request, ct);
        var school = await ResolveSchoolAsync(request.Intent, request.SchoolId, ct);
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var listing = new Listing
        {
            Id = id,
            SellerUserId = sellerUserId,
            CbcTitleId = fields.CbcTitleId,
            SchoolId = school?.Id,
            Title = fields.Title,
            Grade = fields.Grade,
            Subject = fields.Subject,
            Term = fields.Term,
            City = string.IsNullOrWhiteSpace(request.City) ? seller.City : request.City.Trim(),
            Intent = request.Intent,
            Condition = request.Condition,
            Status = ListingStatus.Active,
            PriceKes = request.Intent == ListingIntent.Sale ? request.PriceKes : null,
            Description = request.Description.Trim(),
            CoverImageUrl = request.CoverImageUrl.Trim(),
            Slug = $"{Slugify(fields.Title)}-{id.ToString("N")[..8]}",
            InterestCount = 0,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        listingsDb.Listings.Add(listing);
        await listingsDb.SaveChangesAsync(ct);

        await wishlistAlerts.NotifySimilarListingCreatedAsync(
            listing.Id,
            listing.SellerUserId,
            listing.Title,
            listing.Grade,
            listing.Subject,
            listing.City,
            ct);

        return ToDetail(listing, seller.DisplayName, listing.City, school);
    }

    public async Task<ListingPage> ListMineAsync(
        Guid sellerUserId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        await RequireAuthenticatedUserAsync(sellerUserId, ct);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = listingsDb.Listings.AsNoTracking()
            .Where(l => l.SellerUserId == sellerUserId);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(l => l.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingCard(
                l.Id,
                l.Title,
                l.Grade,
                l.Subject,
                l.Term,
                l.City,
                l.Intent,
                l.Condition,
                l.Status,
                l.PriceKes,
                l.CoverImageUrl,
                l.InterestCount,
                l.CreatedAtUtc))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new ListingPage(items, page, pageSize, total, totalPages);
    }

    public async Task<ListingDetail> GetMineAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default)
    {
        var seller = await RequireAuthenticatedUserAsync(sellerUserId, ct);
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        return await ToDetailAsync(listing, seller.DisplayName, listing.City, ct);
    }

    public async Task<ListingDetail> UpdateAsync(
        Guid sellerUserId,
        Guid listingId,
        UpdateListingRequest request,
        CancellationToken ct = default)
    {
        var seller = await RequirePhoneVerifiedSellerAsync(sellerUserId, ct);
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        EnsureEditable(listing);

        var fields = await ResolveFieldsAsync(
            request.CbcTitleId,
            new CreateListingRequest(
                request.CbcTitleId,
                request.Title,
                request.Grade,
                request.Subject,
                request.Term,
                request.City,
                request.Intent,
                request.Condition,
                request.PriceKes,
                request.Description,
                request.CoverImageUrl,
                request.SchoolId),
            ct);

        var school = await ResolveSchoolAsync(request.Intent, request.SchoolId, ct);

        listing.CbcTitleId = fields.CbcTitleId;
        listing.SchoolId = school?.Id;
        listing.Title = fields.Title;
        listing.Grade = fields.Grade;
        listing.Subject = fields.Subject;
        listing.Term = fields.Term;
        listing.City = request.City.Trim();
        listing.Intent = request.Intent;
        listing.Condition = request.Condition;
        listing.PriceKes = request.Intent == ListingIntent.Sale ? request.PriceKes : null;
        listing.Description = request.Description.Trim();
        listing.CoverImageUrl = request.CoverImageUrl.Trim();
        listing.UpdatedAtUtc = DateTime.UtcNow;

        await listingsDb.SaveChangesAsync(ct);
        return ToDetail(listing, seller.DisplayName, listing.City, school);
    }

    public async Task<ListingDetail> PauseAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default)
    {
        var seller = await RequirePhoneVerifiedSellerAsync(sellerUserId, ct);
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        if (listing.Status != ListingStatus.Active)
        {
            throw new DomainException("listing_not_pausable", "Only active listings can be paused.");
        }

        listing.Status = ListingStatus.Paused;
        listing.UpdatedAtUtc = DateTime.UtcNow;
        await listingsDb.SaveChangesAsync(ct);
        return await ToDetailAsync(listing, seller.DisplayName, listing.City, ct);
    }

    public async Task<ListingDetail> ResumeAsync(Guid sellerUserId, Guid listingId, CancellationToken ct = default)
    {
        var seller = await RequirePhoneVerifiedSellerAsync(sellerUserId, ct);
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        if (listing.Status != ListingStatus.Paused)
        {
            throw new DomainException("listing_not_resumable", "Only paused listings can be resumed.");
        }

        listing.Status = ListingStatus.Active;
        listing.UpdatedAtUtc = DateTime.UtcNow;
        await listingsDb.SaveChangesAsync(ct);
        return await ToDetailAsync(listing, seller.DisplayName, listing.City, ct);
    }

    public ImageStubResponse CreateImageStub(ImageStubRequest request)
    {
        var raw = string.IsNullOrWhiteSpace(request.Filename)
            ? "Book"
            : Path.GetFileNameWithoutExtension(request.Filename.Trim());
        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = "Book";
        }

        if (raw.Length > 24)
        {
            raw = raw[..24];
        }

        return new ImageStubResponse($"https://placehold.co/600x800/png?text={Uri.EscapeDataString(raw)}");
    }

    private async Task<SellerRow> RequirePhoneVerifiedSellerAsync(Guid userId, CancellationToken ct)
    {
        var seller = await RequireAuthenticatedUserAsync(userId, ct);
        if (seller.PhoneVerifiedAtUtc is null)
        {
            throw new ForbiddenDomainException(
                "phone_not_verified",
                "Verify your phone with SMS OTP before selling.");
        }

        return seller;
    }

    private async Task<SellerRow> RequireAuthenticatedUserAsync(Guid userId, CancellationToken ct)
    {
        var seller = await identityDb.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new SellerRow(u.Id, u.DisplayName, u.City, u.PhoneVerifiedAtUtc))
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedDomainException("unauthorized", "Authentication required.");

        return seller;
    }

    private async Task<ResolvedFields> ResolveFieldsAsync(
        Guid? cbcTitleId,
        CreateListingRequest request,
        CancellationToken ct)
    {
        if (cbcTitleId is null)
        {
            return new ResolvedFields(
                null,
                request.Title.Trim(),
                request.Grade.Trim(),
                request.Subject.Trim(),
                string.IsNullOrWhiteSpace(request.Term) ? null : request.Term.Trim());
        }

        var title = await catalogDb.CbcTitles.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == cbcTitleId, ct)
            ?? throw NotFoundException.For("cbc_title", cbcTitleId);

        return new ResolvedFields(
            title.Id,
            string.IsNullOrWhiteSpace(request.Title) ? title.Title : request.Title.Trim(),
            string.IsNullOrWhiteSpace(request.Grade) ? title.Grade : request.Grade.Trim(),
            string.IsNullOrWhiteSpace(request.Subject) ? title.Subject : request.Subject.Trim(),
            string.IsNullOrWhiteSpace(request.Term) ? title.Term : request.Term.Trim());
    }

    private static void EnsureEditable(Listing listing)
    {
        if (listing.Status is ListingStatus.Sold or ListingStatus.Given or ListingStatus.Exchanged
            or ListingStatus.Donated or ListingStatus.Reserved or ListingStatus.Hidden)
        {
            throw new DomainException(
                "listing_not_editable",
                $"Listings in status '{ToSnake(listing.Status.ToString())}' cannot be edited.");
        }
    }

    private async Task<SchoolSnippet?> ResolveSchoolAsync(
        ListingIntent intent,
        Guid? schoolId,
        CancellationToken ct)
    {
        if (schoolId is null)
        {
            return null;
        }

        if (intent != ListingIntent.DonateSchool)
        {
            throw new ValidationException(
                "school_id is only allowed for donate_school listings.",
                new Dictionary<string, string[]> { ["school_id"] = ["Not allowed for this intent."] });
        }

        var school = await catalogDb.Schools.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schoolId && s.IsVerified, ct)
            ?? throw NotFoundException.For("school", schoolId);

        return new SchoolSnippet(school.Id, school.Name, school.City);
    }

    private async Task<ListingDetail> ToDetailAsync(
        Listing listing,
        string displayName,
        string city,
        CancellationToken ct)
    {
        SchoolSnippet? school = null;
        if (listing.SchoolId is { } schoolId)
        {
            var row = await catalogDb.Schools.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == schoolId, ct);
            if (row is not null)
            {
                school = new SchoolSnippet(row.Id, row.Name, row.City);
            }
        }

        return ToDetail(listing, displayName, city, school);
    }

    private static ListingDetail ToDetail(
        Listing listing,
        string displayName,
        string city,
        SchoolSnippet? school) =>
        new(
            listing.Id,
            listing.Title,
            listing.Grade,
            listing.Subject,
            listing.Term,
            listing.City,
            listing.Intent,
            listing.Condition,
            listing.Status,
            listing.PriceKes,
            listing.CoverImageUrl,
            listing.InterestCount,
            listing.CreatedAtUtc,
            listing.Description,
            listing.Slug,
            new SellerSnippet(displayName, city),
            school);

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private static string ToSnake(string pascal)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c) && i > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    private sealed record SellerRow(Guid Id, string DisplayName, string City, DateTime? PhoneVerifiedAtUtc);
    private sealed record ResolvedFields(Guid? CbcTitleId, string Title, string Grade, string Subject, string? Term);
}
