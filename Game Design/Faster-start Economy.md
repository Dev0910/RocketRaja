# Faster-start economy — implemented rules

Updated 15 October 2026. Economy **3.0.0**, content **faster-start-v1**, balance revision **tap-balance-2026-10-15**. This document supersedes numerical economy rules in the v1 drafts. There is no three-month or other total-completion deadline.

## Prices and upgrades

For zero-based catalogue position `i`, the undiscounted price is `20 × exp(0.36i + 0.00005i²)`. The 20 pages contain 50 entries each; crossing a page never changes the formula. All unowned cash upgrades remain purchasable without prerequisites. Buy All remains an immediate global cash batch.

The authoritative [catalogue](1000%20Unlockables%20Catalogue.md) contains exactly 1,000 upgrades:

| Effect | Count | Rule |
|---|---:|---|
| Product profit | 528 | 66 per family; each multiplies profit by ×2–×32 |
| Product level cost | 128 | 16 per family; remaining cost ×0.1 down to ×0.01 |
| Tap profit | 120 | Each multiplies direct tap profit by ×2–×32; no product-linked component |
| Tap level cost | 40 | Remaining cost ×0.1 down to ×0.01 |
| Global profit | 80 | Each multiplies product and tap profit by ×2–×6 |
| Upgrade Price Discount | 32 | 10%–50% off named cash upgrades |
| Skills | 72 | Existing power ranks, durations and cooldowns |

Profit multipliers stack once per purchased entry. For each cost chain, the **strongest purchased discount wins**, including out-of-order purchases. Permanent card discounts multiply with that remaining-cost factor. Buying a weaker discount adds a discovery but never worsens the price.

All 32 links and 16 collection/group upgrades have been replaced with 48 product-profit upgrades. All 80 signature identities and appearance transitions remain. The importer reads each row's exact `factor=` value; it does not substitute a separate signature or supporting-profit formula.

Product `p` starts with level cost `30 × 14^p` and income `0.85 × 2.5^p`. Product and tap level costs grow by **×1.12 per level**, using the geometric sum for bulk purchases. Tap training starts at 40 cash. The product base-cost ratio is calibrated alongside the opening; it is not a gap between cash-upgrade price bands.

Quantity controls start on **Max**, with ×1/×10/×100 still selectable. Stocking an unowned shelf (or starting tap training) buys its first level; later purchases use the selected quantity. This matches the primary simulation policy and makes the target pace accessible from the default controls.

## Milestones and synergy

Product levels and tap-training levels independently grant cumulative multipliers:

| Level reached | Additional profit |
|---:|---:|
| 10 | ×2 |
| 25 | ×2 |
| 50 | ×3 |
| 100 | ×4 |
| 200, 300, then every 100 | ×5 each |

Thus level 100 has a ×48 milestone multiplier and level 200 has ×240. Bulk buying applies every crossed milestone. Gameplay and previews share the same calculation, including large values beyond ordinary floating-point range.

When **all eight products** reach 50, 100, 200, 300, and each subsequent 100 levels, product income gains another ×2. This automatic synergy is derived from the lowest product level, so it cannot be collected twice. It never affects tapping. Product details show the next shared threshold, completed-product count, and products still below it. Individual milestones and synergy reset with product levels on prestige.

## Taps and skills

Direct tap payout is `2.6 × (1 + 1.4 × tap level) × tap milestones × purchased tap multipliers × global multiplier × tap Fame scale × tap card factor`. The global multiplier already contains product Fame income; the additional fixed Fame scale gives the separately authored tap Fame bonus. Rush still adds its existing separate ×1.25 tap multiplier. Every accepted manual tap pays full value; there is no per-second cap, product-income cap or diminishing-return rule. Every purchased ×2–×32 tap upgrade applies its full multiplier to both manual and Helping Hands payouts, including with cards.

Festival Rush and Helping Hands retain their base 30-second duration and 300-second cooldown. Bulk Order retains its 600-second cooldown. Power, duration and cooldown ranks/cards retain their established values. Helping Hands uses direct tap payout on its independent cadence; its former reduced product-linked component no longer exists.

