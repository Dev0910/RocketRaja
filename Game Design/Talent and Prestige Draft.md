# Rocket Raja: Talent and prestige numerical draft

**7 October 2026 — current design:** the user confirmed the Unity implementation as intended. [Current Implementation Design](Current%20Implementation%20Design.md) takes precedence over conflicting passages below. Unrestricted cash upgrades, unlimited manual taps, immediate global Buy All, Classic browsing without product filters/pins, disabled transient notices and 1×/5× pulls are accepted.

Updated: 1 October 2026. Planning only.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

The roster, three tiers, immediate one-level duplicates, fixed-price draws, no pity, guaranteed prestige income growth, and card-draw-only premium spending are confirmed. Three free draws at first prestige are confirmed. Later draw rewards combine current-run progress with lifetime progression. The user's latest choice makes different cash upgrades purchased at least once the lifetime measure, superseding the intervening Fame-based choice. Fame remains a separate proposed basis for permanent income. Current-run metrics, reward thresholds, qualification rules, and numerical coefficients remain proposals unless explicitly marked confirmed. No Unity changes or validated pacing results are implied.

## Card rules

- Proposed presentation: all owned cards apply automatically; no deck slots or manual merging.
- An unowned card has level 0 and no effect. The first copy creates level 1. Every subsequent copy immediately adds one level, with no additional cost.
- Cards, levels, and unused free draws survive prestige.
- Use the same full collection for free, paid, and introductory draws. No guaranteed rarity, streak protection, or ownership-dependent weighting.
- Confirmed probabilities: Common 75%, Rare 22%, Legendary 3%, then equal probability among identities in each tier. The fixed cost is 100 Kaju Katli per draw.
- Show the changed statistic after a draw, including the before/after value on duplicates. Card level, economic effect, and rarity are separate attributes.

## All 28 cards

IDs here identify draft design rows only; they are not Unity asset or save identifiers. Names are functional working names for discussion. Formula references are defined after the table.

| ID | Tier | Working name | Target | Draft level rule |
|---|---|---|---|---|
| C01 | Common | Phuljhari Showcase | Phuljhari normal profit | ProductProfit(L) |
| C02 | Common | Anar Showcase | Anar normal profit | ProductProfit(L) |
| C03 | Common | Chakri Showcase | Chakri normal profit | ProductProfit(L) |
| C04 | Common | Rocket Showcase | Rockets normal profit | ProductProfit(L) |
| C05 | Common | Small Cracker Showcase | Small crackers normal profit | ProductProfit(L) |
| C06 | Common | Cracker String Showcase | Cracker strings normal profit | ProductProfit(L) |
| C07 | Common | Aerial Showcase | Aerial shots normal profit | ProductProfit(L) |
| C08 | Common | Celebration Showcase | Celebration boxes normal profit | ProductProfit(L) |
| C09 | Common | Phuljhari Wholesale | Phuljhari level prices | LevelDiscount(L) |
| C10 | Common | Anar Wholesale | Anar level prices | LevelDiscount(L) |
| C11 | Common | Chakri Wholesale | Chakri level prices | LevelDiscount(L) |
| C12 | Common | Rocket Wholesale | Rockets level prices | LevelDiscount(L) |
| C13 | Common | Small Cracker Wholesale | Small crackers level prices | LevelDiscount(L) |
| C14 | Common | Cracker String Wholesale | Cracker strings level prices | LevelDiscount(L) |
| C15 | Common | Aerial Wholesale | Aerial shots level prices | LevelDiscount(L) |
| C16 | Common | Celebration Wholesale | Celebration boxes level prices | LevelDiscount(L) |
| R01 | Rare | Master Salesperson | Manual and skill-generated tap payout | TapProfit(L) |
| R02 | Rare | Practice Pays | Tap-level prices | LevelDiscount(L) |
| R03 | Rare | Festival Rhythm | Festival Rush cooldown | CooldownDiscount(L) |
| R04 | Rare | Helping Hands Rota | Helping Hands cooldown | CooldownDiscount(L) |
| R05 | Rare | Express Orders | Bulk Order cooldown | CooldownDiscount(L) |
| R06 | Rare | Festival Encore | Festival Rush duration | SkillDuration(L) |
| R07 | Rare | Extra Shift | Helping Hands duration | SkillDuration(L) |
| R08 | Rare | Celebration Contract | Bulk Order payout | OrderPayout(L) |
| R09 | Rare | Night Sales | Offline share of normal production | OfflineEfficiency(L) |
| R10 | Rare | Long Opening Hours | Maximum offline earning duration | OfflineHours(L) |
| L01 | Legendary | Festival Brand | Whole-shop product and direct-tap income | GlobalProfit(L) |
| L02 | Legendary | Master Negotiator | Eligible cash-unlockable prices | UnlockableDiscount(L) |

