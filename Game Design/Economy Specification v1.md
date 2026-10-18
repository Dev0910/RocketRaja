# Rocket Raja: economy specification v1

Current economy: [Faster-start Economy](Faster-start%20Economy.md). Its rules supersede the numerical values and completion targets below.

Updated 7 October 2026 to match the accepted Unity implementation. [Current Implementation Design](Current%20Implementation%20Design.md) defines the current presentation and supersedes earlier interaction drafts. The original simulation is historical, not current pacing validation.

This document supplies the previously missing numerical definitions. It is the current source for candidate values, alongside [Premium Economy v1](Premium%20Economy%20v1.md), the [priced catalogue](1000%20Unlockables%20Catalogue.md), and [simulation report](Economy%20Simulation%20Report%20v1.md). Earlier numerical drafts remain design history where they differ.

**Selected direction:** cash-only catalogue progression, India/INR first, and the **x6 minimum opening prestige bonus**. Buy any affordable cash entry not already owned this run. Product ownership, levels, family counts, prior upgrades and prestige counts do not gate cash purchases. The selected cash-only price/Fame coefficients remain in use.

## 1. Boundaries and units

Keep all eight product families, 1,000 distinct run-local cash purchases, the accepted 28 permanent cards, three skill identities, manual purchasing/Buy All, and normal offline income at 50% for eight hours. Talent opens at first prestige; first prestige grants exactly three free draw credits. Later draws use run progress plus different cash entries bought at least once. Fame remains the separate proposed permanent-income total.

Cash is fictional rupees, distinct from real INR IAP prices. Kaju Katli buys draws only. No daily/weekly objective, achievement currency, ad payout, temporary premium boost, or paid automation is needed by the baseline. Confirmed initial scope excludes rewarded ads and legacy ad-income features; any later introduction requires a separate design and balance pass.

Calculations retain fractional cash. The UI can abbreviate large numbers and expand them on inspection. This model uses JavaScript double precision for planning; eventual runtime large-number and boundary behavior needs implementation verification.

## 2. Starting state and product levels

Confirmed starting state: no product levels, tap level zero, no run upgrades, and enough cash for the current discounted first Phuljhari unit. Buy it manually. The starting grant is not eligible earned cash. On prestige, keep earned shop decorations while product displays rebuild as products are repurchased.

| Key | Product | Base first-level cost | Base normal income per level per second |
|---|---|---:|---:|
| PHU | Phuljhari | 30 | 0.850000 |
| ANA | Anar | 60 | 1.360000 |
| CHA | Chakri | 120 | 2.176000 |
| ROC | Rockets | 240 | 3.481600 |
| SMC | Small crackers | 480 | 5.570560 |
| STR | Cracker strings | 960 | 8.912896 |
| AER | Aerial shots | 1920 | 14.260634 |
| BOX | Celebration boxes | 3840 | 22.817014 |

The displayed incomes are rounded; the exact rule is `0.85 × 1.6^i` for zero-based family index i. The cost rule is `30 × 2^i`.

A later family becomes available when the previous family has at least one level. This is not an all-eight ownership gate on prestige.

For current level n and a manual bundle of q levels:

```text
level cost = C_i × 1.12^n × (1.12^q − 1)/0.12
             × (1 − 0.35 × purchased_PC_ranks/16)
             × (1 − 0.40 × product_cost_card_level/(product_cost_card_level+4))

milestone count(n) = indicator(n>=25) + indicator(n>=50) + floor(n/100)

normal product rate_i =
  (0.85 × 1.6^i) × n × 2^milestone_count(n)
  × product cash multipliers × applicable link/group multipliers
  × (1 + 0.20 × product_profit_card_level)
  × 1.10^owned_GP_count × prestige_multiplier(F)
  × (1 + 0.25 × global_profit_card_level)
```

Sum the eight rates to get normal production P. All permanent/global factors apply once. Offline income and Bulk Order inherit them through P without another application.

Recommend manual x1/x10/x100/Max quantity controls for both product and tap levels. Named-upgrade Buy All remains separate. No action occurs while away. The simulation selects finite bundles at explicit decision times, not continuous automatic buying.

## 3. Tap levels and payouts

