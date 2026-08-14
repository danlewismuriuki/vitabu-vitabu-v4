# ADR 0001 — Auth and RBAC

## Status
Accepted (product locked)

## Context
Parents need accounts to list and arrange deals. Staff need a separate admin surface. Kenya meetups need a trusted phone.

## Decision
- Parent auth: email + password JWT; phone SMS OTP before sell / message / accept.
- Progressive: browse without phone; gate marketplace actions on `phone_verified_at`.
- Admin: separate app; permission policies fail closed (`listings.moderate`, etc.).
- No Firebase Auth in v4.

## Consequences
Identity module owns register/login/OTP. Other modules consume `user_id` + verified flags via contracts, not Identity tables.
