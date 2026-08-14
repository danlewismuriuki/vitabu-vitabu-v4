# Vitabu Vitabu — UI conventions

## Pages, not modals
Auth (`/login`, `/signup`, `/verify-phone`, `/forgot-password`), sell (`/sell`), and arrange (`/arrange/[listingId]`) are **full pages**. Small confirm dialogs only.

## States every screen supports
- **Loading** — skeleton or spinner; never blank forever  
- **Empty** — plain English + one CTA (list a book / show all Kenya)  
- **Error** — Problem `message` + field `errors` when present  
- **Success** — toast or inline confirmation; confetti sparingly  

## CTAs
- Primary: accent orange (`accent-500` / hover `accent-600`), white text  
- Secondary: `neutral-100` + `primary-700`  
- Tap targets ~44px min height on mobile  

## Trust & privacy in UI
- City on listings, never phone until after accept  
- Photo checklist on sell: book only; cover pupil PII; no real children in frame  
- Meetup: mandatory short safety checklist before confirm  

## Brand map
See [design-system.md](design-system.md). Page bg `neutral-50`, wordmark `primary-700`, fonts Poppins + Lato.