Tap-level base cost is `40 × 1.12^n`; a q-level bundle uses the same geometric sum as products. Multiply by the remaining run discount `1 − 0.35 × TC/40` and the tap-cost card's remaining factor `1 − 0.40L/(L+4)`.

```text
B = 0.85 × (1 + 1.4n) × 1.20^j × 1.10^GP
    × prestige_multiplier(F) × (1 + 0.25 × global_card_level)
M = 1 + 0.25n/(n+100) + 0.25j/60
A = 0 if k=0; otherwise 0.12 + 0.18(k−1)/59
C = 1 + tap_profit_card_level/(tap_profit_card_level+5)

manual payout per tap = C × (B + P × A × M/2)
Helping Hands payout per tap = C × (B + 0.25 × P × A × M/2)
```

Here j is owned TD count and k is the highest purchased TL rank. A raw TD multiplier affects B only; its count also advances M. Every accepted manual gameplay tap earns full value, with no cooldown or per-second ceiling. The historical benchmark's two taps/second is a simulation input, not an input restriction. UI buttons never send gameplay taps.

## 4. All 1,000 cash effects

Every catalogue row includes a numerical base price, complete effect or cumulative target, and legacy prerequisite metadata that does not gate runtime purchases. IDs and counts remain unchanged.

| Type | Count | v1 numerical effect |
|---|---:|---|
| PP | 480 | Supporting purchases in band 1 multiply their family by 1.20; later supporting purchases by 1.60. First signature for a family gives x2; its nine later signatures each give x8. A signature replaces the supporting effect. |
| PC | 128 | Eight chains of 16: total run level discount becomes 0.35 × rank/16. |
| TD | 60 | Independent tap B x1.20; owned count adds its bounded mastery contribution. |
| TL | 60 | A becomes 0.12 + 0.18(rank−1)/59. |
| TC | 40 | Total run tap-level discount becomes 0.35 × rank/40. |
| GP | 80 | Normal products and independent tap B x1.10, once per entry. |
| UC | 32 | Total run named-upgrade discount becomes 0.35 × rank/32. |
| SK | 72 | The exact power/duration/cooldown values below. Includes three base unlocks. |
| XL | 32 | Each purchase multiplies its named beneficiary by 1.50. No source-level or family-ownership gate. |
| PF | 16 | Each purchase multiplies every listed family by 1.30. No family-level or ownership gate. |

Cumulative effects use the strongest purchased rank, including out-of-order purchases: PC/TC/UC use the largest discount; TL and skill power/duration/cooldown use the highest purchased rank. Lower ranks never downgrade these effects. In the cost formulas, PC/TC/UC refer to the highest purchased rank, not purchase count. Do not multiply historical discount targets together. Multiply remaining factors between independent card and run sources. Product/tap maximum combined discount approaches 61%; named-cash discount approaches 67.5%.

XL links are PHU→ANA→CHA→ROC→SMC→STR→AER→BOX→PHU. PF membership is explicit per row; the MIX partner changes across its four entries. No income recursively depends on another product's already-boosted income.

## 5. Base prices and cash-only catalogue access

For position j within a band containing m entries:

```text
opening_price[1] = 100
opening_price[2] = 3000000
opening_price[3] = 72000000
opening_price[b] = 72000000 × 31^(b−3)   for b=4..20

base price = round to 6 significant digits(opening_price[band] × 4^((j−1)/(m−1)))
charged price = base price × (1 − 0.35UC/32)
                × (1 − 0.50L_negotiator/(L_negotiator+4))
```

Every price key is concrete in the main catalogue. The table shows rounded authored first/last prices; derive each row from the exact opening formula before rounding. Prices never react to card luck or the player's wallet. New discounts affect future purchases, never refund earlier spending or discount their own already-paid cost.

