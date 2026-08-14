# S0 — Repo scaffold

## IS
- Monorepo layout (api, web, admin, contract, infra, design, data, docs)
- OpenAPI shell: Problem, Page, bearerAuth, `GET /health`
- C# host + Core exceptions middleware + empty modules
- Docker Compose: Postgres, MinIO (+ bucket init), Mailpit
- Design tokens + docs seed
- Next.js and admin shells with brand colors

## IS NOT
- Auth, listings, deals, MinIO upload wiring, EF migrations
- Production SMS / Resend

## Exit criteria
- [x] `dotnet build` succeeds
- [x] `docker compose -f infra/docker-compose.yml up -d` (Postgres on **5433**, MinIO, Mailpit)
- [x] `GET /health` returns `{ status: ok, service: vitabu-api, utc }` on **:5080**
- [x] Web shell builds (`web/` Next.js + brand tokens)
- [x] Admin shell installs (`admin/` Vite on :5174)
