# Rocket Raja: premium economy v1

1 October 2026. Confirmed INR-first initial design; no store products or purchases created.

India/INR as the first pricing market is confirmed. The user selected a fixed Kaju Katli price per draw and card draws as the only initial premium use. The user has now confirmed the 100-Kaju-Katli draw cost, 75/22/3 odds, second-prestige gift, and initial pack design below. Rewarded ads are excluded from the initial version.

## Draw price and reward pool

Use the confirmed **100 Kaju Katli per draw**, fixed throughout the account's life. Free prestige credits are separate entitlements, consumed through their own action; they do not become Kaju Katli. Paid and free draws use the same 28-card pool and confirmed 75/22/3 tier odds, with equal probability among identities within a tier. Show tier odds and individual-card odds before the player spends.

One paid draw always grants one card copy: an unowned identity becomes level 1, or a duplicate immediately adds exactly one level. No pity, escalating draw price, multi-copy level requirement, direct card purchase, or paid cooldown reset is introduced.

## Confirmed initial INR packs

| Pack | Kaju Katli | Whole draws at 100 each | Design price | Effective INR per draw |
|---|---:|---:|---:|---:|
| Small box | 1,000 | 10 | ₹99 | ₹9.90 |
| Festival box | 6,000 | 60 | ₹499 | ₹8.32 |
| Celebration box | 13,000 | 130 | ₹999 | ₹7.68 |
| Grand celebration box | 28,000 | 280 | ₹1,999 | ₹7.14 |

These are confirmed design price points. Availability on the target platform's price grid has not been verified, and no live offers have been created. Larger currency packs improve unit value; the actual single-draw price remains 100. Each pack is divisible into whole draws, avoiding an intentionally stranded remainder.

Do not advertise a guaranteed rarity, a specific card, or a guaranteed number of days saved. Store product IDs, localized prices, transaction restore/delivery, tax/fee treatment, and localized text belong to later implementation. No revenue/conversion forecast is asserted here.

## Initial free Kaju Katli supply

Grant the confirmed **100-Kaju-Katli introduction gift after the second completed prestige**, once per account. It supplies one optional currency-funded draw and teaches the wallet. It is deliberately outside the first-prestige reward, which remains exactly three free credits.

Do not require a purchase, ad, claim streak, or new daily objective for this gift. Persist eligibility and delivery; prevent duplicate grants on reopen/restore. Unspent currency survives reset. The model assumes the player elects to spend it once, and records that draw separately from prestige credits and IAP draws.

No recurring free Kaju Katli source or rewarded-ad grant is included initially, as confirmed. Achievements and daily/weekly rewards remain deferred; add their budgets later rather than silently assuming them now. Ordinary nonpaying collection growth comes mainly from prestige credits.

This makes the currency scarce without making the collection inaccessible. It also means currency scarcity is not the same as draw scarcity: the simulated regular account obtains hundreds of free credits over the content horizon.

## Paid value and limits

The [simulation report](Economy%20Simulation%20Report%20v1.md) compares 16 paired histories with packs spent at the first prestige. The same free-draw stream is shared between paired cases; paid draws use a separate independent stream. A player's first prestige is unaffected because Talent is locked until then.

In the selected cash-only model, 10/60/130/280 paid draws save a median 3/10/21/34 paired days respectively. These are policy-dependent sensitivities with wide variation; some paired cases finish later because changed affordability affects the simplified spending/reset policy. No purchased effect reduces its stat. The earlier zero-day small-pack median belonged to the superseded gated version. Use the current report, and do not promise a guaranteed shortcut or desired card.

Pack choice must remain optional. First prestige and the basic rebuild work without payment or a Legendary. The no-card-effect diagnostic is not a nonpaying account: real nonpaying players still receive and benefit from free cards.

## Purchase and draw flow

Show the currency amount and real-money price before purchase. A failed or cancelled payment grants nothing and consumes nothing; a verified successful transaction delivers its currency once, including after reconnect. Show the wallet before a currency-funded draw, its fixed cost, and the result. Never change the last-free-draw action into a paid action on the same button position without an explicit new choice.

No random-result-driven offer, escalating price, surprise extra payment, or sale countdown is part of this initial proposal. Retain readable effect previews for duplicates and unowned cards.

## Validation still requiring a prototype

Check whether players understand credits versus Kaju Katli, whether a Common duplicate is useful and legible, whether the smallest pack feels worth its price, and whether purchase/reveal time fits short visits. These are open player-experience questions, not gaps in the numerical definitions. Actual market performance cannot be established by this simulation.
