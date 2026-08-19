# S4 — Deals (interest / arrange / reserve + notifications)

## IS
- Phone-verified buyers send **arrange / interest** requests while listing stays **Active**
- Public cards show **“X interested”** (pending open requests)
- Seller **Accept** one → listing `reserved`, others `waitlisted`, **phones unlock** for both parties
- Seller Decline; buyer Cancel; either party **Release** accepted deal → listing `Active` again
- In-app notifications + email on new interest / accept / release
- Web: `/arrange/[listingId]`, `/my-interests`, seller interest inbox, `/notifications`

## IS NOT
- Full chat threads / messaging UI (later)
- 72h background worker (field + manual release in S4; job later)
- M-Pesa / escrow / cart
- Exchange propose with second listing
- Ratings / disputes

## Invariants
- Many pending interests allowed; **one** accepted reserve at a time
- Cannot interest your own listing
- Phone never on public listing payloads; phones only on accepted deal for parties
- Browse still Active-only

## Error codes (examples)
| error | HTTP |
|-------|------|
| phone_not_verified | 403 |
| cannot_interest_own_listing | 400 |
| interest_already_exists | 409 |
| listing_not_available | 409 |
| interest_not_found | 404 |

## Exit criteria
- [x] Buyer creates interest → count increments; seller notified
- [x] Accept → reserved; phones on deal detail; browse hides listing
- [x] Release → active again
- [x] Web arrange + inbox + notifications
- [x] Tests + OpenAPI CI greps
