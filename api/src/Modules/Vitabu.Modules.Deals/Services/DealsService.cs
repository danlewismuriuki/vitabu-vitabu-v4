using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Abstractions;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Deals.Contracts;
using Vitabu.Modules.Deals.Domain;
using Vitabu.Modules.Deals.Entities;
using Vitabu.Modules.Deals.Persistence;
using Vitabu.Modules.Deals.PickupMtaani;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Services;

namespace Vitabu.Modules.Deals.Services;

public interface IDealsService
{
    Task<InterestDetail> CreateAsync(Guid buyerUserId, Guid listingId, CreateInterestRequest request, CancellationToken ct = default);
    Task<InterestPage> ListMineAsBuyerAsync(Guid buyerUserId, int page, int pageSize, CancellationToken ct = default);
    Task<InterestPage> ListForListingAsSellerAsync(Guid sellerUserId, Guid listingId, int page, int pageSize, CancellationToken ct = default);
    Task<InterestDetail> GetAsync(Guid userId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> AcceptAsync(Guid sellerUserId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> DeclineAsync(Guid sellerUserId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> CancelAsync(Guid userId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> ReleaseAsync(Guid userId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> CompleteAsync(Guid userId, Guid interestId, CancellationToken ct = default);
    Task<InterestDetail> DisputeAsync(Guid userId, Guid interestId, DisputeInterestRequest request, CancellationToken ct = default);
    Task RateAsync(Guid userId, Guid interestId, RateInterestRequest request, CancellationToken ct = default);
    Task ReportListingAsync(Guid userId, Guid listingId, ReportListingRequest request, CancellationToken ct = default);
    Task ExpireIfNeededAsync(Guid interestId, CancellationToken ct = default);
}

public sealed class DealsService(
    IDealsDbContext dealsDb,
    IListingsDbContext listingsDb,
    IIdentityDbContext identityDb,
    INotificationService notifications,
    IPickupMtaaniClient mtaani,
    IWishlistAlertService wishlistAlerts) : IDealsService
{
    private static readonly TimeSpan ReserveWindow = TimeSpan.FromHours(72);

    public async Task<InterestDetail> CreateAsync(
        Guid buyerUserId,
        Guid listingId,
        CreateInterestRequest request,
        CancellationToken ct = default)
    {
        var buyer = await RequirePhoneVerifiedAsync(buyerUserId, ct);
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        if (listing.Status != ListingStatus.Active)
        {
            throw new ConflictException("listing_not_available", "This listing is not open for new interest.");
        }

        if (listing.SellerUserId == buyerUserId)
        {
            throw new DomainException("cannot_interest_own_listing", "You cannot arrange your own listing.");
        }

        var existing = await dealsDb.DealInterests.AsNoTracking()
            .AnyAsync(i =>
                i.ListingId == listingId &&
                i.BuyerUserId == buyerUserId &&
                (i.Status == InterestStatus.Pending ||
                 i.Status == InterestStatus.Waitlisted ||
                 i.Status == InterestStatus.Accepted),
                ct);
        if (existing)
        {
            throw new ConflictException("interest_already_exists", "You already have an open request on this listing.");
        }

        var agent = await ResolveMtaaniAgentAsync(request.HandoffMode, request.MtaaniAgentId, ct);

        var now = DateTime.UtcNow;
        var interest = new DealInterest
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            BuyerUserId = buyerUserId,
            SellerUserId = listing.SellerUserId,
            Status = InterestStatus.Pending,
            HandoffMode = request.HandoffMode,
            City = request.City.Trim(),
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            MtaaniAgentId = agent?.Id,
            MtaaniAgentName = agent?.BusinessName,
            MtaaniLocationId = agent?.LocationId,
            MtaaniLocationName = agent?.LocationName,
            MtaaniEstimatedFeeKes = agent is null ? null : 0,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dealsDb.DealInterests.Add(interest);
        listing.InterestCount = await CountOpenInterestsAsync(listing.Id, ct) + 1;
        listing.UpdatedAtUtc = now;
        await dealsDb.SaveChangesAsync(ct);
        await listingsDb.SaveChangesAsync(ct);

        await notifications.NotifyAsync(
            listing.SellerUserId,
            "interest_created",
            "New interest on your book",
            $"{buyer.DisplayName} is interested in “{listing.Title}”.",
            interest.Id,
            "New interest on Vitabu Vitabu",
            $"{buyer.DisplayName} requested to arrange “{listing.Title}”. Open My listings to Accept or Decline.",
            ct);

        return await GetAsync(buyerUserId, interest.Id, ct);
    }

    public async Task<InterestPage> ListMineAsBuyerAsync(
        Guid buyerUserId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        await RequireUserAsync(buyerUserId, ct);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = from i in dealsDb.DealInterests.AsNoTracking()
                join l in listingsDb.Listings.AsNoTracking() on i.ListingId equals l.Id
                where i.BuyerUserId == buyerUserId
                select new { i, l };

        var total = await q.CountAsync(ct);
        var rows = await q
            .OrderByDescending(x => x.i.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var buyerIds = rows.Select(r => r.i.BuyerUserId).Distinct().ToList();
        var names = await identityDb.Users.AsNoTracking()
            .Where(u => buyerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var items = rows.Select(r => new InterestCard(
            r.i.Id,
            r.i.ListingId,
            r.l.Title,
            r.i.Status,
            r.i.HandoffMode,
            r.i.City,
            names.GetValueOrDefault(r.i.BuyerUserId, "Parent"),
            r.i.CreatedAtUtc,
            r.i.ReservedUntilUtc,
            r.i.MtaaniAgentName)).ToList();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new InterestPage(items, page, pageSize, total, totalPages);
    }

    public async Task<InterestPage> ListForListingAsSellerAsync(
        Guid sellerUserId,
        Guid listingId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        await RequireUserAsync(sellerUserId, ct);
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = dealsDb.DealInterests.AsNoTracking()
            .Where(i => i.ListingId == listingId && i.SellerUserId == sellerUserId);

        var total = await q.CountAsync(ct);
        var interests = await q
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var buyerIds = interests.Select(i => i.BuyerUserId).Distinct().ToList();
        var names = await identityDb.Users.AsNoTracking()
            .Where(u => buyerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var items = interests.Select(i => new InterestCard(
            i.Id,
            i.ListingId,
            listing.Title,
            i.Status,
            i.HandoffMode,
            i.City,
            names.GetValueOrDefault(i.BuyerUserId, "Parent"),
            i.CreatedAtUtc,
            i.ReservedUntilUtc,
            i.MtaaniAgentName)).ToList();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new InterestPage(items, page, pageSize, total, totalPages);
    }

    public async Task<InterestDetail> GetAsync(Guid userId, Guid interestId, CancellationToken ct = default)
    {
        await ExpireIfNeededAsync(interestId, ct);

        var interest = await dealsDb.DealInterests.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId && interest.SellerUserId != userId)
        {
            throw NotFoundException.For("interest", interestId);
        }

        return await ToDetailAsync(interest, userId, ct);
    }

    public async Task<InterestDetail> AcceptAsync(Guid sellerUserId, Guid interestId, CancellationToken ct = default)
    {
        await RequirePhoneVerifiedAsync(sellerUserId, ct);
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId && i.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.Status != InterestStatus.Pending && interest.Status != InterestStatus.Waitlisted)
        {
            throw new DomainException("interest_not_acceptable", "Only pending or waitlisted requests can be accepted.");
        }

        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == interest.ListingId, ct)
            ?? throw NotFoundException.For("listing", interest.ListingId);

        if (listing.Status != ListingStatus.Active)
        {
            throw new ConflictException("listing_not_available", "Listing is not available to reserve.");
        }

        var now = DateTime.UtcNow;
        interest.Status = InterestStatus.Accepted;
        interest.AcceptedAtUtc = now;
        interest.ReservedUntilUtc = now.Add(ReserveWindow);
        interest.UpdatedAtUtc = now;

        var others = await dealsDb.DealInterests
            .Where(i =>
                i.ListingId == interest.ListingId &&
                i.Id != interest.Id &&
                i.Status == InterestStatus.Pending)
            .ToListAsync(ct);

        foreach (var other in others)
        {
            other.Status = InterestStatus.Waitlisted;
            other.UpdatedAtUtc = now;
        }

        listing.Status = ListingStatus.Reserved;
        listing.InterestCount = others.Count;
        listing.UpdatedAtUtc = now;

        await dealsDb.SaveChangesAsync(ct);
        await listingsDb.SaveChangesAsync(ct);

        await wishlistAlerts.NotifyWishlistedListingUnavailableAsync(
            listing.Id,
            listing.Title,
            "reserved with another parent",
            ct);

        await notifications.NotifyAsync(
            interest.BuyerUserId,
            "interest_accepted",
            "Seller accepted your request",
            $"Your arrange request for “{listing.Title}” was accepted. Phones are unlocked.",
            interest.Id,
            "Deal accepted on Vitabu Vitabu",
            $"The seller accepted your request for “{listing.Title}”. Open the deal to see contact details.",
            ct);

        foreach (var other in others)
        {
            await notifications.NotifyAsync(
                other.BuyerUserId,
                "interest_waitlisted",
                "Seller is dealing with someone else",
                $"“{listing.Title}” is reserved with another parent. We’ll notify you if it opens again.",
                other.Id,
                "Update on Vitabu Vitabu",
                $"The seller is dealing with someone else for “{listing.Title}”. You’re waitlisted.",
                ct);
        }

        return await GetAsync(sellerUserId, interest.Id, ct);
    }

    public async Task<InterestDetail> DeclineAsync(Guid sellerUserId, Guid interestId, CancellationToken ct = default)
    {
        await RequirePhoneVerifiedAsync(sellerUserId, ct);
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId && i.SellerUserId == sellerUserId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.Status is not (InterestStatus.Pending or InterestStatus.Waitlisted))
        {
            throw new DomainException("interest_not_declinable", "Only pending or waitlisted requests can be declined.");
        }

        interest.Status = InterestStatus.Declined;
        interest.UpdatedAtUtc = DateTime.UtcNow;
        await RefreshListingInterestCountAsync(interest.ListingId, ct);
        await dealsDb.SaveChangesAsync(ct);

        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstAsync(l => l.Id == interest.ListingId, ct);

        await notifications.NotifyAsync(
            interest.BuyerUserId,
            "interest_declined",
            "Request declined",
            $"Your request for “{listing.Title}” was declined.",
            interest.Id,
            "Request declined on Vitabu Vitabu",
            $"Your arrange request for “{listing.Title}” was declined.",
            ct);

        return await GetAsync(sellerUserId, interest.Id, ct);
    }

    public async Task<InterestDetail> CancelAsync(Guid userId, Guid interestId, CancellationToken ct = default)
    {
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId)
        {
            throw new ForbiddenDomainException("forbidden", "Only the buyer can cancel a pending request.");
        }

        if (interest.Status is not (InterestStatus.Pending or InterestStatus.Waitlisted))
        {
            throw new DomainException("interest_not_cancellable", "Only pending or waitlisted requests can be cancelled.");
        }

        interest.Status = InterestStatus.Cancelled;
        interest.UpdatedAtUtc = DateTime.UtcNow;
        await RefreshListingInterestCountAsync(interest.ListingId, ct);
        await dealsDb.SaveChangesAsync(ct);

        return await GetAsync(userId, interest.Id, ct);
    }

    public async Task<InterestDetail> ReleaseAsync(Guid userId, Guid interestId, CancellationToken ct = default)
    {
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId && interest.SellerUserId != userId)
        {
            throw NotFoundException.For("interest", interestId);
        }

        if (interest.Status != InterestStatus.Accepted)
        {
            throw new DomainException("interest_not_releasable", "Only an accepted deal can be released.");
        }

        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == interest.ListingId, ct)
            ?? throw NotFoundException.For("listing", interest.ListingId);

        var now = DateTime.UtcNow;
        interest.Status = InterestStatus.Cancelled;
        interest.UpdatedAtUtc = now;
        interest.ReservedUntilUtc = null;

        listing.Status = ListingStatus.Active;
        listing.UpdatedAtUtc = now;

        var waitlisted = await dealsDb.DealInterests
            .Where(i => i.ListingId == listing.Id && i.Status == InterestStatus.Waitlisted)
            .ToListAsync(ct);

        foreach (var w in waitlisted)
        {
            w.Status = InterestStatus.Pending;
            w.UpdatedAtUtc = now;
        }

        listing.InterestCount = waitlisted.Count;
        await dealsDb.SaveChangesAsync(ct);
        await listingsDb.SaveChangesAsync(ct);

        var otherParty = userId == interest.BuyerUserId ? interest.SellerUserId : interest.BuyerUserId;
        await notifications.NotifyAsync(
            otherParty,
            "deal_released",
            "Deal released",
            $"The reserve on “{listing.Title}” was released. The book is Active again.",
            interest.Id,
            "Deal released on Vitabu Vitabu",
            $"“{listing.Title}” is available again on Vitabu Vitabu.",
            ct);

        foreach (var w in waitlisted)
        {
            await notifications.NotifyAsync(
                w.BuyerUserId,
                "listing_available_again",
                "Book available again",
                $"“{listing.Title}” is Active again — you can still arrange.",
                w.Id,
                "Book available again",
                $"“{listing.Title}” opened again. Your interest is pending.",
                ct);
        }

        return await GetAsync(userId, interest.Id, ct);
    }

    public async Task<InterestDetail> CompleteAsync(Guid userId, Guid interestId, CancellationToken ct = default)
    {
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId && interest.SellerUserId != userId)
        {
            throw NotFoundException.For("interest", interestId);
        }

        if (interest.Status is not (InterestStatus.Accepted or InterestStatus.Disputed))
        {
            throw new DomainException("interest_not_completable", "Only an accepted (or disputed) deal can be confirmed complete.");
        }

        var now = DateTime.UtcNow;
        if (userId == interest.BuyerUserId)
        {
            if (interest.BuyerCompletedAtUtc is not null)
            {
                throw new DomainException("already_confirmed", "You already confirmed completion.");
            }

            interest.BuyerCompletedAtUtc = now;
        }
        else
        {
            if (interest.SellerCompletedAtUtc is not null)
            {
                throw new DomainException("already_confirmed", "You already confirmed completion.");
            }

            interest.SellerCompletedAtUtc = now;
        }

        interest.UpdatedAtUtc = now;

        var otherParty = userId == interest.BuyerUserId ? interest.SellerUserId : interest.BuyerUserId;
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == interest.ListingId, ct)
            ?? throw NotFoundException.For("listing", interest.ListingId);

