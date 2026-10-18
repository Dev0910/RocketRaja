# Rocket Raja: cash-only economy simulation report v1

**Historical reference — 7 October 2026:** preserve the original rationale, scenarios and results below. They do not define the current interaction/access rules or establish current pacing. See [Current Implementation Design](Current%20Implementation%20Design.md) for the user-confirmed implementation.

Executed and selected 1 October 2026. Planning model, not Unity or human-player testing.

**Current scope:** the user chose cash-only progression. The [specification](Economy%20Specification%20v1.md), all 1,000 [catalogue rows](1000%20Unlockables%20Catalogue.md), and [model defaults](Economy%20Model%20v1.md) now agree: no prestige-count access requirements, late price-band ratio x31, and Fame income 6+3×(sqrt(F)−1) above zero Fame. The prior gated option is superseded; its results remain only in the [decision record](Cash-only%20Economy%20Alternative%20v1.md).

The rerun models product/tap investments, skills, permanent card effects, prestige, offline returns, management time, and unique-upgrade tracking together.

## Principal results

| Check | Cash-only result | Interpretation |
|---|---|---|
| First prestige, sustained | 2h 5m 40s; about 126 active minutes | Within the 2–3-hour target under the stated policy. |
| First prestige, mixed opening | Eight elapsed hours; about 30 active minutes used from a 40-minute first-day schedule | Within the 6–10-hour target for this visit pattern. |
| First-run cash frontier | 148 distinct entries mixed; 127 sustained | No prestige-count lock limits the frontier. |
| All eight families | Owned at 6m 20s in the reference | Available before first prestige, but discovery is concentrated early. |
| First rebuild, sustained, cards suppressed | 21m 20s; 5.89x faster to the original purchased frontier | Faster active recovery is accepted with the selected x6 opening. |
| First rebuild, matched visits, cards suppressed | About 2h 2m; 3.92x faster | Supports the check-in priority under controlled visit offsets. |
| Completion, 32 free histories | 10th / median / 90th: **75 / 86 / 98 days** | Around the three-month aim, not a universal completion promise. |
| Mean daily resets, days 31–90 | 10th / median / 90th: **1.23 / 1.38 / 1.53** | Within the typical 1–2-per-day target in this cohort. |
| Complete by day 90 | 23 of 32 histories (71.875%) | All 32 finish within the 150-day observation window. |

The sustained reference alternates management and tapping windows; it does not give simultaneous purchasing unlimited free attention. Rebuild diagnostics suppress card effects to isolate the guaranteed income benefit. Real players still receive the three promised cards.

Matched recovery replays first-day visit offsets relative to reset. Under the actual long-horizon schedule, seed 1009 instead recovers after **24 elapsed hours and about 14 active minutes**, because purchases wait for a real return. The controlled 3.92x result must not be displayed as a guaranteed next-return forecast.

## Reference history and free supply

Seed 1009; first-day visits at 0/2/4/6/8 elapsed hours with 15/5/5/5/10 active minutes. After opening, three visits at day start, +8h, +14h with 4/4/12 minutes: twenty active minutes/day including management.

| Day | Different cash upgrades | Completed prestiges | Cards drawn | Distinct cards |
|---|---:|---:|---:|---:|
| 7 | 250 | 12 | 31 | 21 |
| 30 | 479 | 54 | 124 | 27 |
| 60 | 758 | 97 | 253 | 28 |
| 90 | 1000 | 134 | 419 | 28 |
| 150 | 1000 | 196 | 726 | 28 |

The reference completes on day 90; the 32-history median is day 86. Do not confuse one seed with the cohort median.

Across the cohort, day-90 total draws are **373 / 438 / 531** at the 10th / median / 90th percentiles. Each includes one draw funded by the confirmed 100-Kaju-Katli second-prestige gift; subtract one for consumed prestige credits. No IAP, ads, achievements, or daily objectives supply this reference. This is measured candidate supply, not a fixed daily entitlement.

## Behavior and stress policies

Each row uses seed 1009. First-day policy is held constant unless tapping or purchase order changes explicitly. >150 means incomplete at the observation horizon, not impossible.

| Policy | Different cash entries at day 90 | Completion day | Mean resets/day, days 31–90 |
|---|---:|---:|---:|
| 15 minutes | 856 | 118 | 1.20 |
| 25 minutes | 1000 | 71 | 1.55 |
| One visit | 789 | >150 | 1.00 |
| Two visits | 848 | 119 | 1.18 |
| No tapping | 1000 | 79 | 1.50 |
| Levels first | 1000 | 75 | 1.38 |
| 4T resets | 1000 | 66 | 1.65 |
| 16T resets | 1000 | 57 | 1.97 |
| Common only | 844 | 123 | 1.13 |
| No card effects | 720 | >150 | 0.82 |
| Never prestige | 896 | >150 | 0.00 |

