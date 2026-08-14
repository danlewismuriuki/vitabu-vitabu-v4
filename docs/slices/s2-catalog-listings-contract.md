# S2 — Catalog + public listings (SEO)

## IS
- CBC titles seed from `data/cbc-book-catalog.json`
- Public `GET /listings` (Active only) with filters: q, grade, subject, city, intent, condition, page
- Public `GET /listings/{id}` detail + seller snippet (city, display name — no phone)
- `GET /catalog/facets` for filter UI (grades, subjects, cities, intents)
- Soft geo: optional `city` filter; default “all” unless client passes city
- Next.js `/books`, `/books/[id]`, home featured cards, SEO metadata
- Demo seed listings for local/CI empty DBs

## IS NOT
- Create/edit listing (S3)
- Photos upload / MinIO
- Interest counts / deals
- Grade SEO hub matrix (basic link optional; full hubs later)
- Admin catalog CRUD

## Invariants
- Only `active` listings appear in public browse/detail
- Mixture of intents by default (sale / free / exchange)
- Phone never on public listing payloads

## Exit criteria
- [x] Seed titles + demo listings on migrate/startup
- [x] List/filter/get endpoints match OpenAPI
- [x] Web browse + detail with metadata
- [x] Tests cover list filter + get-by-id 404
