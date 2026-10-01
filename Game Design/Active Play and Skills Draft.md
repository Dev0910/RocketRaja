# Rocket Raja: active play and skills draft

Updated: 1 October 2026. Planning only.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

The user selected worthwhile ordinary tapping throughout the run, with skills providing stronger bursts. Skills and all cash-purchased skill improvements must be bought again after prestige. Purchasing remains manual with Buy All. Offline earnings start at 50% of normal product income for eight hours, before Talent improvements. The regular nonpaying player's confirmed daily active-time budget after the opening day is 15–25 minutes, including purchases, menus, and rebuilds as well as tapping.

The formulas, tap rates, skill values, and prices are numerical recommendations. They now have coupled simulation results, including the first/rebuild and long-horizon checks; they are not approved launch balance or observed human behavior. No Unity code or assets are changed.

## Intended experience

Product growth keeps tap payouts relevant. Buying tap levels and cash tap upgrades should also improve active income, so a shop-income link does not erase the reason to invest in tapping. Skills create brief stronger opportunities while ordinary tapping remains useful between them.

Helping Hands adds automatic taps while manual taps remain productive. Festival Rush strengthens product income and gives a smaller, explicit tap bonus. Bulk Order grants a clear amount of ordinary shop production for an immediate purchase opportunity.

## Ordinary tap payout

Define:

- `P`: normal product income per second, including permanent and run product bonuses but excluding Festival Rush, sponsors, ads, offline payouts, and other temporary cash rewards.
- `B`: independent direct cash per tap, including tap levels, purchased direct-tap multipliers, the permanent prestige bonus, and applicable whole-shop bonuses. It excludes the Master Salesperson Talent card, which is applied later.
- `n`: current run tap level.
- `j`: number of purchased direct-tap profit entries, from 0 to 60 in the proposed catalog.
- `k`: number of purchased shop-link tap entries, from 0 to 60.
- `L`: Master Salesperson Talent level.

Candidate curves:

```text
reference manual tap rate = 2 taps/second
B = 0.85 × (1 + 1.4n) × 1.20^j × 1.10^GP
    × prestige_multiplier(F) × (1 + 0.25 × global_card_level)
tap mastery M = 1 + 0.25 × n/(n + 100) + 0.25 × j/60

shop-link share A = 0,                                    when k = 0
                 = 0.12 + 0.18 × (k - 1)/59,             when 1 <= k <= 60

Talent factor C = 1 + L/(L + 5)

ordinary cash per manual tap = C × (B + P × A × M / 2)
```

The division by 2 converts the linked share into a payout per tap at the reference cadence. The reference is a comparison rate, not a requirement to tap continuously. Before any shop-link purchase, direct tapping still works through B.

At the reference cadence, linked taps add `A × M × C` times normal product production. This is a ratio to product income, not the tap share of all earnings. If linked taps add 25% of product income and direct taps are negligible, they contribute 20% of the combined product-plus-tap cash.

Each direct-tap upgrade improves independent B and advances bounded mastery M; its raw multiplier does not multiply the linked portion. V1 specifies B above and tap-level base cost 40×1.12^n before category discounts. The model compares product/tap investments and no-tap policies; a positive marginal effect alone does not prove optimal purchase value.

The first shop-link purchase is proposed to require two owned families, replacing the earlier draft's four-family gate. This introduces the connection before the product ladder can leave ordinary tapping behind. Later links retain their previous tap-upgrade prerequisite. This timing remains a proposal, and no card is required for it.

Proposed manual input ceiling: allow one full-value tap per 1/3 second, with no accumulated allowance while idle. Helping Hands has its own rate. Menu touches never count as gameplay taps. The benchmark uses two taps/second during tapping windows and compares a no-tap policy. Device input, accessibility, and saturation feedback still need runtime/prototype review.

## Worked example

Consider P = ₹100,000/s, tap level n = 100, j = 30 direct upgrades, k = 30 shop-link upgrades, and no tap Talent card. M = 1.25 and A is approximately 0.208475.

- Each manual tap adds approximately ₹13,029.66 from the shop link, plus its independent direct payout B.
- Two taps per second add approximately ₹26,059.32/s from the link: 26.06% of normal product income, or 20.67% of combined product-plus-linked-tap income when B is negligible.
- With the draft level-5 tap card, C = 1.5, the linked addition becomes approximately ₹39,088.98/s at the same cadence.

These are arithmetic examples at an illustrative state, not a claim that this state occurs on a particular day or that users tap continuously.

## Skill rules and 72 cash entries

Retain the existing three skill identities as the working roster. The numerical lists below now supply first-pass values for the SP, SD, and SC entries in the cash catalogue; names and prices remain draft. Each first power entry unlocks its skill for that run. The three unlocks are included within the 72 entries.

| Skill | SP power/payout ranks, including the base unlock | Base duration | Base cooldown |
|---|---|---|---|
| Festival Rush | Product multipliers: ×2, ×2.25, ×2.5, ×2.75, ×3, ×3.25, ×3.5, ×4 | 30 seconds | 300 seconds |
| Helping Hands | Auto taps/s: 3, 4, 5, 6, 8, 10, 12, 16 | 30 seconds | 300 seconds |
| Bulk Order | Seconds of normal production paid: 120, 135, 150, 165, 180, 195, 210, 225, 240, 260, 280, 300 | Instant | 600 seconds |

Working cooldown convention: cooldown starts on activation. Skills are manually activated. Neither cash nor Talent upgrades automate purchases or skill activation.

For Festival Rush and Helping Hands, let d be the number of their duration entries bought (0–8) and c be their cooldown entries bought (0–8):

