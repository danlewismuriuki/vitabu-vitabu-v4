# S5 — Complete / dispute / rate + Admin moderate

## IS
- **Dual confirm** complete: buyer and seller each confirm; listing closes only when both confirm
- **Dispute** an accepted/completing deal with a reason
- **Rate** the other party 1–5 after completed (one rating per side per deal)
- **Report listing** (abuse / condition / photocopy / child privacy, etc.)
- **Staff** users (`is_staff`) can list reports and **hide** listings
- Admin SPA: login + listings hide + reports queue
- Reserve past `reserved_until_utc` can be expired back to Active (on access / expire endpoint)

## IS NOT
- Full RBAC permission matrix
- Automatic 7-day silence job (document; expire endpoint covers reserve)
- Message report / block user (later)
- Catalog CRUD in admin
- Escrow / refunds

## Invariants
- Public browse never shows `hidden` / non-active
- Ratings only after `completed`
- Admin mutations require `is_staff`
- Seed staff from `Seed:AdminEmail` + password

## Exit criteria
- [x] Dual confirm closes deal
- [x] Dispute + report → admin can hide listing
- [x] Rate after complete
- [x] Admin SPA can hide a listing
- [x] Tests + OpenAPI CI greps
