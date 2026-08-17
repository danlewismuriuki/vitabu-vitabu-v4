# S3 — Sell

Prereqs: Postgres up; API on `:5080`; phone OTP verified for the seller account.

```bash
dotnet run --project api/src/Vitabu.Api --launch-profile http
```

```bash
# CBC smart search
curl -s "http://localhost:5080/catalog/titles?q=Math&page_size=5"

# Create (Bearer from login + phone verified)
curl -s -X POST http://localhost:5080/listings \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Book","grade":"Grade 4","subject":"Mathematics","city":"Nairobi","intent":"sale","condition":"good","price_kes":400,"description":"Good copy","cover_image_url":"https://placehold.co/600x800/png?text=Book"}'

curl -s -H "Authorization: Bearer $TOKEN" "http://localhost:5080/me/listings"
```

Web: `/sell`, `/my-listings`, `/my-listings/[id]/edit`. Unverified users are sent to `/verify-phone`.
