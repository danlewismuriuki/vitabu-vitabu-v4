# Vitabu Vitabu v4

**Real Parents. Real Savings. Real Books.** — Built for Kenya.

Peer marketplace for CBC school books: **sale**, **free**, and **exchange** (school donate next). Parents use the public web app; platform staff use a separate admin app.

This repo is the greenfield rebuild (contract-first, CEMES-style C# API + Next.js + admin). v3 is reference only for brand/CBC ideas.

## Monorepo layout

| Path | Role |
|------|------|
| `contract/openapi.yaml` | API source of truth |
| `api/` | ASP.NET host + Core + Infrastructure + modules |
| `web/` | Next.js parent marketplace (SEO) |
| `admin/` | Vite React staff SPA |
| `infra/docker-compose.yml` | Postgres, MinIO, Mailpit |
| `design/` | Tokens + shared Tailwind preset |
| `data/cbc-book-catalog.json` | CBC seed stub |
| `docs/` | Product, design, slice playbook, runbooks |

## CI

GitHub Actions (`.github/workflows/ci.yml`) runs on every push/PR to `main`:

- API Release build
- Web `npm ci` + `next build`
- Admin `npm ci` + `vite build`
- OpenAPI shell checks (`Problem`, `bearerAuth`, `getHealth`)

Unit/integration tests land with S1+.

## Local ports

| Service | Port |
|---------|------|
| API | **5080** (8080 was occupied on this machine) |
| Web | 3000 |
| Admin | 5174 |
| Postgres | **5433** → container 5432 (host 5432 was occupied) |
| MinIO | 9000 / console 9001 |
| Mailpit | SMTP 1025 / UI 8025 |

## Prerequisites

- .NET SDK 10+ (projects target `net10.0`; plan originally said .NET 8 — same modular shape)
- Node.js 20+
- Docker Desktop

## Quick start

### 1. Infra

```bash
docker compose -f infra/docker-compose.yml up -d
```

| Service | URL / port |
|---------|------------|
| Postgres | `localhost:5433` (user/pass/db: `vitabu`) |
| MinIO API | `http://localhost:9000` |
| MinIO console | `http://localhost:9001` (`vitabu` / `vitabuminio`) |
| Mailpit UI | `http://localhost:8025` |

### 2. API

```bash
dotnet run --project api/src/Vitabu.Api --launch-profile http
```

Health: [http://localhost:5080/health](http://localhost:5080/health)

### 3. Web

```bash
cd web && npm install && npm run dev
```

[http://localhost:3000](http://localhost:3000)

### 4. Admin

```bash
cd admin && npm install && npm run dev -- --port 5174
```

[http://localhost:5174](http://localhost:5174)

## Slices

Ship **OpenAPI → API module → web/admin → tests**. See [docs/slice-playbook.md](docs/slice-playbook.md).

| Slice | Status |
|-------|--------|
| **S0** Scaffold + health | Done |
| **S1** Identity + phone OTP | Merged |
| **S2** Catalog + public listings | On `feat/vitabu-s2-catalog` |
| S2 Catalog + public listings | |
| S3 Sell + photos | |
| S4 Deals + notifications | |
| S5 Complete / dispute / admin | |

## Design

Warm neutrals, brown brand, orange CTAs, green trust. Poppins + Lato. See [docs/design-system.md](docs/design-system.md) and `design/tokens.css`.

## Contributing

Trunk-based, Conventional Commits, small PRs. See [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/backend-cemes-practices.md](docs/backend-cemes-practices.md).
