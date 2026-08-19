# S8 — Messaging

## IS
- **1:1 thread** per listing between a parent and the seller
- Open thread from listing (phone-verified); idempotent if already open
- **Inbox** `GET /me/threads` + thread detail with messages
- **Send text** message; notify the other party in-app
- Web: `/messages`, `/messages/[threadId]`, Message seller CTA on book detail

## IS NOT
- Realtime / websockets (poll OK)
- Meetup place/time structured fields
- Block user / report message
- Group chats / media attachments

## Invariants
- Cannot message yourself (seller cannot open own listing thread)
- Only thread participants can read/send
- Body required, max 2000 chars
- Phone verified to open thread and send

## Exit criteria
- [x] Open / list / send / read work
- [x] Web inbox + thread
- [x] Tests + OpenAPI CI greps
