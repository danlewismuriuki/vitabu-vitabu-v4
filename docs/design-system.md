# Vitabu Vitabu — Design System (v4 seed)

**Source of truth for UI.** Port into `vitabu-vitabu-v4/docs/design-system.md` and keep in sync with `design/tokens.css` + `design/tailwind.preset.js`.

Extracted from v3 `tailwind.config.js` + `src/index.css`. Do not invent a new purple/indigo theme.

---

## Brand

| Item | Value |
|------|--------|
| Name | Vitabu Vitabu |
| Tagline | Real Parents. Real Savings. Real Books. |
| Line | Built for Kenya. |
| Voice | Warm, practical, parent-to-parent; trust + savings + local |

---

## Typography

| Role | Family | Weights | CSS / Tailwind |
|------|--------|---------|----------------|
| Headings | **Poppins** | 400, 500, 600, 700 | `font-poppins` / `--font-heading` |
| Body | **Lato** | 300, 400, 500 | `font-lato` / `--font-body` |

Google Fonts (v3):  
`Poppins:wght@400;500;600;700` + `Lato:wght@300;400;500`

**Rules**
- `h1–h6` → Poppins  
- `body`, forms, paragraphs → Lato  
- Prefer clear hierarchy; avoid tiny gray text for primary CTAs  

---

## Color palettes (exact)

### Primary — brown (brand / headings / wordmark)

| Token | Hex | Use |
|-------|-----|-----|
| `primary-50` | `#F5F3F0` | Soft brand wash |
| `primary-100` | `#E8E3DC` | Light surfaces |
| `primary-500` | `#6D4C41` | Brand mid |
| `primary-600` | `#5C3E35` | Hover on brown UI |
| `primary-700` | `#4B3229` | **Default wordmark / strong text** |
| `primary-800` | `#3A261D` | Darker headings |
| `primary-900` | `#2A1B11` | Near-black brand |

### Secondary — green (success / verified / trust)

| Token | Hex | Use |
|-------|-----|-----|
| `secondary-50` | `#E8F5E8` | Success wash |
| `secondary-100` | `#C8E6C9` | Badge success bg |
| `secondary-500` | `#2C5F2D` | Success / verified |
| `secondary-600` | `#1B4332` | Strong success |
| `secondary-700` | `#081C15` | Success text on light |
| `secondary-800` | `#052E16` | — |
| `secondary-900` | `#031A0B` | — |

### Accent — orange (primary CTAs)

| Token | Hex | Use |
|-------|-----|-----|
| `accent-50` | `#FFF3E0` | Soft CTA wash / location chip |
| `accent-100` | `#FFE0B2` | Badge warning bg |
| `accent-500` | `#E57C23` | **Primary button** |
| `accent-600` | `#CC6900` | **Button hover** |
| `accent-700` | `#B25600` | Pressed / strong accent text |
| `accent-800` | `#994400` | — |
| `accent-900` | `#7F3300` | — |

### Gold — highlights / savings / special badges

| Token | Hex | Use |
|-------|-----|-----|
| `gold-50` | `#FEF7E0` | Soft highlight |
| `gold-100` | `#FAECC1` | Badge gold bg |
| `gold-500` | `#C88D36` | Gold accent / progress end |
| `gold-600` | `#B8802A` | — |
| `gold-700` | `#A8731E` | Gold badge text |
| `gold-800` | `#986612` | — |
| `gold-900` | `#885906` | — |

### Neutral — warm beige surfaces / text

| Token | Hex | Use |
|-------|-----|-----|
| `neutral-50` | `#F9F5F2` | **Page background** |
| `neutral-100` | `#F5E1DA` | Secondary button bg |
| `neutral-200` | `#E8DDD6` | Borders soft / progress track |
| `neutral-300` | `#D4C4B8` | Borders |
| `neutral-400` | `#C0AB9A` | Muted icons |
| `neutral-500` | `#A0877A` | Secondary text |
| `neutral-600` | `#806D60` | Body secondary |
| `neutral-700` | `#605346` | Strong secondary |
| `neutral-800` | `#40392C` | Near-body text |
| `neutral-900` | `#201F12` | Highest contrast text |

