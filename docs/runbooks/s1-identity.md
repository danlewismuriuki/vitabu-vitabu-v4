# S1 — Identity happy path

Prereqs: Postgres + Mailpit up (`docker compose -f infra/docker-compose.yml up -d`).

```bash
dotnet run --project api/src/Vitabu.Api --launch-profile http
```

## Register → me → phone OTP

```bash
# Register
curl -s -X POST http://localhost:5080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"display_name":"Amina","email":"amina@example.com","password":"Password1!","city":"Nairobi","accept_terms":true,"confirm_parent_guardian":true}'
```

Copy `access_token`, then:

```bash
TOKEN=...

curl -s http://localhost:5080/auth/me -H "Authorization: Bearer $TOKEN"

curl -s -X POST http://localhost:5080/auth/phone/request-otp \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"phone_e164":"+254712345678"}'
```

In Development the response includes `dev_code`. Verify:

```bash
curl -s -X POST http://localhost:5080/auth/phone/verify-otp \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"code":"123456"}'
```

Web pages: `/signup`, `/login`, `/verify-phone`, `/forgot-password`, `/reset-password`.
Mailpit UI: http://localhost:8025
