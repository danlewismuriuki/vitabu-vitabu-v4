# S7 — Donate to school

## IS
- Listing intent **`donate_school`** (no price; peer or school coordinator claims via existing arrange flow)
- Listing end status **`donated`** after dual-confirm complete
- Sell / edit UI option + browse filter + `/donate` hub
- Facets include `donate_school`

## IS NOT
- School accounts / verified school profiles
- Campaigns / drives admin CRUD
- Separate claim flow that skips parent interest
- Platform logistics for school drop-off

## Invariants
- `donate_school` listings must not have `price_kes`
- Complete maps `donate_school` → listing `donated`
- Public browse only Active (unchanged)

## Exit criteria
- [x] Create donate_school listing
- [x] Dual confirm → donated
- [x] Browse /donate + sell option
- [x] Tests + OpenAPI CI greps
