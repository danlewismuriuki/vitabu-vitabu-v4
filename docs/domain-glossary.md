# Vitabu Vitabu — Domain glossary

| Term | Meaning |
|------|---------|
| **User** | Parent/guardian account (18+). Can browse, list, arrange deals when phone-verified. |
| **Staff** | Platform operator using `admin/` with permission-based access. |
| **CbcTitle** | Canonical CBC catalog entry (grade + subject + term + material type). |
| **Listing** | One physical book a parent owns, linked to a CbcTitle (or custom title). |
| **Intent** | Why the listing exists: `sale`, `free`, `exchange`, later `donate_school`. |
| **Deal** | Shared pipeline: interest → accept → handoff → complete (one intent per deal). |
| **Interest / request** | Buyer enquiry while listing stays `active`; many allowed; UI shows “X interested”. |
| **Reserve** | Seller accepted one buyer; listing held; verified phones unlock. |
| **Handoff** | `meetup` or `pickup_mtaani` (curated points); peer pay for book price if sale. |
| **Condition** | Self-declared: `like_new`, `good`, `fair`, `writing_inside`. |
| **Phone unlock** | After mutual accept only — same verified E.164 number + `wa.me`. |
| **Soft geo** | Default near user’s city; Kenya-wide always allowed. |
| **Problem** | API error envelope: `{ error, message, errors? }`. |

Listing statuses (high level): `draft` → `active` → `reserved` → `sold` / `given` / `exchanged` (also `paused`, `hidden`, `disputed`).