```text
run duration factor = 1 + 0.50 × d/8
run cooldown factor = 1 - 0.25 × c/8
```

For Bulk Order there are 12 cooldown entries; its run cooldown factor is `1 - 0.25 × c/12`. There are no duration entries for the instant skill.

Apply the accepted card categories using the provisional Talent formulas:

```text
duration = base duration × run duration factor × (1 + 0.75 × Lduration/(Lduration + 5))
cooldown = base cooldown × run cooldown factor × (1 - 0.30 × Lcooldown/(Lcooldown + 5))
```

Under these proposed budgets, a timed skill approaches a maximum duration of 78.75 seconds and a minimum cooldown of 157.5 seconds, with maximum theoretical uptime approaching 50%. Bulk Order's cooldown approaches 315 seconds. These are asymptotic limits as card levels increase; finite levels stay below the uptime limit. No hard combined clamp removes the benefit of a future duplicate.

### Festival Rush and the tap combo

Festival Rush multiplies P for live product earnings according to its purchased SP rank. The linked tap calculation continues to use unboosted P. Instead of applying the entire product multiplier again, Rush grants an explicit ×1.25 tap-payout factor to manual taps and Helping Hands while active. The 25% tap bonus is fixed in this candidate; SP ranks increase the product multiplier.

This provides a clear overlap opportunity: activate Helping Hands during Rush and optionally tap along. All three skill purchase/timing components remain independently visible. The ×1.25 interaction is a proposed design rule, not part of the current game or a claimed Tap Tycoon formula.

### Helping Hands

Auto taps receive the full independent direct payout and 25% of the shop-linked payout of a manual tap:

```text
cash per auto tap = C × (B + 0.25 × P × A × M / 2)
auto cash/second  = purchased auto taps/second × cash per auto tap
```

Festival Rush applies its additional ×1.25 factor while active. Manual taps continue alongside this income and keep their full payout. There is no shared manual/auto quota that could make manual taps worthless when Helping Hands runs. Every purchased auto-tap-rate step increases income.

At the worked example's state, eight auto taps/s add approximately ₹26,059.32/s from the link, plus eight direct payouts per second. Manual taps at two per second add another ₹26,059.32/s from the link. During Rush, these linked contributions together rise to approximately ₹65,148.31/s, separate from Rush-boosted product earnings and direct tap income.

The 25% automatic-link weighting is a balance input that should be described through the skill's actual cash/s display. Test whether players find the manual versus automatic payout difference understandable before adopting it. Compare a full-value auto-tap alternative with fewer auto taps if it communicates better.

### Bulk Order

```text
cash payout = P × purchased seconds of normal production
              × (1 + Lorder/(Lorder + 5))
```

Bulk Order uses unboosted normal P and its accepted payout card. It does not receive Rush's product multiplier or its tap bonus. A permanent profit upgrade purchased before Bulk Order changes P and therefore the payout; activating a temporary buff does not silently double it. Show the rupee payout before activation.

## Catalogue consequences

- Keep all 1,000 entries and the 120 tap-profit / 40 tap-cost split. Of the 120 profit entries, the draft uses 60 direct-TD and 60 link-TL steps.
- TD entries now explicitly improve B and contribute to bounded mastery M. This addresses the earlier audit's concern that a flat shop link could make direct-tap investment irrelevant.
- TL entries improve A. Their per-rank growth is deliberately separate from direct multipliers.
- Tap-level purchases improve B and M; their 40 cost-reduction unlockables improve purchase affordability. Their marginal return must be compared with product purchases at every price band.
- The skill catalogue remains 72 entries: 24 Rush, 24 Helping Hands, and 24 Bulk Order. Three are run-local base unlocks and 69 are improvements.

## Presentation and session rules

The [Upgrade Browser and Goals Draft](Upgrade%20Browser%20and%20Goals%20Draft.md) now proposes locked/ready/active/cooldown states, clear Rush/Hands overlap feedback, and a Bulk Order payout preview based on normal production. The [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) introduces these effects when relevant and proposes continuing production/cooldowns in menus. Skills do not activate or generate skill-specific payouts while away; ordinary offline income keeps its normal-product basis.

The [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md) proposes ending active effects on reset and starting repurchased skills ready. Skill repurchasing is confirmed; the ready-on-repurchase rule and detailed UI behavior remain proposals to evaluate with complete reset strategies. No flow or runtime behavior has been implemented.

## Verification and remaining work

Check one-tap, two-tap, and manual-cap activity; no tapping after products; short visits with one skill activation; long sessions combining skills; and the eight-hour offline path. Separate manual, automatic, normal-product, temporary-product, and instant-skill income in results.

The [working plan](Redesign%20Working%20Plan.md) proposes a twenty-minute regular day as two four-minute visits and one twelve-minute visit, with ten total minutes provisionally available for manual tapping. At two credited taps per second that is 1,200 manual taps. Do not model uninterrupted tapping for the full visit while treating buying, drawing cards, and rebuilding as instantaneous. Test the approved fifteen- and twenty-five-minute endpoints and different visit spacing; skill activations must fit the actual online windows and run ownership state. The first-day sustained-active benchmark is a separate policy.

At each stage examine actual cash share, purchase time saved, and marginal return of tap levels and each TD/TL step. Watch for a direct term B that grows too weak or too strong, linked-income dominance at high card levels, and a first-run gap before the first link purchase. The candidate tap formula does not itself solve every price or content pacing problem.

V1 now specifies cash costs, B growth, source-level gates, eligibility progression, and first-run reach. Arithmetic/uptime checks and full economy comparisons have run. Results depend on the selected cash-only numerical baseline and simplified spending policy; actual input handling, player comprehension, and IAP value remain untested. No Unity implementation is authorized.
