# Vitabu Vitabu v4 — Backend practices (CEMES-adapted)

Vitabu’s API is **C# / ASP.NET Core**, following **CEMES_2.0 operating practices**: contract-first, trunk-based, vertically sliced modules. This is not generic .NET advice — it is how we build Vitabu so it stays reviewable and parallelizable.

Product flows stay as locked in the v4 plan; this doc is **how the API is engineered**.

---

## 1. Stack lock

| Layer | Choice |
|-------|--------|
| API | **.NET 8**, ASP.NET Core |
| ORM | EF Core, **one** `DbContext` + migrations in Infrastructure |
| Validation | **FluentValidation** (validators in Application / Validation) |
| Auth | JWT (identity); phone OTP; permissions resolved server-side for admin |
| DB | PostgreSQL (Docker local) |
| Objects | MinIO (S3 API) |
| Contract | `contract/openapi.yaml` OpenAPI 3.1 — single source of truth |
| Web | Next.js (separate app; consumes OpenAPI) |
| Admin | Vite React SPA |

**JSON wire (CEMES ADR-0004 style):** `snake_case` property names, enums as snake_case strings, datetimes ISO-8601 UTC (`*_utc`). Next.js API client maps to camelCase TypeScript types at the edge if desired — **wire stays snake_case**.

**Error envelope:**
```json
{ "error": "listing_not_active", "message": "Human readable explanation", "errors": null }
```
Domain exceptions → middleware → HTTP. No stack traces to clients.

---

## 2. Git, branching, delivery (trunk-based)

- `main` always deployable; no direct push  
- Branch from up-to-date `main`: `feat/...`, `fix/...`, `chore/...`, `docs/...`  
- Short branches (~3 days); split if bigger  
- Open PRs early (draft OK)  
- **Squash-merge**, delete branch  
- Conventional Commits: `feat(listings): ...`, `fix(auth): ...`  
- PR: link work item, test plan, ideally **&lt; 400 lines**, split if &gt; 800  
- CI green + happy-path check; no secrets in diff  

---

## 3. Architecture: host + modules (Vitabu map)

```
api/
  src/
    Vitabu.Api/                 # Host: Program.cs, middleware, JSON options
    Vitabu.Core/                # Shared kernel: exceptions, base types
    Vitabu.Infrastructure/      # DbContext, migrations, MinIO, SMS/email adapters
    Modules/
      Vitabu.Modules.Identity/    # register, login, phone OTP, profile
      Vitabu.Modules.Catalog/     # CBC titles taxonomy
      Vitabu.Modules.Listings/    # parent listings, photos, pause
      Vitabu.Modules.Deals/       # arrange, interest, reserve, complete, dispute
      Vitabu.Modules.Messaging/   # threads / messages
      Vitabu.Modules.Notifications/
      Vitabu.Modules.Admin/       # moderate, reports, staff permissions
```