The roster has exactly 16 Common, 10 Rare, and 2 Legendary identities. A Legendary copy does not create a different rarity version of a Common card.

## Candidate level curves

Let `L` be copies owned, which equals the card level under the agreed rule. Multipliers equal 1 and discounts equal 0 at L = 0. All values are provisional.

| Rule | Formula | Level 1 | Level 5 | Level 10 |
|---|---|---:|---:|---:|
| ProductProfit | `1 + 0.20L` | ×1.20 | ×2.00 | ×3.00 |
| LevelDiscount | `0.40L / (L + 4)` | 8% off | 22.22% off | 28.57% off |
| TapProfit | `1 + L / (L + 5)` | ×1.167 | ×1.50 | ×1.667 |
| CooldownDiscount | `0.30L / (L + 5)` | 5% shorter | 15% shorter | 20% shorter |
| SkillDuration | `1 + 0.75L / (L + 5)` | ×1.125 | ×1.375 | ×1.50 |
| OrderPayout | `1 + L / (L + 5)` | ×1.167 | ×1.50 | ×1.667 |
| GlobalProfit | `1 + 0.25L` | ×1.25 | ×2.25 | ×3.50 |
| UnlockableDiscount | `0.50L / (L + 4)` | 10% off | 27.78% off | 35.71% off |

Profit cards add to their own multiplier as levels rise; ×1.20 at level 1 does not imply ×1.20 compounded for every copy. Bounded effects approach a limit gradually while every duplicate adds a level. No finite level cap is proposed in this draft. Check whether marginal improvements remain perceptible over the intended paid and unpaid draw ranges; mathematically positive but invisible gains are not enough.

A Rare card need not have a larger raw percentage than a Common card when it changes a different activity. Compare actual useful output under the intended play pattern. The broader GlobalProfit effect should not make the eight targeted profit cards feel irrelevant.

### Offline cards

Confirmed baseline: `eta0 = 0.50` of normal product income, stored for `H0 = 8` hours. The efficiency and storage card curves below remain numerical proposals; the user confirmed their effect categories and the base offline rule.

```text
OfflineEfficiency(L) = eta0 + (1 - eta0) × L / (L + 5)
OfflineHours(L)      = H0 + 12 × L / (L + 5) hours
offline cash        = normal product income/s
                      × OfflineEfficiency(efficiency card level)
                      × min(absence seconds, 3600 × OfflineHours(storage card level))
```

With the confirmed 50%/eight-hour baseline and the proposed card curves, efficiency is 58.33% / 75% / 83.33% at levels 1 / 5 / 10; storage is 10 / 14 / 16 hours. The proposed curves approach 100% efficiency and 20 hours. The eight-hour base is confirmed; the 12 additional hours approached by the storage card are not. The card levels are independent: owning one does not grant the other's effect. Ordinary offline production does not buy upgrades; the user chose manual purchases throughout. This normal-income basis excludes temporary skill and sponsor multipliers.

### Stacking and effect boundaries

