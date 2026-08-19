# S11 — Wishlist alerts

Prereqs: Postgres; API `:5080`; two JWTs (`$WISHLISTER`, `$SELLER` phone-verified).

```bash
# Wishlister: save a Grade 4 Math listing, then leave alerts on (default)
curl -X POST -H "Authorization: Bearer $WISHLISTER" \
  http://localhost:5080/listings/<existingListingId>/wishlist

# Opt out / in
curl -X PATCH -H "Authorization: Bearer $WISHLISTER" \
  -H "Content-Type: application/json" \
  -d '{"wishlist_alerts_enabled":false}' \
  http://localhost:5080/auth/me/notification-prefs

# Seller creates another Grade 4 Math listing → wishlister gets
# type wishlist_similar_listing on GET /me/notifications (if alerts enabled)

# Accept a deal on a wishlisted listing → wishlisters get
# type wishlist_listing_unavailable
```

Web: `/wishlist` alert toggle · `/notifications`.
