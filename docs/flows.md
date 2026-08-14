# Vitabu Vitabu — Core flows (S0 summary)

Full locked product map lives in the v4 plan. This is the working summary for implementers.

## Master journey
Browse → detail → (login + phone verify if acting) → arrange / message → seller accepts one → phones unlock → meetup or Pickup Mtaani → both confirm → optional rate.

## Auth
- Signup: display name, email, password, city; 18+/parent + Terms  
- Login: email + password → JWT  
- `/verify-phone`: SMS OTP before sell / message / accept  
- Progressive: browse free without phone  

## Sell
`/sell` multi-step page → CBC smart pick → condition → photos → intent (sale/free/exchange) → instant Active → `/my-listings`.

## Arrange (“checkout”)
No cart / platform M-Pesa in early slices. `/arrange/[listingId]`: interest → handoff choice → safety checklist (meetup) or Mtaani point → seller accept → reserve + phones.

## Interest vs reserve
Many verified users may request; listing stays Active; show “X interested”. Seller accepts **one** → reserved (72h timeout).

## Intents (first build)
`sale` | `free` | `exchange`. `donate_school` later; same handoff skeleton.
