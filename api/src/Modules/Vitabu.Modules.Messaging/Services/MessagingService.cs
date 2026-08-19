using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Messaging.Contracts;
using Vitabu.Modules.Messaging.Entities;
using Vitabu.Modules.Messaging.Persistence;
using Vitabu.Modules.Notifications.Services;
using CoreValidationException = Vitabu.Core.Exceptions.ValidationException;

namespace Vitabu.Modules.Messaging.Services;

public interface IMessagingService
{
    Task<ThreadDetail> OpenThreadAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task<ThreadPage> ListMineAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<ThreadDetail> GetThreadAsync(Guid userId, Guid threadId, CancellationToken ct = default);
    Task<MessageItem> SendAsync(Guid userId, Guid threadId, SendMessageRequest request, CancellationToken ct = default);
}

public sealed class MessagingService(
    IMessagingDbContext messagingDb,
    IListingsDbContext listingsDb,
    IIdentityDbContext identityDb,
    INotificationService notifications,
    IValidator<SendMessageRequest> sendValidator) : IMessagingService
{
    public async Task<ThreadDetail> OpenThreadAsync(Guid userId, Guid listingId, CancellationToken ct = default)
    {
        await RequirePhoneVerifiedAsync(userId, ct);

        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        if (listing.SellerUserId == userId)
        {
            throw new CoreValidationException(
                "You cannot message yourself about your own listing.",
                new Dictionary<string, string[]>
                {
                    ["listing_id"] = ["Cannot open a thread on your own listing."]
                });
        }

        if (listing.Status != ListingStatus.Active && listing.Status != ListingStatus.Reserved)
        {
            throw new ConflictException("listing_not_available", "This listing is not open for messaging.");
        }

        var existing = await messagingDb.MessageThreads
            .FirstOrDefaultAsync(t => t.ListingId == listingId && t.BuyerUserId == userId, ct);
        if (existing is not null)
        {
            return await GetThreadAsync(userId, existing.Id, ct);
        }

        var now = DateTime.UtcNow;
        var thread = new MessageThread
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            BuyerUserId = userId,
            SellerUserId = listing.SellerUserId,
            CreatedAtUtc = now,
            LastMessageAtUtc = now
        };
        messagingDb.MessageThreads.Add(thread);
        await messagingDb.SaveChangesAsync(ct);

        await notifications.NotifyAsync(
            listing.SellerUserId,
            "message_thread_opened",
            "New message thread",
            $"Someone wants to chat about “{listing.Title}”.",
            thread.Id,
            ct: ct);

        return await GetThreadAsync(userId, thread.Id, ct);
    }

    public async Task<ThreadPage> ListMineAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = messagingDb.MessageThreads.AsNoTracking()
            .Where(t => t.BuyerUserId == userId || t.SellerUserId == userId);

        var total = await q.CountAsync(ct);
        var threads = await q
            .OrderByDescending(t => t.LastMessageAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var cards = await MapCardsAsync(userId, threads, ct);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new ThreadPage(cards, page, pageSize, total, totalPages);
    }

    public async Task<ThreadDetail> GetThreadAsync(Guid userId, Guid threadId, CancellationToken ct = default)
    {
        var thread = await messagingDb.MessageThreads.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == threadId, ct)
            ?? throw NotFoundException.For("thread", threadId);

        EnsureParticipant(userId, thread);

        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == thread.ListingId, ct);
        var listingTitle = listing?.Title ?? "Listing";

        var otherId = thread.BuyerUserId == userId ? thread.SellerUserId : thread.BuyerUserId;
        var other = await identityDb.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == otherId, ct);
        var otherName = other?.DisplayName ?? "Parent";

        var messages = await messagingDb.Messages.AsNoTracking()
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(ct);

        var senderIds = messages.Select(m => m.SenderUserId).Distinct().ToList();
        var names = await identityDb.Users.AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var items = messages.Select(m => new MessageItem(
            m.Id,
            m.SenderUserId,
            names.GetValueOrDefault(m.SenderUserId, "Parent"),
            m.Body,
            m.CreatedAtUtc)).ToList();

        return new ThreadDetail(
            thread.Id,
            thread.ListingId,
            listingTitle,
            thread.BuyerUserId,
            thread.SellerUserId,
            otherName,
            thread.CreatedAtUtc,
            thread.LastMessageAtUtc,
            items);
    }

    public async Task<MessageItem> SendAsync(
        Guid userId,
        Guid threadId,
        SendMessageRequest request,
        CancellationToken ct = default)
    {
        await RequirePhoneVerifiedAsync(userId, ct);
        var validation = await sendValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new CoreValidationException(
                "Validation failed.",
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => ToSnake(g.Key),
                        g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var thread = await messagingDb.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == threadId, ct)
            ?? throw NotFoundException.For("thread", threadId);

        EnsureParticipant(userId, thread);

        var body = request.Body.Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new CoreValidationException(
                "Message body is required.",
                new Dictionary<string, string[]> { ["body"] = ["Required."] });
        }

        var now = DateTime.UtcNow;
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            SenderUserId = userId,
            Body = body,
            CreatedAtUtc = now
        };
        messagingDb.Messages.Add(message);
        thread.LastMessageAtUtc = now;
        await messagingDb.SaveChangesAsync(ct);

        var sender = await identityDb.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        var recipientId = thread.BuyerUserId == userId ? thread.SellerUserId : thread.BuyerUserId;
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == thread.ListingId, ct);

        await notifications.NotifyAsync(
            recipientId,
            "new_message",
            "New message",
            $"{sender?.DisplayName ?? "A parent"}: {Truncate(body, 120)}",
            thread.Id,
            ct: ct);

        return new MessageItem(
            message.Id,
            message.SenderUserId,
            sender?.DisplayName ?? "Parent",
            message.Body,
            message.CreatedAtUtc);
    }

    private async Task<IReadOnlyList<ThreadCard>> MapCardsAsync(
        Guid userId,
        IReadOnlyList<MessageThread> threads,
        CancellationToken ct)
    {
        if (threads.Count == 0)
        {
            return [];
        }

        var listingIds = threads.Select(t => t.ListingId).Distinct().ToList();
        var listings = await listingsDb.Listings.AsNoTracking()
            .Where(l => listingIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        var otherIds = threads
            .Select(t => t.BuyerUserId == userId ? t.SellerUserId : t.BuyerUserId)
            .Distinct()
            .ToList();
        var users = await identityDb.Users.AsNoTracking()
            .Where(u => otherIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var threadIds = threads.Select(t => t.Id).ToList();
        var lastBodies = await messagingDb.Messages.AsNoTracking()
            .Where(m => threadIds.Contains(m.ThreadId))
            .GroupBy(m => m.ThreadId)
            .Select(g => new
            {
                ThreadId = g.Key,
                Body = g.OrderByDescending(m => m.CreatedAtUtc).Select(m => m.Body).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ThreadId, x => x.Body, ct);

        return threads.Select(t =>
        {
            var otherId = t.BuyerUserId == userId ? t.SellerUserId : t.BuyerUserId;
            lastBodies.TryGetValue(t.Id, out var preview);
            return new ThreadCard(
                t.Id,
                t.ListingId,
                listings.TryGetValue(t.ListingId, out var listing) ? listing.Title : "Listing",
                t.BuyerUserId,
                t.SellerUserId,
                users.GetValueOrDefault(otherId, "Parent"),
                preview is null ? null : Truncate(preview, 100),
                t.LastMessageAtUtc,
                t.CreatedAtUtc);
        }).ToList();
    }

    private static void EnsureParticipant(Guid userId, MessageThread thread)
    {
        if (thread.BuyerUserId != userId && thread.SellerUserId != userId)
        {
            throw new ForbiddenDomainException("not_thread_participant", "You are not part of this conversation.");
        }
    }

    private async Task RequirePhoneVerifiedAsync(Guid userId, CancellationToken ct)
    {
        var user = await identityDb.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw NotFoundException.For("user", userId);

        if (user.PhoneVerifiedAtUtc is null)
        {
            throw new ForbiddenDomainException(
                "phone_not_verified",
                "Verify your phone with SMS OTP before messaging.");
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 1)] + "…";

    private static string ToSnake(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var chars = new List<char>();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) chars.Add('_');
            chars.Add(char.ToLowerInvariant(c));
        }
        return new string(chars.ToArray());
    }
}
