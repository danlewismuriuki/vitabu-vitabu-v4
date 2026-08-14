# Contributing — Vitabu Vitabu v4

## Branching
- Branch from up-to-date `main`: `feat/...`, `fix/...`, `chore/...`, `docs/...`
- Prefer short-lived branches; squash-merge
- Never push secrets

## Commits
Conventional Commits, e.g. `feat(identity): add phone OTP verify`, `feat(contract): register login me`.

## Slices
1. Update `contract/openapi.yaml` first (`operationId` on every op)
2. Implement API module (thin controllers / minimal APIs → services)
3. Wire `web/` and/or `admin/`
4. Tests + runbook for the happy path
5. PR with summary + test plan; keep diffs small (&lt; ~400 lines when practical)

Wire: **snake_case** JSON, Problem errors, FluentValidation (from S1).

Details: [docs/slice-playbook.md](docs/slice-playbook.md), [docs/backend-cemes-practices.md](docs/backend-cemes-practices.md).
