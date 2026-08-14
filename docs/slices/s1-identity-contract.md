# S1 — Identity (Auth + phone OTP)

## IS
- Register, login, `GET /auth/me`
- Forgot / reset password (email via Mailpit locally)
- Phone request/verify OTP (mock SMS in Development; `dev_code` returned)
- JWT bearer auth; FluentValidation; Problem errors; snake_case
- EF Core + Postgres users table

## IS NOT
- Google OAuth, full email-verify gate enforcement on listings
- Production Africa’s Talking SMS
- Admin staff permissions matrix
- Refresh-token rotation (access token only for S1)

## Invariants
- One account per email (normalized)
- One account per verified phone
- OTP required before marketplace actions (enforced by later slices reading `phone_verified`)
- Browse remains public without auth

## Error codes (examples)
| error | HTTP |
|-------|------|
| validation_failed | 400 |
| invalid_credentials | 401 |
| email_taken | 409 |
| phone_taken | 409 |
| otp_invalid | 400 |
| otp_expired | 400 |
| reset_token_invalid | 400 |

## Exit criteria
- [x] Register → login → me
- [x] Request OTP → verify → `phone_verified: true`
- [x] Forgot / reset endpoints (email via Mailpit)
- [x] Unit/integration tests green locally (CI with Postgres service)
- [x] Web pages: `/signup`, `/login`, `/verify-phone`, `/forgot-password`, `/reset-password`
