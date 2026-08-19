# S9 — School profiles

Prereqs: API `:5080`; staff JWT for create.

```bash
# Public schools
curl http://localhost:5080/schools

# Admin create
curl -X POST -H "Authorization: Bearer $STAFF" -H "Content-Type: application/json" \
  http://localhost:5080/admin/schools -d '{
    "name":"Kenyatta Primary","city":"Nairobi","contact_name":"Head teacher"
  }'

# Donate listing targeting a school
curl -X POST -H "Authorization: Bearer $SELLER" -H "Content-Type: application/json" \
  http://localhost:5080/listings -d '{
    "title":"Grade 4 Maths","grade":"Grade 4","subject":"Mathematics",
    "city":"Nairobi","intent":"donate_school","condition":"good",
    "school_id":"<schoolId>",
    "description":"For Kenyatta Primary","cover_image_url":"https://placehold.co/600x800/png?text=Donate"
  }'
```

Web: `/donate` · Sell → Donate school + school picker · Admin SPA Schools tab.