1. Product-profit cards affect their own product. GlobalProfit and the guaranteed prestige bonus affect normal products and the independent direct-tap component. Offline income and Bulk Order derive their basis from normal product output, so they inherit these bonuses once.
2. The user's selected active-play direction is worthwhile ordinary tapping plus stronger skill bursts. The [Active Play and Skills Draft](Active%20Play%20and%20Skills%20Draft.md) supplies a candidate shop-income component and credited tap rules. That component already includes bonuses contained in shop income; do not multiply it by GlobalProfit or the prestige bonus again. TapProfit multiplies the resulting direct-plus-linked payout once, with its own contribution remaining below ×2 under this candidate curve.
3. Product and tap level discounts affect their named level prices. Master Negotiator affects eligible named cash unlockables, not product/tap level prices, Kaju Katli, or real-money prices. All three price categories should be distinguishable in the UI.
4. Multiply remaining price factors from permanent cards and cash-run discounts. Allocate their maximum reductions together. Avoid a hard combined cap that would make an additional duplicate have no effect.
5. Apply cooldown and duration cards to their named skill only. The active-play draft proposes cooldown starting on activation and allocates cash/card growth together to keep timed-skill uptime below 50% at finite card levels. That convention and numerical limit are not yet approved. Validate the complete combined curve; a cooldown floor alone cannot prevent continuous uptime if duration also grows.
6. Bulk Order is instant. Its card improves payout; it does not create a duration. Its basis remains normal shop production, so Festival Rush does not multiply it. The active-play draft instead proposes a clearly stated ×1.25 tap bonus during Rush, which can combine with Helping Hands and manual taps.

## Draft prestige reward rules

The confirmed reward direction is three first-prestige credits, then calculated free draws combining run earnings and different cash upgrades ever purchased (U). Rebuys do not increase U. Fame separately drives guaranteed income. The latest x6 opening choice prioritizes roughly 3–4x faster matched check-in rebuilding and accepts faster sustained recovery.

Let `T` be the run's prestige-eligible earnings target, fixed for that run, and `E` be eligible earnings accumulated during it. Spending cash does not reduce E. All eight product families being obtainable before the first prestige is now confirmed. The current baseline does not require owning all eight to prestige; availability does not imply mandatory ownership. Talent remains locked until the first completed prestige.

For an eligible reset, define `r = E / T`, with r >= 1:

```text
new Fame points = floor(sqrt(r))
F_before = lifetime Fame before this reset
F_after  = F_before + new Fame points

free draws = 3,                         on the first completed prestige
           = 1 + q + earnedCareerBonus, on later eligible prestiges (candidate)

q = floor(log base 4(r))
U = count of different cash unlockables ever purchased, including this run (0..1000)
careerBonus = floor(U / 250)             (candidate: 0..4)
earnedCareerBonus = 0,                   if 1 <= r < 4
                  = careerBonus,        if r >= 4

lifetime Fame = F_after, committed once when prestige completes

opening multiplier A = 6    (selected opening direction)
later growth g       = 3.0  (selected cash-only baseline)
prestige income multiplier = 1,                              if lifetime Fame = 0
                           = A + g × (sqrt(lifetime Fame)-1), if lifetime Fame >= 1
```

Brand Fame remains the working name for a persistent prestige total proposed to drive guaranteed permanent income growth. The user's latest choice removes Fame from the lifetime draw-bonus calculation. Its earning equation and income mapping remain proposals. A and g are independently tunable so a strong opening income reward does not require multiplying income by the same amount at every later reset. A is applied through the income multiplier formula once, not granted again as a stacking reset reward.

### How Fame is calculated

The proposed award is the square root of the ratio between eligible cash earned this run and this run's prestige requirement, rounded down to a whole point. The requirement T is fixed throughout that run. E is earned cash, including cash already spent on purchases, rather than the current wallet. Normal active and offline earnings can contribute under the eligibility rules below. A reset is unavailable when E < T, so no Fame is awarded for an ineligible reset.

| Run earnings relative to T | Fame earned on this reset |
|---|---:|
| 1T up to, but below, 4T | 1 |
| 4T up to, but below, 9T | 2 |
| 9T up to, but below, 16T | 3 |
| 16T up to, but below, 25T | 4 |
| 25T up to, but below, 36T | 5 |
| 100T up to, but below, 121T | 10 |

For an illustrative T of 1 million, earning 9 million gives floor(sqrt(9)) = 3 Fame. Earning 10 million also gives 3; the fourth point is reached at 16 million. Spending earnings does not reduce the award. If the account previously had 8 Fame, the preview shows 'Fame: 8 -> 11 (+3)' and the total becomes 11 when the reset completes. The fractional square-root remainder is not banked in this candidate; the preview should show the next whole-point threshold before reset.

