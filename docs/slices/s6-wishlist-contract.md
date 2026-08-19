# S6 — Wishlist

## IS
- Logged-in parent can **save** an active listing to wishlist
- **Remove** from wishlist
- **List** my wishlist (`/wishlist` web page)
- Check whether a listing is on my wishlist (for book detail toggle)
- No phone verification required to wishlist (browse + save OK unverified)

## IS NOT
- Email grade/subject alerts / marketing digests (see S11)
- Auto-match job when matching listings appear (see S11 in-process alerts)
- Public wishlists
- donate_school

## Invariants
- Cannot wishlist your own listing
- Can only add Active (public) listings
- Unique (user_id, listing_id)
- Auth required for all wishlist ops

## Exit criteria
- [x] Add / remove / list work end-to-end
- [x] Book detail Save toggle
- [x] `/wishlist` page
- [x] Tests + OpenAPI CI greps