| Band | Entries | First base price | Last base price |
|---|---:|---:|---:|
| 1 | 40 | 100 | 400 |
| 2 | 51 | 3000000 | 12000000 |
| 3 | 54 | 72000000 | 288000000 |
| 4 | 54 | 2232000000 | 8928000000 |
| 5 | 51 | 69192000000 | 276768000000 |
| 6 | 40 | 2144950000000 | 8579810000000 |
| 7 | 51 | 66493500000000 | 265974000000000 |
| 8 | 54 | 2061300000000000 | 8245200000000000 |
| 9 | 53 | 63900300000000000 | 255601000000000000 |
| 10 | 52 | 1980910000000000000 | 7923630000000000000 |
| 11 | 38 | 61408200000000000000 | 245633000000000000000 |
| 12 | 53 | 1.90365e+21 | 7.61461e+21 |
| 13 | 55 | 5.90132e+22 | 2.36053e+23 |
| 14 | 50 | 1.82941e+24 | 7.31764e+24 |
| 15 | 54 | 5.67117e+25 | 2.26847e+26 |
| 16 | 37 | 1.75806e+27 | 7.03225e+27 |
| 17 | 54 | 5.45e+28 | 2.18e+29 |
| 18 | 55 | 1.6895e+30 | 6.758e+30 |
| 19 | 46 | 5.23745e+31 | 2.09498e+32 |
| 20 | 58 | 1.62361e+33 | 6.49443e+33 |

**No catalogue entry has product, level, family-count, prior-upgrade or prestige-count requirements.** If the player can pay and does not own the entry this run, they can buy it. Prestige remains valuable through guaranteed income and free cards.

U counts different cash entries ever purchased and determines the lifetime draw bonus. N counts completed prestiges and still advances the next-run earnings requirement T; it does not unlock or lock catalogue sections.

This selection uses later price growth x31 and later Fame coefficient g=3 together. The previous x22/g=1/prestige-gate package is superseded. See the [decision record](Cash-only%20Economy%20Alternative%20v1.md) for the comparison.

Band 1 is a cheap introductory set; band 2 starts much higher. The sustained benchmark's longest gap between named cash purchases is about 13.7 minutes while product/tap investments remain available. That gap and the early reveal of all eight products are prototype review risks, not proven engagement wins.

## 6. Skills

Skill power entries are inside the 1,000 purchases and reset each run. Any purchased power rank unlocks the skill, including a higher rank bought first; duration/cooldown entries alone do not. Skill cards never unlock skills for free. Cooldown starts at activation. The first owned power rank starts ready; buying lower ranks afterward does not reset timing or reduce power. No active skill carries through prestige.

| Skill | Purchased power/payout ranks | Base duration | Base cooldown |
|---|---|---:|---:|
| Festival Rush | Product x2 / 2.25 / 2.5 / 2.75 / 3 / 3.25 / 3.5 / 4 | 30s | 300s |
| Helping Hands | 3 / 4 / 5 / 6 / 8 / 10 / 12 / 16 auto taps/s | 30s | 300s |
| Bulk Order | 120 / 135 / 150 / 165 / 180 / 195 / 210 / 225 / 240 / 260 / 280 / 300 seconds of P | Instant | 600s |

For timed skills, duration = `30 × (1+d/16) × (1+0.75L_duration/(L_duration+5))`; cooldown = `300 × (1−c/32) × (1−0.30L_cooldown/(L_cooldown+5))`. For Bulk Order, cooldown = `600 × (1−c/48) × (1−0.30L_cooldown/(L_cooldown+5))`, and payout = `P × purchased_seconds × (1+L_payout/(L_payout+5))`.

Rush also gives x1.25 to manual/Helping Hands payouts. Their P reference remains normal production, so the Rush product multiplier is not applied twice. Bulk Order uses normal P and does not receive Rush's temporary multipliers. Timed-skill uptime approaches, but does not reach, 50% at finite card levels.

## 7. Permanent cards

Retain all 28 identities/names in [Talent and Prestige Draft](Talent%20and%20Prestige%20Draft.md). The user confirmed 75% Common / 22% Rare / 3% Legendary odds, uniform within each tier. No pity or ownership/spending-dependent odds. The 100-Kaju-Katli draw cost, second-prestige gift, and INR pack design are confirmed in [Premium Economy v1](Premium%20Economy%20v1.md).

Every acquired copy adds one level immediately; no merge, extra cost, or finite cap. All owned cards apply to their appropriate systems automatically.

| Effect | Level-L rule |
|---|---|
| Product profit | 1 + 0.20L |
| Product/tap level discount | 0.40L/(L+4) |
| Tap or Bulk Order payout | 1 + L/(L+5) |
| Skill cooldown reduction | 0.30L/(L+5) |
| Skill duration | 1 + 0.75L/(L+5) |
| Global profit | 1 + 0.25L |
| Named-cash discount | 0.50L/(L+4) |
| Offline efficiency | 0.50 + 0.50L/(L+5) |
| Offline storage | 8 + 12L/(L+5) hours |

