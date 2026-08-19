# S7 — Donate school

Prereqs: API `:5080`; phone-verified seller JWT.

```bash
# Create donate listing (no price)
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  http://localhost:5080/listings -d '{
    "title":"Grade 4 Maths","grade":"Grade 4","subject":"Mathematics",
    "city":"Nairobi","intent":"donate_school","condition":"good",
    "description":"For a neighbourhood primary school.","cover_image_url":"https://placehold.co/600x800/png?text=Donate"
  }'

# Browse
curl "http://localhost:5080/listings?intent=donate_school"
```

Web: `/donate` · Sell intent **Donate to school**.
