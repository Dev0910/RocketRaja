# Rocket Raja: planning decisions and review v1

1 October 2026. Planning only.

This closes the numerical authoring pass and records what is selected versus recommended. Read [Economy Specification v1](Economy%20Specification%20v1.md), [Premium Economy v1](Premium%20Economy%20v1.md), and [Economy Simulation Report v1](Economy%20Simulation%20Report%20v1.md) as the current candidate. The executable model is preserved in [Economy Model v1](Economy%20Model%20v1.md).

## Confirmed decisions

- **India / INR first**, with other regions localized later.
- **Cash-only catalogue progression**, with no completed-prestige access requirements. Promote its price curve and later Fame growth into the main plan.
- **Stronger x6 opening income bonus.** Prioritize a roughly 3–4x faster first rebuild with comparable check-ins; accept faster sustained rebuilding. This supersedes the x3.5 opening candidate and the earlier expectation that sustained rebuilding also stay inside 3–4x.

Latest confirmation resolves the remaining discussion items:

- Keep earned shop decorations through prestige; product displays rebuild as products are repurchased.
- Start with enough cash for the current discounted first Phuljhari unit, purchased manually. The grant does not count as earned cash.
- Fixed draw cost: **100 Kaju Katli**. Odds: **75% Common / 22% Rare / 3% Legendary**, uniform within tier.
- Grant **100 Kaju Katli once after the second prestige**. First prestige still awards exactly three separate free draw credits.
- Initial INR packs: **1,000 / 6,000 / 13,000 / 28,000 KK for ₹99 / ₹499 / ₹999 / ₹1,999**.
- Exclude rewarded ads and ad-income features from the initial redesigned economy. Any later ad design is separate scope.

All earlier selected mechanics remain: 1,000 distinct cash entries; the 28-card roster and three tiers; every duplicate adds one level immediately; fixed-price draws; no pity; three first-prestige free credits; later draw rewards combine run progress and different cash upgrades bought at least once; manual purchasing and Buy All; skills rebought each run; 50%/eight-hour base offline income; same-shop evolution; one filtered Upgrades screen; roughly three months and 15–25 regular daily active minutes; first prestige later on opening day.

## Catalogue-access decision resolved

**Cash-only progression is confirmed.** Every cash entry is available when its individual cash, ownership, level, and prior-upgrade requirements are met. Completed prestige count never locks catalogue sections.

The main catalogue, specification, and model now use the same selected baseline: later price-band growth x31 and permanent income 6+3×(sqrt(F)−1), with x1 at zero Fame. Prestige remains valuable for Fame and free cards. U still counts different cash entries ever purchased for the lifetime draw bonus; N still advances the next-run earnings target.

The rerun of 32 free histories completes on days **75 / 86 / 98** at the sample 10th / median / 90th percentiles. The 86-day median depends on the stated play policy. Deep runs can finish sooner; actual play remains untested. The former prestige-milestone option is superseded; its comparison remains in the [decision record](Cash-only%20Economy%20Alternative%20v1.md).

## Confirmed values and prototype defaults

