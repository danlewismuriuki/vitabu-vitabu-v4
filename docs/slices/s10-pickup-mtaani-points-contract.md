# S10 — Pickup Mtaani partner facade (mock-first)

## IS
- `IPickupMtaaniClient` matching partner API shape (`locations`, `agents`, delivery charge)
- **Dev stub** when `PickupMtaani:ApiKey` is empty; **HTTP client** when key is set
- Public Vitabu facade: `GET /mtaani/locations`, `/mtaani/agents`, `/mtaani/delivery-charge`
- Arrange with `handoff_mode=pickup_mtaani` requires `mtaani_agent_id`
- Interest stores agent snapshot (id, name, location)
- Web arrange agent picker

## IS NOT
- Creating Mtaani packages / STK payment
- Dropped / collected tracking webhooks
- Admin-curated pickup_points table

## Invariants
- `mtaani_agent_id` required for `pickup_mtaani`; forbidden for `meetup`
- Agent must resolve via the client (stub or live)
- Empty ApiKey → never call live partner

## Exit criteria
- [x] Facade + stub + Http client switch
- [x] Arrange links Mtaani agent
- [x] Tests + OpenAPI CI greps
