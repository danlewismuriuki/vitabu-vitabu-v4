using Vitabu.Modules.Deals.PickupMtaani;

namespace Vitabu.Infrastructure.PickupMtaani;

/// <summary>
/// In-memory agents shaped like Pickup Mtaani API responses — used when ApiKey is unset.
/// </summary>
public sealed class DevPickupMtaaniClient : IPickupMtaaniClient
{
    private static readonly IReadOnlyList<MtaaniLocation> Locations =
    [
        new(101, "Westlands", 1),
        new(102, "Nairobi CBD", 1),
        new(201, "Kisumu Mega City", 2),
        new(301, "Nyali", 3)
    ];

    private static readonly IReadOnlyList<MtaaniAgent> Agents =
    [
        new(1001, "Westlands Book Drop Agent", 101, "Westlands", "Westlands"),
        new(1002, "Tom Mboya Street Agent", 102, "Nairobi CBD", "CBD"),
        new(2001, "Kisumu Mega Agent", 201, "Kisumu Mega City", "Mega City"),
        new(3001, "Nyali Centre Agent", 301, "Nyali", "Nyali")
    ];

    public Task<IReadOnlyList<MtaaniLocation>> ListLocationsAsync(
        string? search = null,
        CancellationToken ct = default)
    {
        IEnumerable<MtaaniLocation> q = Locations;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(l => l.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<MtaaniLocation>>(q.ToList());
    }

    public Task<IReadOnlyList<MtaaniAgent>> ListAgentsAsync(
        int? locationId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        IEnumerable<MtaaniAgent> q = Agents;
        if (locationId is { } lid)
        {
            q = q.Where(a => a.LocationId == lid);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a =>
                a.BusinessName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                (a.LocationName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Area?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return Task.FromResult<IReadOnlyList<MtaaniAgent>>(q.ToList());
    }

    public Task<MtaaniAgent?> GetAgentAsync(int agentId, CancellationToken ct = default) =>
        Task.FromResult(Agents.FirstOrDefault(a => a.Id == agentId));

    public Task<MtaaniDeliveryCharge?> GetAgentPackageChargeAsync(
        int senderAgentId,
        int receiverAgentId,
        CancellationToken ct = default)
    {
        if (Agents.All(a => a.Id != senderAgentId) || Agents.All(a => a.Id != receiverAgentId))
        {
            return Task.FromResult<MtaaniDeliveryCharge?>(null);
        }

        var amount = senderAgentId == receiverAgentId ? 0 : 150;
        return Task.FromResult<MtaaniDeliveryCharge?>(new MtaaniDeliveryCharge(amount));
    }
}