When the currently quoted Fame gain is k, the next Fame point requires total run earnings of `(k + 1)^2 × T`. A player with a quoted +3 sees the threshold for +4 at 16T. This is distinct from the next draw threshold. Use thresholds directly when presenting milestones and comparing large values; do not allow rounding near a boundary to alter the intended reward.

Fame is granted once at prestige, not each time a threshold is crossed during the run. Run earnings restart after reset; accumulated Fame remains. Repeated and deeper eligible prestiges both contribute. A deeper run earning four times as much relative to its requirement doubles the underlying square-root reward before rounding. This moderates the increase in points at very high earnings, but does not prove that deeper runs offer better rewards per minute than short resets.

The selected cash-only baseline uses T=min(1e36,1e10×1.5^N), with N completed prestiges at run start. This is a numerically tested recommendation, not a confirmed coefficient. Keep T fixed during the run and show the next target before resetting.

| Eligible run earnings | New Fame | Later prestige draws (candidate; B = careerBonus) | First prestige total draws |
|---|---:|---:|---:|
| T | 1 | 1 | 3 |
| 4T | 2 | 2 + B | 3 |
| 16T | 4 | 3 + B | 3 |
| 64T | 8 | 4 + B | 3 |

At the minimum first prestige, income becomes x6 before cards. The selected cash-only baseline uses g=3: Fame 1/4/9/16 gives x6/x9/x12/x15. At F=4→5, income goes from x9 to about x9.708 (+7.87%). Resets add earned Fame and recalculate this formula; they never multiply the previous result by six again.

Three first-prestige draws are confirmed. Treat that as the total first-prestige award; the calculated ladder starts with the second prestige. There is no additional introductory bonus or first-run depth-draw award in this revised candidate. Later counts in the table remain provisional. The alternative five-first/two-later package is superseded by the user's request for a calculation based on progress.

Show the current payout and next draw/Fame milestone independently. They need not occur together: the next Fame point can arrive before the next draw. Grant the quoted rewards on reset once. Unused free draws persist; they do not automatically draw cards or convert into Kaju Katli. Introductory draws use the same random probabilities as later draws.

### Proposed progress measure and reward calculation

For the current-run part, recommend total eligible cash earned, E, measured against that run's prestige requirement, T. Spending money on levels, skills, or cash unlockables never reduces E. Cash carried in at the start, refunded purchases, and previously credited earnings are not counted again. First-pass normal earnings include products, manual/automatic taps, skill income, and legitimate offline production; rewarded ads and ad-income features are excluded from the initial redesign as confirmed. Permanent cards may raise earned income normally; buying a card or Kaju Katli does not itself add eligible cash.

Without a lifetime bonus, the candidate run ladder at T / 4T / 16T / 64T / 256T gives 1 / 2 / 3 / 4 / 5 draws. Once the run reaches 4T, add its available lifetime bonus to the quoted total. Count the total once at prestige, not the sum of each previously crossed reward threshold. E restarts after prestige; lifetime progress persists. The formula is evaluated only for an eligible reset, so no zero-earnings logarithm or negative draw result is needed.

For an illustrative later run with T = 1 million, U = 500, and E = 10 million, the run component gives two draws and the catalogue-progress bonus gives two: four draws in total. At 16 million, the run component becomes three, giving five total draws. Spending earnings does not lower E or either draw component. Fame is calculated separately for the proposed income bonus and does not enter this draw calculation. The sample target, fourfold spacing, and catalogue milestones remain unapproved.

The hybrid allows players at different lifetime catalogue stages to earn different draw totals for the same run-depth ratio, once they meet the proposed bonus qualification. Below 4T both receive only the run reward in this candidate. First prestige remains exactly three draws. A new first-time cash purchase updates the persistent catalogue record and the draw preview during the run; a milestone does not grant an immediate draw payout. The quoted draw total is awarded once when prestige completes.

### Lifetime component: different cash upgrades purchased

Confirmed by the user's latest explicit choice: lifetime progression for draw rewards counts different cash upgrades purchased at least once. This supersedes the immediately preceding Fame-based lifetime reward choice. The first-time purchase record persists across prestige; buying the same entry again does not count twice.

