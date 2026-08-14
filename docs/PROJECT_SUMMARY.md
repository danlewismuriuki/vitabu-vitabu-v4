# Vitabu Vitabu — Project Summary (updated for v4)

**Product name:** Vitabu Vitabu  
**Tagline:** *Real Parents. Real Savings. Real Books.*  
**Positioning:** Built for Kenya — a peer marketplace for CBC school books (sell, free giveaways, exchange; school donate next).

**Status:**  
- **This repo (`vitabu-vitabu-v3`)** = legacy Vite + Firebase frontend prototype (reference for brand/CBC ideas only).  
- **Next build (`vitabu-vitabu-v4`)** = greenfield monorepo — contract-first, Next.js + API + admin — per the locked product plan.

Full planning detail: Cursor plan `vitabu_vitabu_v4_550f4c55.plan.md` (also copy in Downloads when exported).

---

## What the product is

Vitabu Vitabu helps Kenyan **parents** circulate CBC schoolbooks: buy/sell used copies, give books away free, exchange, and later donate to schools. One shared deal flow (discover → connect → handoff → complete), not four separate apps.

**Not** a heavy seller admin for every parent — marketplace UX + light “My listings.” Platform ops use a **separate admin** app.

---

## Target users

| Audience | Surface |
|----------|---------|
| Parents (browse / list / deal) | Next.js public web (`web/`) |
| Same parents managing their books | `/sell`, `/my-listings`, `/messages` in same web app |
| Platform staff | Separate `admin/` SPA |

Accounts are for **18+ / parents-guardians**, not children.

---

## Core user flow (locked)

1. **Browse** all Active CBC listings (mixture of sale / free / exchange) with filters; soft geo “near you” but Kenya-wide allowed.  
2. **Signup** (email + password + city); **phone SMS OTP** required before sell/message/accept.  
3. **List** a book on `/sell` (page, not modal) with CBC catalog, condition, camera/web photos.  
4. Others **request / arrange**; listing shows **“X interested.”**  
5. Seller **accepts one** → reserved → **verified phones** shared (same number as OTP) + wa.me.  
6. **Meetup** (safety checklist) or **nearby Pickup Mtaani** (buyer pays agent at point).  
7. Both **confirm complete** (or dispute); ratings; 72h reserve timeout if ghosting.  

**Checkout** = arrange handoff, not platform M-Pesa (M-Pesa later).

---

## Listing intents

| Intent | Book money? | First build? |
|--------|-------------|--------------|
| Sale | Peer KES | Yes |
| Free | No | Yes |
| Exchange | No | Yes |
| Donate to school | No | Later phase |

---

## Product decisions (high level)

- Progressive gates: browse free; **phone verified** to act; email for login/notify.  
- Instant listing publish + report/admin hide.  
- MinIO for photos; mobile **web camera** (no native app required).  
- Notifications: in-app + email default on (toggleable); SMS OTP early.  
- Privacy: book artwork OK; no real kids / pupil IDs in uploads; KES; Africa/Nairobi.  
- Full CBC taxonomy (grades, subjects, terms, material types) + SEO hubs.  
- CEMES-style: OpenAPI contract → API → web/admin vertical slices.

---

## v3 codebase (this repo) — what it was

- Vite + React + Tailwind SPA; Firebase Auth/Hosting.  
- Mock books/messaging; view-state navigation; auth fragmented/broken.  
- **Reuse from v3:** colors/fonts (`tailwind.config.js`, `index.css`), CBC catalog ideas, UX concepts — **not** Firebase auth shell.

---

## v4 technical shape (to build)

```
vitabu-vitabu-v4/
  contract/openapi.yaml
  api/                 # C# .NET 8 — Vitabu.Api, Core, Infrastructure, Modules.*
  web/                 # Next.js (SEO)
  admin/               # Vite admin SPA
  infra/               # Docker: Postgres, MinIO, Mailpit
  design/              # tokens + Tailwind preset
  docs/                # flows, design-system, backend-cemes-practices, slices, runbooks
```

**Backend:** CEMES-adapted C# — see [`docs/backend-cemes-practices.md`](docs/backend-cemes-practices.md) (trunk-based, OpenAPI first, modules, FluentValidation, snake_case wire, Problem errors, SS0 stubs, integration tests).

**Implementation start:** S0 scaffold → S1 Identity+OTP → S2 Catalog/SEO → S3 Sell → S4 Deals/notify → S5 Complete/admin → later donate/M-Pesa/etc.

---

## Brand & UI design (tight docs)

Design is **documented and mapped** — use these files as source of truth for v4:

| Document | Path |
|----------|------|
| Design system | [`docs/design-system.md`](docs/design-system.md) |
| Brand voice | [`docs/brand.md`](docs/brand.md) |
| CSS variables | [`design/tokens.css`](design/tokens.css) |
| Tailwind preset | [`design/tailwind.preset.js`](design/tailwind.preset.js) |
| **C# / CEMES backend practices** | [`docs/backend-cemes-practices.md`](docs/backend-cemes-practices.md) |
| Historical v3 theme | `tailwind.config.js`, `src/index.css` |

**Quick map:** page `neutral-50` · wordmark `primary-700` · CTA `accent-500`/`600` · success `secondary-*` · fonts Poppins + Lato.

---

## Bottom line

Vitabu Vitabu is a **Kenya CBC parent marketplace** for sale, free, and exchange (donate next), with meetup/Mtaani handoff, phone-verified trust, and SEO-friendly Next.js. **v3** is the old prototype; **v4** is the planned professional rebuild — contract-first slices, not a Firebase patch.
