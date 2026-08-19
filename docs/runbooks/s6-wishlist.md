# S6 — Wishlist

Prereqs: Postgres; API `:5080`; logged-in JWT (`$TOKEN`).

```bash
# Save
curl -X POST -H "Authorization: Bearer $TOKEN" \
  http://localhost:5080/listings/<listingId>/wishlist

# Status
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5080/listings/<listingId>/wishlist

# List
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5080/me/wishlist

# Remove
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  http://localhost:5080/listings/<listingId>/wishlist
```

Web: `/wishlist` · Save toggle on `/books/[id]`.