        if (interest.BuyerCompletedAtUtc is not null && interest.SellerCompletedAtUtc is not null)
        {
            interest.Status = InterestStatus.Completed;
            listing.Status = listing.Intent switch
            {
                ListingIntent.Sale => ListingStatus.Sold,
                ListingIntent.Free => ListingStatus.Given,
                ListingIntent.Exchange => ListingStatus.Exchanged,
                ListingIntent.DonateSchool => ListingStatus.Donated,
                _ => ListingStatus.Sold
            };
            listing.UpdatedAtUtc = now;

            var leftover = await dealsDb.DealInterests
                .Where(i =>
                    i.ListingId == listing.Id &&
                    i.Id != interest.Id &&
                    (i.Status == InterestStatus.Pending || i.Status == InterestStatus.Waitlisted))
                .ToListAsync(ct);
            foreach (var left in leftover)
            {
                left.Status = InterestStatus.Cancelled;
                left.UpdatedAtUtc = now;
            }

            listing.InterestCount = 0;
            await dealsDb.SaveChangesAsync(ct);
            await listingsDb.SaveChangesAsync(ct);

            await notifications.NotifyAsync(
                otherParty,
                "deal_completed",
                "Deal completed",
                $"Both of you confirmed “{listing.Title}”. You can rate each other now.",
                interest.Id,
                "Deal completed on Vitabu Vitabu",
                $"“{listing.Title}” is complete. Asante — leave a rating if you can.",
                ct);
        }
        else
        {
            await dealsDb.SaveChangesAsync(ct);
            await notifications.NotifyAsync(
                otherParty,
                "deal_confirm_needed",
                "Please confirm the handoff",
                $"The other parent confirmed “{listing.Title}”. Confirm when you’ve finished the handoff.",
                interest.Id,
                "Confirm handoff on Vitabu Vitabu",
                $"Please confirm completion for “{listing.Title}”.",
                ct);
        }