Show current→next effect with enough precision to reveal small changes; a rounded display must not imply that a duplicate had no effect. At extreme levels, the prototype must check numeric precision and benefit visibility. A level increase is promised; an exponential power increase is not.

## 8. Prestige requirement, guaranteed income, and draws

Let N be completed prestiges at the start of the run. Recommend:

```text
T = min(1e36, 1e10 × 1.5^N)
r = E / T                              (eligible when r>=1)
Fame gain = floor(sqrt(r))
F_after = F_before + Fame gain

income multiplier(F) = 1                         if F=0
                     = 6 + 3 × (sqrt(F) − 1)     if F>=1

first prestige draws = exactly 3
later prestige draws = 1 + floor(log_4(r))
                       + (floor(U/250) if r>=4, otherwise 0)
```

E includes legitimately earned product, tap, skill, and offline cash, even after spending it. It excludes seed cash, refunds, IAP currency, and card acquisition itself. U counts different catalogue IDs bought at least once, including this run. Rebuying, product/tap levels, and card levels never increment U.

T is fixed throughout a run. Cards or purchases never move the live requirement. Before reset, disclose both the current reward and next target `min(1e36,1.5T)`. The cap stops exponential requirement growth after the authored content horizon. It is a permanent target cap, not a daily reset limit or an extra reward cap.

The x6 opening coefficient is the user's selected direction. T, its growth/cap, square-root Fame, later income growth, and fourfold/lifetime thresholds remain prototype balance values. Draw odds and fixed premium draw cost are now confirmed. Fame is not spendable and never replaces U in the draw calculation.

Examples: F=1/4/9/16 gives income x6/x9/x12/x15. At U=500, later runs at T/4T/16T/64T give 1/4/5/6 draws and 1/2/4/8 Fame. First prestige still gives three draws even after a deeper push. Award rewards once; retain unused draw credits.

## 9. Offline and reset settlement

Use the departure normal product rate, current card efficiency/storage, and elapsed absence. Credit once on resumption, before the return summary or prestige preview. The baseline is `P × 0.50 × min(absence_seconds,28800)`. No purchases, taps, skill activations, or temporary skill payouts occur offline. Cooldowns can elapse. Live production starts at resumption with no overlapping credit.

Recommended reset: clear cash, product/tap levels, named run purchases, skill access, active effects, and run earnings; grant the manual opening cash and apply permanent gains. Retain cards, Fame, Kaju Katli, unused credits, lifetime unique purchases, completed prestiges, taught topics, and earned shop decorations. Product displays rebuild and earned decorations remain, as confirmed. The starting grant and manual first purchase are also confirmed; other detailed presentation behaviors remain prototype defaults.

## 10. Historical session policies and interpretation

These policies and results predate unrestricted upgrades, unlimited manual taps and Classic's discounted 5× pulls. They are retained as historical benchmarks, not current balance validation.

First-day mixed benchmark: visits at launch, +2h, +4h, +6h, +8h with 15/5/5/5/10 active minutes, totalling 40. This replaces the earlier 24-minute test proposal. The 15–25-minute user baseline applies after opening day; it did not prescribe a 24-minute opening.

Regular benchmark: 4/4/12-minute visits starting at day start, +8h, +14h. The overnight gap exceeds the base eight-hour cap; the model does not invent an overnight login. Every 20 online seconds, the policy budgets a management window and a tapping window. Purchasing and skill activation occur only in management windows. A reset reserves 40 online seconds for presentation/rebuilding setup.

The simulation assumes competent manual use of Buy All and quantity controls. It is not an implementation of auto-buy. Longer absences, reading time, misclicks, and human preferences can change results. See the report for variants and exact model limitations.

## 11. Implementation and release readiness

The Unity implementation is the accepted design as of 7 October 2026. See [Current Implementation Design](Current%20Implementation%20Design.md) for Classic browsing, immediate global Buy All, disabled notices and 1×/5× payment selection. Catalogue values and the formulas above remain in use.

Historical simulation timings require revalidation for the accepted access and input rules. Live billing, account-service acceptance, device coverage and player balance observation remain tracked in [Final Release TODO](Final%20Release%20TODO.md).
