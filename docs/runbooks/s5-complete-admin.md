# S5 — Complete / admin

Prereqs: Postgres; API `:5080`.

Staff seed: `admin@vitabu.local` / `AdminPassword1!`

```bash
# After accept, each party confirms:
curl -X POST -H "Authorization: Bearer $BUYER" http://localhost:5080/interests/<id>/complete
curl -X POST -H "Authorization: Bearer $SELLER" http://localhost:5080/interests/<id>/complete

# Rate + report
curl -X POST -H "Authorization: Bearer $BUYER" -H "Content-Type: application/json" \
  http://localhost:5080/interests/<id>/rate -d '{"stars":5}'
curl -X POST -H "Authorization: Bearer $BUYER" -H "Content-Type: application/json" \
  http://localhost:5080/listings/<listingId>/reports -d '{"reason":"spam_or_scam"}'

# Admin hide
curl -X POST -H "Authorization: Bearer $STAFF" \
  http://localhost:5080/admin/listings/<listingId>/hide
```

Admin SPA: `admin/` on `:5174` — login as staff, Listings / Reports tabs.
