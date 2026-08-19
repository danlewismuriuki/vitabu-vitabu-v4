# S10 — Pickup Mtaani (mock-first)

Prereqs: API `:5080`. Leave `PickupMtaani:ApiKey` empty for the in-memory stub.

```bash
# Dev stub agents
curl "http://localhost:5080/mtaani/agents?search=Nairobi"
curl "http://localhost:5080/mtaani/locations"

# Arrange with agent
curl -X POST -H "Authorization: Bearer $BUYER" -H "Content-Type: application/json" \
  http://localhost:5080/listings/<listingId>/interests -d '{
    "handoff_mode":"pickup_mtaani","city":"Nairobi","mtaani_agent_id":1001,
    "message":"Can drop Friday"
  }'
```

When you receive a key from support@api.pickupmtaani.com:

```json
"PickupMtaani": {
  "BaseUrl": "https://api.pickupmtaani.com/api/v1",
  "ApiKey": "<your-key>"
}
```

Restart the API — it switches to the live HTTP client automatically.
