# S8 — Messaging

Prereqs: API `:5080`; phone-verified JWTs.

```bash
# Open (or reuse) thread as interested parent
curl -X POST -H "Authorization: Bearer $BUYER" \
  http://localhost:5080/listings/<listingId>/threads

# Inbox
curl -H "Authorization: Bearer $BUYER" http://localhost:5080/me/threads

# Send
curl -X POST -H "Authorization: Bearer $BUYER" -H "Content-Type: application/json" \
  http://localhost:5080/threads/<threadId>/messages -d '{"body":"Hi, still available?"}'

# Thread
curl -H "Authorization: Bearer $SELLER" http://localhost:5080/threads/<threadId>
```

Web: `/messages` · `/messages/[threadId]` · Message seller on `/books/[id]`.
