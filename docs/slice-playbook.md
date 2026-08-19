# Vitabu Vitabu — Slice playbook

How we ship features. Backend detail: [backend-cemes-practices.md](backend-cemes-practices.md).

## Golden rules
1. **Contract first** — change `contract/openapi.yaml` (`operationId`) before C# or UI.  
2. **Vertical slice** — one capability end-to-end; prefer PRs &lt; 400 lines.  
3. **Problem + snake_case** on the wire; FluentValidation; enums as strings.  
4. **Thin controllers → services**; domain events across modules.  
5. **Web pages not modals** for auth / sell / arrange.  
6. **Done** = matches OpenAPI + happy path + tests for that slice.

## Sequence
| Slice | Deliverable |
|-------|-------------|
| S0 | Repo, Docker, design, OpenAPI shell, `GET /health`, web/admin shells |
| S1 | Identity + phone OTP |
| S2 | Catalog + public listings / SEO |
| S3 | Listings write + MinIO photos |
| S4 | Deals arrange/interest + notifications |
| S5 | Complete / dispute / admin moderate |
| S6 | Wishlist (save listings) |
| S7 | donate_school intent + donated complete |
| S8 | Messaging (1:1 threads per listing) |
| S9 | School profiles (directory + donate `school_id`) |
| S10+ | M-Pesa, SMS/WhatsApp, Mtaani, wishlist alerts, growth, … |

## Per-slice steps
1. Update OpenAPI (+ optional `docs/slices/sN-*-contract.md`)  
2. SS0 stubs if parallel  
3. Implement module + infra  
4. Wire web and/or admin  
5. Unit/integration + runbook curl  
6. Small PR with test plan  

Branches: `feat/vitabu-sN-...`. Commits: Conventional (`feat(identity):`, `feat(contract):`).