---

## Usage map (do this)

| UI element | Tokens |
|------------|--------|
| Page background | `neutral-50` |
| Card / panel | white + soft shadow |
| Site title / logo text | `primary-700` |
| Body text | `neutral-800` / `neutral-700` |
| Muted / helper | `neutral-500`–`600` |
| Primary CTA | `accent-500` bg, white text, hover `accent-600` |
| Secondary CTA | `neutral-100` bg, `primary-700` text, border `neutral-300` |
| Links (inline) | `accent-600` hover `accent-700` |
| Success / verified | `secondary-*` badges |
| Warning / urgent | `accent-100` + `accent-700` text |
| Savings / special | `gold-*` |
| Location chip | `accent-50` bg, `accent-200`-style border, `accent-600` icon |
| Focus ring | `accent-500` at reduced opacity |

## Do not

- Purple / indigo default AI themes  
- Flat pure `#FFFFFF` only pages with no warm neutral atmosphere  
- Neon glow, heavy multi-layer shadows as default  
- Emoji as primary UI decoration  
- Dark mode as default (not in v4 seed)  

---

## Atmosphere

- Optional `kitenge-pattern` background (accent orange at ~10% opacity SVG) — subtle, not loud  
- Prefer soft warmth over stark SaaS white  

---

## Component utilities (from v3)

| Class | Behavior |
|-------|----------|
| `.btn-primary` | accent-500 → hover accent-600, white, px-6 py-3, rounded-lg, shadow |
| `.btn-secondary` | neutral-100 → hover 200, primary-700, border neutral-300 |
| `.card` | white, rounded-xl, shadow-md, hover shadow-lg, p-6 |
| `.badge` | inline-flex, px-3 py-1, rounded-full, text-sm |
| `.badge-success` | secondary-100 / secondary-700 |
| `.badge-warning` | accent-100 / accent-700 |
| `.badge-gold` | gold-100 / gold-700 |
| `.progress-bar` | neutral-200 track, h-2, rounded-full |
| `.progress-fill` | gradient accent-500 → gold-500 |

### Intent badges (v4 product — map to tokens)

| Intent | Suggested style |
|--------|-----------------|
| For sale | `.badge-warning` or accent chip |
| Free | `.badge-success` |
| Exchange | `.badge-gold` or primary-100 / primary-700 |
| Donate (later) | secondary or distinct blue-avoid — use primary-100 |

### Motion

| Name | Role |
|------|------|
| `fade-in` | Page / section enter |
| `slide-up` | Modal-replacement page panels, toasts |
| `bounce-subtle` | Light attention (use sparingly) |
| `confetti` | Success moments only |

Parent app: **2–3 intentional motions**, not constant animation noise.

---

## Spacing & radius (practical)

| Token | Guidance |
|-------|----------|
| Radius buttons/cards | `rounded-lg` / `rounded-xl` (match v3) |
| Page padding | `px-4` mobile → `max-w-7xl mx-auto` desktop |
| Tap targets | Min ~44px height for primary actions |

---

## File mapping

| File | Role |
|------|------|
| [`design/tokens.css`](../design/tokens.css) | CSS variables |
| [`docs/brand.md`](brand.md) | Voice / tagline |
| v4 `design/tailwind.preset.js` | Tailwind theme extend (create in S0) |
| v3 `tailwind.config.js` | Historical source |

---

## UI conventions (pages)

- Auth, sell, arrange = **full pages**, not modals  
- Always support: loading, empty, error, success  
- Big clear CTAs in accent orange  
- Soft geo location control uses accent-50 chip pattern from v3 Header  