Count purchases from the existing 1,000-entry cash catalogue. Seeing an entry, gaining product or tap levels, drawing Talent cards, buying Kaju Katli, and completing a prestige do not themselves add to this count. Base skill purchases count because those three entries are already included in the 1,000. Each catalogue ID contributes at most one, including entries first bought during the current run.

The metric is confirmed. The following milestone sizes and bonus amounts remain proposals:

| Different cash upgrades ever purchased | Candidate lifetime draw bonus |
|---|---:|
| 0–249 | +0 |
| 250–499 | +1 |
| 500–749 | +2 |
| 750–999 | +3 |
| 1,000 | +4 |

These milestones add no new catalogue purchase, Talent card, or spending currency. Reaching a milestone makes its bonus available on future qualified prestiges as well as the pending one; another new catalogue milestone is not required for every payout. Once earned, the catalogue progress survives reset.

The 4T qualification is a separate, unapproved proposal: the lifetime bonus contributes only when current-run earnings reach four times that run's prestige requirement. It prevents minimum-depth resets from automatically collecting the full catalogue bonus, but does not prove that repeated 4T runs are balanced. The bonus is awarded once per qualifying prestige, not at every deeper earnings milestone.

With the current 1,000-entry scope, the proposed catalogue component ends at +4; the run-depth component can continue growing. This bonus ceiling is a formula candidate, not a user-approved cap on total draws or card levels. Any later catalogue expansion requires explicit reward redesign.

### Combined reward examples and preview

All following totals are for later eligible prestiges and are provisional.

| Different cash upgrades ever purchased | At T | At 4T | At 16T | At 64T |
|---|---:|---:|---:|---:|
| 0–249 | 1 | 2 | 3 | 4 |
| 250–499 | 1 | 3 | 4 | 5 |
| 500–749 | 1 | 4 | 5 | 6 |
| 750–999 | 1 | 5 | 6 | 7 |
| 1,000 | 1 | 6 | 7 | 8 |

A first-time purchase during the run can move the player into a later row. Rebuying a previously purchased entry cannot.

The prestige preview should explain the parts: 'Run progress: 2 draws. Lifetime upgrade bonus: +2. Receive: 4 draws.' Separately show the proposed Fame gain and resulting permanent income. Before reaching 4T, show the available catalogue bonus and the run earnings needed to activate it. A lifetime tracker can show '500 / 750 different cash upgrades purchased' for the next milestone. These are draft labels, not a screen implementation.

At run-depth index q, the next run-draw rung is `T × 4^(q + 1)`. Reaching 4T may raise the payout by more than one because the catalogue bonus becomes available. Crossing a first-purchase milestone can also increase the payout if the run qualifies. Preview both routes separately; the combined draw total does not determine the next run-earnings threshold. Fame milestones affect the proposed income calculation only. First prestige shows its fixed three draws.

There is no fixed daily draw entitlement. At a given reset cadence, free supply depends on E/T and the persistent count of different cash purchases. Projections must follow the actual sequence of earnings, first-time discoveries, and completed resets. They must not grant the final catalogue bonus from day one. Fame can influence earnings through the separate proposed income multiplier, but it does not directly add a lifetime draw bonus.

### Short-reset comparison remains necessary

A logarithmic reward curve does not by itself prevent shallow reset farming. With U held at 500 after first prestige, four later minimum-depth resets each at r = 1 award four draws and four Fame in total. One run at r = 4 awards two Fame and four draws under the hybrid. Equal draw counts are not equal efficiency: these are relative-depth comparisons, not equal-time or necessarily equal-earnings routes. Rebuilding, production growth, Fame gains, catalogue discoveries, and future targets affect outcomes. Compare draws per elapsed day and per active minute from the same starting state, including minimum resets, repeat 4T runs, and deeper pushes.

Keep T fixed during a run and disclose its next-run change. The coupled model now compares minimum-depth, 4T, 16T, no-reset, and paid policies. Deeper runs finish sooner in the tested cases; these finite comparisons do not establish an optimal strategy or rule out every exploit.

### Measuring the first rebuild

The latest selected direction prioritizes roughly 3–4x faster check-in recovery with an x6 minimum opening bonus and accepts faster sustained recovery. Compare reacquisition of the original cash IDs and all eight families; merely matching displayed income is a different checkpoint.

