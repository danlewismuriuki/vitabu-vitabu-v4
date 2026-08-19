# S4 — Deals

Prereqs: Postgres; API `:5080`; two phone-verified accounts (buyer + seller).

```bash
# Buyer sends interest
curl -s -X POST "http://localhost:5080/listings/<listingId>/interests" \
  -H "Authorization: Bearer $BUYER" -H "Content-Type: application/json" \
  -d '{"handoff_mode":"meetup","city":"Nairobi","message":"Saturday OK"}'

# Seller inbox
curl -s -H "Authorization: Bearer $SELLER" \
  "http://localhost:5080/me/listings/<listingId>/interests"

# Accept → phones unlock on detail
curl -s -X POST -H "Authorization: Bearer $SELLER" \
  "http://localhost:5080/interests/<interestId>/accept"

curl -s -H "Authorization: Bearer $BUYER" \
  "http://localhost:5080/me/notifications"
```

Web: `/arrange/[listingId]`, `/my-interests`, `/my-listings/[id]/interests`, `/interests/[id]`, `/notifications`.
