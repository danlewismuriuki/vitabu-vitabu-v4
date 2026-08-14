# S2 — Catalog & listings

Prereqs: Postgres up; API on `:5080`.

```bash
dotnet run --project api/src/Vitabu.Api --launch-profile http
```

```bash
curl -s "http://localhost:5080/catalog/facets"
curl -s "http://localhost:5080/listings?page_size=5"
curl -s "http://localhost:5080/listings?intent=free&city=Nairobi"
# pick an id from list:
curl -s "http://localhost:5080/listings/<id>"
```

Web: `/books`, `/books/[id]`, home featured strip.
