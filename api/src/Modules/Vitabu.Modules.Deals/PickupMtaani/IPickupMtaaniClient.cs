namespace Vitabu.Modules.Deals.PickupMtaani;

public sealed class PickupMtaaniOptions
{
    public const string SectionName = "PickupMtaani";

    public string BaseUrl { get; set; } = "https://api.pickupmtaani.com/api/v1";
    public string? ApiKey { get; set; }
}

public sealed record MtaaniLocation(int Id, string Name, int? ZoneId = null);

public sealed record MtaaniAgent(
    int Id,
    string BusinessName,
    int? LocationId = null,
    string? LocationName = null,
    string? Area = null);

public sealed record MtaaniDeliveryCharge(int AmountKes, string? Currency = "KES");

public interface IPickupMtaaniClient
{
    Task<IReadOnlyList<MtaaniLocation>> ListLocationsAsync(
        string? search = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<MtaaniAgent>> ListAgentsAsync(
        int? locationId = null,
        string? search = null,
        CancellationToken ct = default);

    Task<MtaaniAgent?> GetAgentAsync(int agentId, CancellationToken ct = default);

    Task<MtaaniDeliveryCharge?> GetAgentPackageChargeAsync(
        int senderAgentId,
        int receiverAgentId,
        CancellationToken ct = default);
}