## Prestige and persistence

The first target remains **6e21** earned cash. Later targets use the exponential backbone `3e23 × 100000^(FameStage−1)`, multiplied by `10^TargetOffsetLogs[Fame]`. The authored offsets place prestige eligibility around upgrade thresholds where tapping has a consistent measured benefit. At one Fame the target is approximately **1.5386e23**. Targets remain increasing with Fame and depend on **total Fame at the start of the run**, so pushing for additional Fame advances the next content stage instead of leaving an obsolete low target available for repeated farming.

Product Fame income is ×1 before prestige and ×6 at one Fame. Tap Fame income is product Fame income multiplied by `TapFameScales[Fame]`; at one Fame it is ×31.8. Both total bonuses increase with Fame. The Player and prestige screens label the product and tap bonuses separately. The product log-multipliers remain in `EconomyBalance.FameIncomeLogs`; the new tap scales and target offsets are exported in [TapBalanceCalibration.json](../Documentation/TapBalanceCalibration.json).

These values are fitted offline using the production engine and remain fixed at runtime. They never depend on a player's current tap speed, card collection, income ratio or elapsed play time. Beyond the authored table, the Fame stage grows logarithmically and the final product-income slope continues; the last tap scale and target offset remain fixed. The numerical stage saturates at 90,000 to stay within the existing Amount representation; this is not a tested endgame forecast.

Rewards retain `floor(sqrt(earned / target))` Fame. First prestige grants three free credits; subsequent rewards retain the earnings-based and lifetime-discovery card-credit bonuses. The second completed prestige retains its one-time 100 KK gift. Pushing beyond eligibility earns more Fame and, after the first prestige, more card credits.

Known older **local** economies start fresh once on this version transition. New progress then saves normally. Future/unknown versions, corrupt current saves and authoritative premium wallets are not silently overwritten. Cloud services and production billing remain disabled as before.

The 15 October tap calibration does **not** cause another reset. Existing 3.0.0 saves retain cash, cards, progress and their current run's original target. The revised target takes effect on their next prestige. State validation accepts the exact original or revised target, rather than accepting arbitrary target values.

## Currency and sources

Cash keeps ₹ and uses K, M, B, T, aa, ab … az, ba … zz, aaa. Each suffix step is ×1,000, following the [Tap Tycoon money convention](https://tap-tycoon.fandom.com/wiki/Money) and extending alphabetically beyond two letters.

Balance constants and milestone helpers: [EconomyBalance.cs](../Assets/Game/Core/EconomyBalance.cs). Human-readable entries: [1,000 upgrades](1000%20Unlockables%20Catalogue.md). Structured data: [RedesignContentCatalog.json](../Documentation/RedesignContentCatalog.json). Unity reads `redesign.upgrades` in `Assets/Resources/GameContent.asset`. Catalogue and Talent-roster text are hashed together; regenerate the asset, export and manifest after changing either source.

## Measured pacing and limits

Acceptance targets remain: all products in 5–10 minutes; first prestige in 20–30 minutes; second in 10–15 minutes; later typical runs in 15–30 minutes. Manual tapping should shorten comparable runs by 10–20% while products lead income. Typical later meaningful waits should stay within 1–3 minutes, with occasional longer breakthroughs.

See [the tap calibration report](../Documentation/TapBalanceReport.md) for current measured results and the explicitly tested card configurations. The [first rebalance report](../Documentation/FasterStartBalanceReport.md) records the previous coefficients. Simulation timings are not human-playtest observations. Level-100 card collections have 14 unresolved failures out of 15 stress comparisons; these remain balance work, not accepted exceptions. The [16 October diagnosis](../Documentation/Level100CardDiagnosis.md) separates card growth, Helping Hands and purchasing-policy effects. Full authored multipliers remain intact. The old 75/86/98-day cohort is historical and does not validate this economy.
