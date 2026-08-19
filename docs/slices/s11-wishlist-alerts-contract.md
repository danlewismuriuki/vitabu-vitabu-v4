# S11 — Wishlist email / in-app alerts

## IS
- When a **new listing** is created, parents who wishlisted a book with the same **grade + subject** get an in-app notification (and email when the account has an email)
- When a wishlisted listing becomes **unavailable** (deal accepted → reserved), those who saved it get an alert
- Opt-out via `wishlist_alerts_enabled` on the user profile (`PATCH /auth/me/notification-prefs`), default **on**
- Web: toggle on `/wishlist`

## IS NOT
- SMS / WhatsApp
- Marketing digests or weekly digests
- Matching on title/CBC title id beyond grade+subject
- Alerts for the seller of the new listing

## Invariants
- Respects `wishlist_alerts_enabled` (skip when false)
- Seller of a new listing is never notified about their own create
- Alert failures must not fail listing create or deal accept (logged)

## Exit criteria
- [x] Similar-listing + unavailable alerts fire
- [x] Prefs PATCH + profile field
- [x] Wishlist page toggle
- [x] Tests + OpenAPI CI greps