**Hard rule:** No cross-module coupling of internals.  
Modules talk via **Contracts/** (DTOs/events/interfaces), not each other’s repositories or DbSets.

**Centralized migrations, decentralized configs:**  
EF migrations live in `Vitabu.Infrastructure`; each module owns entity configurations.

**Host vs modules:**  
Api = middleware + DI wiring. Core = exceptions (`NotFoundException`, `ValidationException`, …). Infrastructure = persistence + external I/O.

**MediatR:** Use for **cross-module domain events** (e.g. `DealAccepted` → Notifications), not as a mandate for every CRUD. Prefer thin controller → service for single-module commands (closer to CEMES Website “thin controller / fat service” where that applied).

---

## 4. Contract-first slice development

### Before coding a slice
1. Update / merge `contract/openapi.yaml` (`operationId` on every op)  
2. Optional prose contract: `docs/slices/sN-<name>-contract.md` — IS / IS NOT, HTTP edges, invariants, error codes, lane map if parallel  
3. **SS0 stub-first** when parallel: shared DTOs/events land as **compiling stubs on main** before impl lanes redefine them  

### Slice sequence (Vitabu)

| Slice | Capability |
|-------|------------|
| S0 | Scaffold host, Infra DbContext, compose, OpenAPI shell (Problem, Page, bearerAuth) |
| S1 | Identity: register/login/me/phone OTP |
| S2 | Catalog + public listings list/get (SEO data) |
| S3 | Listings write: create/update/pause + image upload |
| S4 | Deals arrange/interest/accept + Notifications email/in-app |
| S5 | Deal complete/dispute/rate + Admin moderate |
| S6+ | Wishlist, donate_school, M-Pesa, Mtaani API, … |

Each slice is the **smallest end-to-end** use case. Contracts say what **later** slices own so nobody builds ahead.

---

## 5. Parallel lanes (when team &gt; 1)

- One owner per PBI / file area  
- Lane map in slice contract  
- Don’t touch files you don’t own  
- Guardian review for Core / Infrastructure / shared Contracts  

Solo: still ship **contract → stubs → impl → tests** in that order.

---

## 6. API wire conventions

| Concern | Rule |
|---------|------|
| JSON names | snake_case (`listing_id`, `created_at_utc`) |
| Enums | snake_case strings (`like_new`, `pending_review`) |
| Money | `decimal` KES; precision per contract |
| Auth | Bearer JWT; admin permissions fail closed |
| Missing resource / tenancy | Prefer **404** over leaking existence where scoped |
| Pagination | Shared `Page` schema in OpenAPI |

Global JSON options in `Program.cs` (SnakeCaseLower). **Contract tests** pin property sets so renames fail CI.

---

## 7. Domain practices (marketplace-adapted)

- **Listing status machine** guarded in domain/service (active → reserved → sold/given/exchanged; pause; disputed)  
- **Idempotency** on accept/complete/OTP where retries happen: e.g. `accept:{dealId}`, `complete:{dealId}:{userId}`  
- **Append-only** where money later appears (M-Pesa phase); for now deals/messages are auditable with timestamps  
- Freeze enums once shipped in a slice (condition, intent, deal status) — later slices don’t renumber  

---

## 8. Cross-module seams

Example:

| From → To | Seam |
|-----------|------|
| Deals → Notifications | `DealInterestCreated`, `DealAccepted` events |
| Listings → Deals | Listing must be `active` to arrange; accept reserves via Listings contract |
| Identity → all | `UserId`, phone verified flag — no module reaches into Identity tables |

**Publish, don’t call** across modules for side effects (notifications). Handlers in the consuming module.

---

## 9. Security

- Phone OTP + JWT; verified phone only after mutual accept on deals  
- Admin: permission policies (`listings.moderate`, `users.manage`, …) — not “role name in JWT is enough”  
- gitleaks / secret scan in CI  
- No secrets in repo; `.env` / user secrets locally  

---

## 10. Testing strategy

| Layer | What |
|-------|------|
| Unit | Validators, status transitions, wire-map / serialization pins |
| Integration | WebApplicationFactory + Testcontainers Postgres |
| Contract / SS0 | Skeleton tests for OpenAPI shapes; un-skip when endpoint lands |
| Runbooks | `docs/runbooks/` curl happy paths per slice |

**Done** = matches OpenAPI + tests + happy path exercised (not only compiles).

---

## 11. Validation & errors

- FluentValidation per request DTO  
- Services throw domain exceptions  
- `ExceptionMiddleware` → Problem envelope + status  
- Application layer stays HTTP-agnostic  

---

## 12. Documentation hierarchy

1. `docs/slices/sN-*-contract.md` — your slice  
2. `contract/openapi.yaml` — wire truth  
3. This file + ADRs under `docs/adr/`  
4. `CONTRIBUTING.md`  
5. `docs/runbooks/`  
6. `docs/design-system.md` (UI)  

---

## 13. Idempotency (Vitabu keys)

| Action | Example key |
|--------|-------------|
| Phone OTP verify | `otp_verify:{userId}:{codeHash}` window |
| Accept deal | `deal_accept:{dealId}` |
| Complete confirm | `deal_complete:{dealId}:{userId}` |
| Image upload | client idempotency key optional |

---

## 14. Before you code checklist

1. Read frozen OpenAPI / slice contract  
2. Check lane ownership  
3. Confirm SS0 stubs on main — redefine nothing  
4. Branch `feat/vitabu-sN-...` from main  
5. Implement only this slice’s scope  
6. Happy-path test + contract pin if new wire shape  
7. Update runbook if new API/event  
8. PR: summary, test plan, Resolves #…, keep small  

---

## 15. Adaptation notes (CEMES loans → Vitabu)

| CEMES loans | Vitabu |
|-------------|--------|
| Modules Loan/Payment/Customers | Identity/Catalog/Listings/Deals/Messaging/… |
| MSSQL + RLS multi-country | Postgres; single-country KE first; soft geo cities |
| Money ledgers early | Peer pay first; platform M-Pesa later slice |
| snake_case + FluentValidation + contract-first + trunk | **Same** |
| Full MediatR everywhere | Events for seams; fat services for in-module CRUD |

That is the backend operating system for Vitabu v4.