        return await GetAsync(userId, interest.Id, ct);
    }

    public async Task<InterestDetail> DisputeAsync(
        Guid userId,
        Guid interestId,
        DisputeInterestRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("One or more validation errors occurred.",
                new Dictionary<string, string[]> { ["reason"] = ["Reason is required."] });
        }

        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId && interest.SellerUserId != userId)
        {
            throw NotFoundException.For("interest", interestId);
        }

        if (interest.Status is not (InterestStatus.Accepted or InterestStatus.Disputed))
        {
            throw new DomainException("interest_not_disputable", "Only accepted deals can be disputed.");
        }

        var now = DateTime.UtcNow;
        interest.Status = InterestStatus.Disputed;
        interest.DisputeReason = request.Reason.Trim();
        interest.DisputedAtUtc = now;
        interest.UpdatedAtUtc = now;
        await dealsDb.SaveChangesAsync(ct);

        var otherParty = userId == interest.BuyerUserId ? interest.SellerUserId : interest.BuyerUserId;
        await notifications.NotifyAsync(
            otherParty,
            "deal_disputed",
            "Deal disputed",
            $"A dispute was opened: {interest.DisputeReason}",
            interest.Id,
            "Deal disputed on Vitabu Vitabu",
            $"A dispute was opened on your deal: {interest.DisputeReason}",
            ct);

        return await GetAsync(userId, interest.Id, ct);
    }

    public async Task RateAsync(
        Guid userId,
        Guid interestId,
        RateInterestRequest request,
        CancellationToken ct = default)
    {
        if (request.Stars is < 1 or > 5)
        {
            throw new ValidationException("One or more validation errors occurred.",
                new Dictionary<string, string[]> { ["stars"] = ["Stars must be between 1 and 5."] });
        }

        var interest = await dealsDb.DealInterests.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interestId, ct)
            ?? throw NotFoundException.For("interest", interestId);

        if (interest.BuyerUserId != userId && interest.SellerUserId != userId)
        {
            throw NotFoundException.For("interest", interestId);
        }

        if (interest.Status != InterestStatus.Completed)
        {
            throw new DomainException("interest_not_rateable", "You can only rate after both parties complete the deal.");
        }

        var toUserId = userId == interest.BuyerUserId ? interest.SellerUserId : interest.BuyerUserId;
        var exists = await dealsDb.DealRatings.AnyAsync(
            r => r.InterestId == interestId && r.FromUserId == userId, ct);
        if (exists)
        {
            throw new ConflictException("already_rated", "You already rated this deal.");
        }

        dealsDb.DealRatings.Add(new DealRating
        {
            Id = Guid.NewGuid(),
            InterestId = interestId,
            FromUserId = userId,
            ToUserId = toUserId,
            Stars = request.Stars,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        });
        await dealsDb.SaveChangesAsync(ct);

        await notifications.NotifyAsync(
            toUserId,
            "rating_received",
            "You received a rating",
            $"Someone rated you {request.Stars}/5 after a Vitabu handoff.",
            interestId,
            "New rating on Vitabu Vitabu",
            $"You received a {request.Stars}/5 rating.",
            ct);
    }

    public async Task ReportListingAsync(
        Guid userId,
        Guid listingId,
        ReportListingRequest request,
        CancellationToken ct = default)
    {
        await RequireUserAsync(userId, ct);
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("One or more validation errors occurred.",
                new Dictionary<string, string[]> { ["reason"] = ["Reason is required."] });
        }

        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        dealsDb.ListingReports.Add(new ListingReport
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            ReporterUserId = userId,
            Reason = request.Reason.Trim(),
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = "open",
            CreatedAtUtc = DateTime.UtcNow
        });
        await dealsDb.SaveChangesAsync(ct);
    }

    public async Task ExpireIfNeededAsync(Guid interestId, CancellationToken ct = default)
    {
        var interest = await dealsDb.DealInterests
            .FirstOrDefaultAsync(i => i.Id == interestId, ct);
        if (interest is null || interest.Status != InterestStatus.Accepted)
        {
            return;
        }

        if (interest.ReservedUntilUtc is null || interest.ReservedUntilUtc > DateTime.UtcNow)
        {
            return;
        }

        await ReleaseAsync(interest.SellerUserId, interest.Id, ct);
    }

    private async Task<int> CountOpenInterestsAsync(Guid listingId, CancellationToken ct) =>
        await dealsDb.DealInterests.CountAsync(
            i => i.ListingId == listingId && i.Status == InterestStatus.Pending,
            ct);

    private async Task RefreshListingInterestCountAsync(Guid listingId, CancellationToken ct)
    {
        var listing = await listingsDb.Listings.FirstOrDefaultAsync(l => l.Id == listingId, ct);
        if (listing is null)
        {
            return;
        }

        listing.InterestCount = await CountOpenInterestsAsync(listingId, ct);
        listing.UpdatedAtUtc = DateTime.UtcNow;
        await listingsDb.SaveChangesAsync(ct);
    }

    private async Task<InterestDetail> ToDetailAsync(DealInterest interest, Guid viewerId, CancellationToken ct)
    {
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == interest.ListingId, ct)
            ?? throw NotFoundException.For("listing", interest.ListingId);

        var users = await identityDb.Users.AsNoTracking()
            .Where(u => u.Id == interest.BuyerUserId || u.Id == interest.SellerUserId)
            .ToListAsync(ct);

        var buyer = users.First(u => u.Id == interest.BuyerUserId);
        var seller = users.First(u => u.Id == interest.SellerUserId);
        var unlock = interest.Status is InterestStatus.Accepted or InterestStatus.Disputed;

        MtaaniAgentSnippet? mtaaniAgent = null;
        if (interest.MtaaniAgentId is { } agentId)
        {
            mtaaniAgent = new MtaaniAgentSnippet(
                agentId,
                interest.MtaaniAgentName ?? $"Agent {agentId}",
                interest.MtaaniLocationId,
                interest.MtaaniLocationName,
                interest.MtaaniEstimatedFeeKes);
        }

        return new InterestDetail(
            interest.Id,
            interest.ListingId,
            listing.Title,
            listing.Intent,
            interest.Status,
            interest.HandoffMode,
            interest.City,
            interest.Message,
            interest.CreatedAtUtc,
            interest.AcceptedAtUtc,
            interest.ReservedUntilUtc,
            interest.BuyerCompletedAtUtc,
            interest.SellerCompletedAtUtc,
            interest.DisputeReason,
            new PartySnippet(buyer.Id, buyer.DisplayName, buyer.City, unlock ? buyer.PhoneE164 : null),
            new PartySnippet(seller.Id, seller.DisplayName, seller.City, unlock ? seller.PhoneE164 : null),
            mtaaniAgent);
    }

    private async Task<MtaaniAgent?> ResolveMtaaniAgentAsync(
        HandoffMode mode,
        int? mtaaniAgentId,
        CancellationToken ct)
    {
        if (mode == HandoffMode.Meetup)
        {
            if (mtaaniAgentId is not null)
            {
                throw new ValidationException(
                    "mtaani_agent_id is only allowed for pickup_mtaani handoff.",
                    new Dictionary<string, string[]> { ["mtaani_agent_id"] = ["Not allowed for meetup."] });
            }

            return null;
        }

        if (mtaaniAgentId is null)
        {
            throw new ValidationException(
                "mtaani_agent_id is required for pickup_mtaani handoff.",
                new Dictionary<string, string[]> { ["mtaani_agent_id"] = ["Required."] });
        }

        var agent = await mtaani.GetAgentAsync(mtaaniAgentId.Value, ct)
            ?? throw NotFoundException.For("mtaani_agent", mtaaniAgentId.Value);

        return agent;
    }

    private async Task<Identity.Entities.User> RequirePhoneVerifiedAsync(Guid userId, CancellationToken ct)
    {
        var user = await RequireUserAsync(userId, ct);
        if (user.PhoneVerifiedAtUtc is null)
        {
            throw new ForbiddenDomainException(
                "phone_not_verified",
                "Verify your phone with SMS OTP before arranging a book.");
        }

        return user;
    }

    private async Task<Identity.Entities.User> RequireUserAsync(Guid userId, CancellationToken ct) =>
        await identityDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
        ?? throw new UnauthorizedDomainException("unauthorized", "Authentication required.");
}
