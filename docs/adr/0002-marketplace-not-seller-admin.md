# ADR 0002 — Marketplace, not seller admin

## Status
Accepted

## Context
v3 blurred “become a seller” and modal-heavy flows. Parents need a simple peer marketplace, not a storefront back-office.

## Decision
- One Next.js app for browse + light “My listings” (`/sell`, `/my-listings`, `/messages`).
- Platform ops use separate `admin/` SPA.
- Primary flows are pages, not modals.
- Every parent account can buy and sell (no seller-wall role).

## Consequences
Keep seller UX thin. Do not port Firebase view-state shell or AuthFlow modals.
