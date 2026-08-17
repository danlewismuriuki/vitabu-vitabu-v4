# S3 — Sell (listings write)

## IS
- Phone-verified parents create listings that publish **instant Active**
- Update own listing fields; pause / resume
- Own listings via `GET /me/listings` (+ get by id for edit)
- CBC smart title search `GET /catalog/titles`
- Cover image as **stub URL** (MinIO upload later)
- Web: `/sell`, `/my-listings`, `/my-listings/[id]/edit`

## IS NOT
- MinIO / multipart photo upload
- Admin approve-before-publish
- Deals, messages, interest counts
- Soft-delete / sold / reserved transitions (later deal slices)

## Invariants
- `phone_verified` required for create / update / pause / resume
- Public browse still only shows `active`
- Owner can see own `paused` listings in `/me/listings`
- Intents: sale | free | exchange; sale requires `price_kes`
- Cover image URL required on publish (stub OK)

## Error codes (examples)
| error | HTTP |
|-------|------|
| validation_failed | 400 |
| unauthorized | 401 |
| phone_not_verified | 403 |
| listing_not_found | 404 |
| listing_not_editable | 400 |

## Exit criteria
- [x] Phone-verified user creates listing → appears in `GET /listings`
- [x] Unverified user gets 403 on create
- [x] Pause hides from public browse; resume restores
- [x] Web sell + my-listings + edit pages work
- [x] Tests + OpenAPI ops grepped in CI
