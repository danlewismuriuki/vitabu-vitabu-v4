# S9 — School profiles

## IS
- **School** directory: name, city, optional contact, `is_verified`
- Public `GET /schools`, `GET /schools/{id}`
- Staff `POST /admin/schools` to add a school
- Donate listings may set optional **`school_id`**
- Listing detail includes school snippet when linked
- Filter browse `?school_id=`
- Seed a few Kenyan schools
- Web: school picker on sell (donate), schools on `/donate`, admin Schools tab

## IS NOT
- School login / coordinator accounts
- Donate campaigns / drives
- Automatic school matching

## Invariants
- `school_id` only allowed when intent is `donate_school`
- Referenced school must exist
- Public school list is verified schools only (admin create sets verified true for MVP)

## Exit criteria
- [x] List / get / admin create schools
- [x] Donate listing can target a school
- [x] Web + admin UX
- [x] Tests + OpenAPI CI greps