| Area | Recommended v1 default | Status |
|---|---|---|
| Guaranteed income | F=0 gives x1; F>=1 gives 6+3×(sqrt(F)−1) | Current cash-only numerical baseline; x6 opening selected. |
| Prestige eligibility | T=min(1e36,1e10×1.5^N), fixed within a run | Numerically tested candidate. |
| Fame | floor(sqrt(E/T)), retained, nonspendable | Candidate; separate from lifetime draw metric. |
| Later draws | 1+floor(log4(E/T)); add floor(U/250) when E>=4T | Candidate thresholds; confirmed hybrid structure. |
| Draw price | 100 Kaju Katli | Confirmed initial design. |
| Odds | 75% Common / 22% Rare / 3% Legendary, uniform within tier | Confirmed; no pity remains selected. |
| Currency gift | 100 Kaju Katli once after the second prestige | Confirmed; separate from the first-three-credit award. |
| INR packs | 1,000/6,000/13,000/28,000 KK for ₹99/499/999/1,999 | Confirmed initial pack design; platform price availability remains an implementation check. |
| Opening/rebuild | Grant the current cost of the first Phuljhari; buy it manually | Confirmed starting/rebuild rule. |
| Shop appearance | Permanent shop decorations; run-specific product displays rebuild | Confirmed retention/reset direction. |
| Visual milestones | Permanent U stages at 0/100/250/500/750/1000; product looks at ownership/signatures I/IV/X | Recommended presentation mapping. |
| Offline return | Automatically credit once, then show time/rate/cap/amount and a next action | Recommended flow. |
| Skill reset | End effects; a repurchased base skill starts ready | Recommended and included in the model. |
| Buy All | Explicit filtered scope; cheapest eligible sequence; review a fixed quoted list | Recommended flow; manual buying remains selected. |
| Menus | Production and cooldowns continue; menu touches do not count as taps | Recommended flow. |
| Legacy ads | Leave their payouts outside the redesigned launch economy | Confirmed initial scope; all numerical targets use no ads. |
| First-day test schedule | 15/5/5/5/10 minutes at 0/2/4/6/8 elapsed hours | Tested input, not a required player routine. |
| Names | Use the current 1,000 entry names and 28 functional card names for the prototype | Complete naming baseline; localization/editorial changes can follow. |

No additional premium spending use, daily objectives, card slots, automated buying, rarity guarantee, or multiplayer system is introduced.

## Experience review completed on paper

- Browser filters preserve one ownership identity; first purchases and rebuys are separate; all 1,000 entries remain inspectable.
- Buy All states its scope and cost, recalculates within its proposed sequence, and confirms a fixed list. It does not silently expand while passive income accrues.
- Prestige shows permanent income, free credits and their components, next requirement, reset/keep summary, and progress toward deeper cash upgrades; no prestige-count unlock is shown.
- Card reveals show immediate levels and actual effects. Free and paid actions are distinct, including after the last free credit.
- Offline credit settles once before a reset preview, uses normal production, and does not make purchases.
- Permanent decorations and run-specific product displays have separate meanings. Six shop looks and 32 owned product looks are scoped; no art has been produced.
- Tutorials are contextual and skippable. First-time teaching does not replay every prestige or withhold rewards.

This is a consistency review, not evidence of user comprehension or visual usability. Existing detailed flow drafts remain the references for interactions.

## Known tradeoffs that remain visible

1. The three-month midpoint assumes three daily visits at the selected twenty-minute midpoint. Less frequent/shorter play takes longer; deeper pushes and larger paid bundles can finish earlier.
2. All eight products arrive early in the reference. Later cash milestones carry the rest of first-day discovery.
3. The first-to-second price-band jump creates a longest named-upgrade gap of roughly 13.7 active minutes in the sustained reference. A prototype must establish whether product/tap goals make that interval engaging.
4. The smallest initial IAP pack mainly provides immediate collection gains; the selected cash-only model showed three median paired days saved, with wide variation and some slower policy outcomes. Do not advertise a guaranteed shortcut.
5. Finite simulated histories do not establish real retention, spending conversion, or the optimal strategy. Wider optimization and live player behavior can reveal issues.

## What is now complete and what requires a later phase

Complete as planning artifacts: all 1,000 numerical rows; product/tap growth; skill/card interaction rules; a complete prestige candidate; free/paid currency budget; play-pattern and card-history comparisons; executable reproduction source; proposed UI, visual, onboarding, and return flows; and this decision register.

No mandatory game-design discussion items remain from the latest review. Cash-only access, opening/visual retention, the initial premium economy, and exclusion of rewarded ads are confirmed. Other balance coefficients, exact visual thresholds, and detailed UX remain prototype defaults for testing; this does not assert that every coefficient was separately approved.

Still requiring a later, separately authorized implementation/prototype phase: actual screens/art, uncoached play observation, runtime large-number/purchase-boundary tests, platform transactions, and save migration. For existing saves, preserve the original schema/snapshot and entitlements; do not invent or execute an old-Legacy conversion during this planning task. Inspect actual deployed save/entitlement usage before specifying that migration.

No Unity code, assets, scenes, settings, packages, saves, or storefront products were modified by this work.
