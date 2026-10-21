# Rocket Raja: premium economy v1

Updated 7 October 2026. Confirmed INR-first pack design and current Classic 1×/5× pulls. See [Current Implementation Design](Current%20Implementation%20Design.md). Real production purchases remain unavailable pending release integration.

India/INR as the first pricing market is confirmed. The user selected a fixed Kaju Katli price per draw and card draws as the only initial premium use. The user has now confirmed the 100-Kaju-Katli draw cost, 75/22/3 odds, second-prestige gift, and initial pack design below. Rewarded ads are excluded from the initial version.

## Draw price and reward pool

Use the confirmed **100 Kaju Katli for a single draw**. Classic's 1× action uses one free credit when available, otherwise 100 KK. Its 5× action uses four free credits when available, otherwise 400 KK, with no mixed payment. Labels show the selected payment source before the action. Free credits remain separate entitlements and never convert to KK. Paid and free pulls use the same 28-card pool and 75/22/3 tier odds, uniform within tier. Tier and individual-card odds are available before spending.

Each draw result grants one copy: an unowned identity becomes level 1, or a duplicate immediately adds exactly one level. A 5× pull commits all five results and its payment atomically, then presents a persisted reveal queue. No pity, escalating draw price, multi-copy level requirement, direct card purchase or paid cooldown reset is introduced. Live account-funded batch authority remains release work; the local implementation does not establish that acceptance.

## Confirmed initial INR packs

| Pack | Kaju Katli | Single draws at 100 each | Design price | INR per single draw |
|---|---:|---:|---:|---:|
| Small box | 1,000 | 10 | ₹99 | ₹9.90 |
| Festival box | 6,000 | 60 | ₹499 | ₹8.32 |
| Celebration box | 13,000 | 130 | ₹999 | ₹7.68 |
| Grand celebration box | 28,000 | 280 | ₹1,999 | ₹7.14 |

These are confirmed design price points, not live offers. Larger packs improve unit value; a single draw remains 100 KK and a five-card pull costs 400 KK. Spending each pack using as many 5× pulls as possible and then singles yields 12 / 75 / 162 / 350 cards respectively. These are arithmetic totals assuming KK payment and no free credits, not extra pack grants or guarantees of rarity.

Do not advertise a guaranteed rarity, a specific card, or a guaranteed number of days saved. Store product IDs, localized prices, transaction restore/delivery, tax/fee treatment, and localized text belong to later implementation. No revenue/conversion forecast is asserted here.

## Initial free Kaju Katli supply

Grant the confirmed **100-Kaju-Katli introduction gift after the second completed prestige**, once per account. It supplies one optional currency-funded draw and teaches the wallet. It is deliberately outside the first-prestige reward, which remains exactly three free credits.

Do not require a purchase, ad, claim streak, or new daily objective for this gift. Persist eligibility and delivery; prevent duplicate grants on reopen/restore. Unspent currency survives reset. The model assumes the player elects to spend it once, and records that draw separately from prestige credits and IAP draws.

No recurring free Kaju Katli source or rewarded-ad grant is included initially, as confirmed. Achievements and daily/weekly rewards remain deferred; add their budgets later rather than silently assuming them now. Ordinary nonpaying collection growth comes mainly from prestige credits.

This makes the currency scarce without making the collection inaccessible. It also means currency scarcity is not the same as draw scarcity: the simulated regular account obtains hundreds of free credits over the content horizon.

## Historical paid-value simulation and limits

The results below used the earlier single-draw/access policy. They have not been rerun for unrestricted cash upgrades, unlimited manual tapping or discounted 5× pulls and must not be presented as current pacing forecasts.

The [simulation report](Economy%20Simulation%20Report%20v1.md) compares 16 paired histories with packs spent at the first prestige. The same free-draw stream is shared between paired cases; paid draws use a separate independent stream. A player's first prestige is unaffected because Talent is locked until then.

In the selected cash-only model, 10/60/130/280 paid draws save a median 3/10/21/34 paired days respectively. These are policy-dependent sensitivities with wide variation; some paired cases finish later because changed affordability affects the simplified spending/reset policy. No purchased effect reduces its stat. The earlier zero-day small-pack median belonged to the superseded gated version. Use the current report, and do not promise a guaranteed shortcut or desired card.

Pack choice must remain optional. First prestige and the basic rebuild work without payment or a Legendary. The no-card-effect diagnostic is not a nonpaying account: real nonpaying players still receive and benefit from free cards.

## Purchase and draw flow

Production currently disables pack purchases as unavailable. When real billing is integrated, show the actual store price and currency amount before purchase and fulfill a verified transaction once, including after reconnect. Classic keeps its 1× and 5× controls in place and updates their cost labels as credits are used. The next deliberate pull selects free credits if sufficient, otherwise the complete KK cost; no mixed payment or separate free/paid selector is required.

No random-result-driven offer, escalating price, surprise extra payment, or sale countdown is part of this initial proposal. Retain readable effect previews for duplicates and unowned cards.

## Validation still requiring a prototype

Check whether players understand credits versus Kaju Katli, whether a Common duplicate is useful and legible, whether the smallest pack feels worth its price, and whether purchase/reveal time fits short visits. These are open player-experience questions, not gaps in the numerical definitions. Actual market performance cannot be established by this simulation.