Use the same purchase policy, management budget, skill rules, and visit offsets for the first/rebuild comparison. The selected cash-only candidate measures about 3.92x faster matched check-ins and 5.89x faster sustained recovery with card effects suppressed. These are model outputs, not forecasts for an individual player.

The earlier x3.5 coefficient is superseded. Purchase choices, cooldowns, manual interactions, offline caps, and return times prevent equating an income multiplier with an identical speedup. A player returning after eight hours must wait for that return to buy; never insert an extra visit or offline purchases to meet a target.

For the guaranteed-benefit check, first compare with card effects suppressed, then examine actual random introductory draws, including weak immediate combinations. This is a diagnostic model case, not withholding earned cards from players. The dependable first-reset benefit should not require a useful rarity or a purchase. Strong card outcomes may exceed the target range; do not reduce their effects to enforce the range on every player.

Reaching the old frontier is separate from reaching the next prestige threshold. Tune future T progression and deeper content against the confirmed 1–2 typical daily resets and 15–25 active-minute budget. A faster first rebuild does not select a faster daily reset target. Later recovery behavior still needs testing alongside long-term Fame and card growth.

### Eligibility and full-economy validation

The run target, 1,000 prices/effects, and reward formulas are now supplied in v1. The cash-only baseline uses T=min(1e36,1e10×1.5^N), exclude seed cash from E, and propose ready-to-use repurchased skills. The target is fixed within the run; the 1–2-reset target is a cadence, not a daily cap.

The [simulation report](Economy%20Simulation%20Report%20v1.md) includes ordinary, deep-run, lower-activity, paid, Common-only, and no-Legendary cases for the selected cash-only baseline. The former prestige-count access option is superseded; all current prices use the stronger late curve and g=3 Fame growth together.

V1 confirms one 100-Kaju-Katli grant after the second prestige and excludes rewarded ads from the initial economy. Account settlement, live numeric boundaries, and migration remain implementation work; no deployed save is changed by this plan.

## Reset experience

Confirmed: players must buy the skills themselves again each run. Prestige resets base skill access and all cash-purchased skill improvements. The 72 skill entries comprise three base skill-unlock purchases and 69 later improvements, all available to repurchase in subsequent runs. Permanent skill cards remain owned but do not grant or activate a locked skill; their effects apply when the relevant skill is bought again. This preserves the 1,000-entry count without adding three hidden extra unlocks.

Cash, product levels, tap levels, and run improvements reset under the working proposal. Talent levels, permanent income growth, Kaju Katli, unused draws, and lifetime records persist. The [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md) now records the confirmed start with no owned products and a cash grant equal to the currently discounted first Phuljhari unit cost, then highlighting that manual purchase. The grant is excluded from eligible earnings. Active skill effects end at prestige; newly repurchased skills starting ready is a candidate to test against reset farming. The grant, manual purchase, and retention of decorations with rebuilt product displays are confirmed; skill-ready timing remains a prototype default. Buy All remains player-triggered and pays current cash prices.

The [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) proposes crediting offline earnings once on resume before showing its summary or offering a reset. Menus and absences have explicit production/cooldown rules there. Ad rewards are excluded from the initial version. Account/migration handling remains later implementation work.

## What has and has not been checked

The roster count, formula endpoints, example levels, and reward-table arithmetic have been checked. The [Collection Balance Study](Collection%20Balance%20Study.md) also records an executed 100,000-history discovery simulation using the now-confirmed 75/22/3 probabilities, with exact expected-ownership and no-Legendary probability checks. Its checkpoints are cumulative draw counts, not days. It does not apply the proposed level effects to income or simulate reward supply.

The later coupled model now measures first prestige, rebuilds, daily resets, long-term catalogue reach, and paid sensitivity using the priced catalogue and randomized cards. Its results are conditional on the selected numerical baseline and its player policy. They do not establish real retention, comprehension, spending value, or shipping readiness; human observation and runtime checks remain later work.

Values explicitly marked confirmed, including three first-prestige draws, are user decisions. Other coefficients and reward thresholds remain provisional numerical design inputs. No gameplay code, Unity assets, saved economy, or project settings are modified.
