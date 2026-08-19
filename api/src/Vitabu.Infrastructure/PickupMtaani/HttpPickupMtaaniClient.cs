using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vitabu.Modules.Deals.PickupMtaani;

namespace Vitabu.Infrastructure.PickupMtaani;

public sealed class HttpPickupMtaaniClient(
    HttpClient http,
    IOptions<PickupMtaaniOptions> options,
    ILogger<HttpPickupMtaaniClient> logger) : IPickupMtaaniClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<IReadOnlyList<MtaaniLocation>> ListLocationsAsync(
        string? search = null,
        CancellationToken ct = default)
    {
        var url = "locations";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?searchKey={Uri.EscapeDataString(search.Trim())}";
        }

        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var payload = await res.Content.ReadFromJsonAsync<LocationsEnvelope>(JsonOptions, ct);
        var items = payload?.Data ?? [];
        return items
            .Select(x => new MtaaniLocation(x.Id, x.Name ?? $"Location {x.Id}", x.ZoneId))
            .ToList();
    }

    public async Task<IReadOnlyList<MtaaniAgent>> ListAgentsAsync(
        int? locationId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var qs = new List<string>();
        if (locationId is { } lid) qs.Add($"locationId={lid}");
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"searchKey={Uri.EscapeDataString(search.Trim())}");
        var url = qs.Count == 0 ? "agents" : $"agents?{string.Join('&', qs)}";

        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var items = await res.Content.ReadFromJsonAsync<List<AgentDto>>(JsonOptions, ct) ?? [];
        return items
            .Select(x => new MtaaniAgent(
                x.Id,
                x.BusinessName ?? $"Agent {x.Id}",
                locationId,
                null,
                null))
            .ToList();
    }

    public async Task<MtaaniAgent?> GetAgentAsync(int agentId, CancellationToken ct = default)
    {
        var agents = await ListAgentsAsync(ct: ct);
        var match = agents.FirstOrDefault(a => a.Id == agentId);
        if (match is not null)
        {
            return match;
        }

        logger.LogWarning("Pickup Mtaani agent {AgentId} not found in list response", agentId);
        return null;
    }

    public async Task<MtaaniDeliveryCharge?> GetAgentPackageChargeAsync(
        int senderAgentId,
        int receiverAgentId,
        CancellationToken ct = default)
    {
        var url =
            $"delivery-charge/agent-package?senderAgentID={senderAgentId}&receiverAgentID={receiverAgentId}";
        using var res = await http.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Pickup Mtaani delivery charge failed ({Status}) for {Sender}->{Receiver}",
                (int)res.StatusCode,
                senderAgentId,
                receiverAgentId);
            return null;
        }

        var dto = await res.Content.ReadFromJsonAsync<ChargeDto>(JsonOptions, ct);
        if (dto is null)
        {
            return null;
        }

        var amount = dto.Amount ?? dto.DeliveryFee ?? dto.Fee ?? 0;
        return new MtaaniDeliveryCharge(amount, dto.Currency ?? "KES");
    }

    private sealed record LocationsEnvelope(List<LocationDto>? Data);
    private sealed record LocationDto(int Id, string? Name, int? ZoneId);
    private sealed record AgentDto(int Id, string? BusinessName);
    private sealed record ChargeDto(int? Amount, int? DeliveryFee, int? Fee, string? Currency);
}