Cash-only permits deeper runs to advance earlier. The 4T and 16T examples finish at days 66 and 57; never prestiging reaches 896 entries at day 90 but remains incomplete at day 150. This supports a useful prestige loop without an access lock; it does not prove an optimal strategy or eliminate every exploit.

One-/two-visit and shorter-budget policies progress more slowly because attendance and the eight-hour base offline cap matter. The reference is not a ninety-day promise for every attendance pattern.

Some no-tap or alternative-purchase policies finish sooner than the default: spending and reset timing change. Tapping itself never subtracts income. These results expose the limits of the simplified purchasing policy; they are a reason to test how players compare investments and decide to reset.

Common-only cycles all draws among the sixteen Common identities and is an extreme utility test, not a luck percentile. No-card-effects suppresses earned effects for diagnosis; real nonpaying accounts receive cards.

## No-Legendary stress

Sixteen histories, seeds 1009×i for i=1..16, with Legendary outcomes removed and the remaining odds renormalized to Common 75/97 and Rare 22/97, completed on **100 / 113 / 127 days** at the sample 10th / median / 90th percentiles. All sixteen completed by day 150.

This deliberately altered distribution tests progress without either Legendary effect. It is not a percentile statement about actual 75/22/3 luck. Legendary ownership is not necessary for completion in these tested cases.

## Paid-draw sensitivity

Each pack has sixteen paired histories. Paid draws are opened at first prestige; the control shares the same free/gift random stream, while paid cards use a separate stream. First prestige cannot be shortened by these purchases because Talent is still locked.

The paired free control completes on days **74 / 90 / 101** at the sample 10th / median / 90th percentiles.

| Paid draws | Confirmed design INR price | Completion day: 10th / median / 90th | Median paired days saved |
|---|---:|---|---:|
| 10 | ₹99 | 72 / 82 / 95 | 3 |
| 60 | ₹499 | 71 / 79 / 86 | 10 |
| 130 | ₹999 | 65 / 69 / 83 | 21 |
| 280 | ₹1999 | 52 / 56 / 63 | 34 |

These are sensitivities to the confirmed initial packs, not guaranteed benefits. Individual paired cases can finish later under the fixed spending/reset policy despite stronger cards: changed affordability and timing can lead that nonoptimal policy down a slower path. Ten, sixty, and 130-draw scenarios include negative paired time savings; no purchased effect reduces its stat. Do not advertise a guaranteed card, rarity, or completion-time shortcut.

The smallest pack gives three median paired days saved in the selected cash-only cohort; the earlier zero-day result belonged to the rejected gated version. Perceived value, conversion, and actual spending behavior require player/market evidence. No revenue forecast or storefront price-tier claim is made.

## Reproduction and consistency checks

- The saved Markdown model defaults now reproduce cash-only without overrides. Its reference wrapper runs 150 days.
- Reran all 32 free histories, 16 paired histories for each of four packs, 11 behavior/stress policies, and 16 no-Legendary histories after promoting the selected parameters.
- Exactly 1,000 stable, unique IDs. Prices in the main catalogue, ID/price reference, and model match; all are positive and finite.
- No row or active model eligibility check requires completed prestiges. Product, level, and prior-upgrade requirements remain.
- Every explicit prerequisite exists and precedes its dependent row. Counts remain 80 signatures and 28 Talent identities.
- First prestige awards exactly three credits. In ordinary histories, acquired copies equal total card levels; every duplicate immediately adds one level.
- No purchases or resets occur while away. Seed cash is excluded from eligible earned cash; normal offline production is credited once.
- Use seeds 1009×i, i=1..32 for the free cohort, i=1..16 for paired/stress cohorts. Percentiles are observed nearest-rank sample values, not confidence bounds for real players.

These are model/data checks, not Unity integration tests. The simulator uses ten-second integration and twenty-second decision cadence, with forty active seconds reserved after reset. Runtime arithmetic, transaction boundaries, input, and saves still need implementation checks.

## Review risks

1. The spending/reset policy is simplified and nonoptimal; strategy differences are substantial.
2. The first-to-second price-band jump produces a roughly 13.7-active-minute maximum gap between named cash purchases in the sustained opening. Level investments remain possible, but engagement is untested.
3. All eight products arrive early. Later cash upgrades and skills must carry first-day discovery.
4. The routine assumes prompt manual purchases, skill use, and drawing credits. Reading, hoarding, missed visits, or errors can delay progress.
5. Many cash upgrades repeat multipliers. Numerical completion does not prove every purchase is interesting.
6. The selected curve is a reviewable prototype baseline, not validated retention, enjoyment, fairness perception, monetization appeal, or shipping balance.

The catalogue-access, opening/visual-retention, premium-economy, and initial no-rewarded-ad decisions are resolved. These confirmations match the existing simulation inputs, so no model parameters or results changed in this decision-record update. Other numerical/UX defaults remain in [Planning Decisions and Review v1](Planning%20Decisions%20and%20Review%20v1.md). Unity implementation, art, store integration, save migration, and uncoached play observation remain a later phase.
