# Rocket Raja: collection balance study

Run: 1 October 2026. Planning study, version 0.1. No Unity changes.

The fixed-draw findings below retain their original scope. The later [full economy report](Economy%20Simulation%20Report%20v1.md) now models prices, reward supply, elapsed progression, and card effects together for selected cash-only progression.

This is an executed **draw-only collection simulation**, using the accepted 28-card roster and the **now-confirmed** 75% Common / 22% Rare / 3% Legendary probabilities from [Talent and Prestige Draft](Talent%20and%20Prestige%20Draft.md). It measures card discovery at fixed draw counts. It does not simulate cash progression, card effects on income, sessions, prestige frequency, Kaju Katli supply, or payments.

## Inputs and method

- 16 Common, 10 Rare, and 2 Legendary card identities. Cards are equally likely within their tier.
- Per-card probabilities: Common 4.6875%, Rare 2.2%, Legendary 1.5%.
- Independent draws from the same pool, with no pity, rarity guarantee, or ownership weighting.
- First copy means level 1; each duplicate adds one level. Ownership never disappears.
- 100,000 simulated histories, each followed through 360 draws. Checkpoints: 3, 30, 90, 180, 360.
- Reproducible pseudorandom generator: xorshift32, initial seed `0x72616a61`. The script below was executed as a function in JavaScript.
- Cross-check: compare simulated mean unique cards with the exact expected number. Use the exact formula for the chance of receiving no Legendary, particularly in the extreme tail.

For N draws, exact expected distinct cards:

```text
16 × (1 − (1 − 0.75/16)^N)
+ 10 × (1 − (1 − 0.22/10)^N)
+  2 × (1 − (1 − 0.03/2)^N)

Probability of no Legendary = 0.97^N
```

## Results

The unique-card percentiles are observed nearest-rank percentiles of the simulated histories. Collection completion means discovering all 28 identities, not reaching maximum power; no finite card-level cap is currently proposed.

| Cumulative draws | Simulated mean unique cards | Exact expected unique cards | Unique cards: 10th / 50th / 90th percentile | All 28 owned, simulated | No Legendary, exact |
|---|---:|---:|---|---:|---:|
| 3 | 2.881 | 2.880 | 2 / 3 / 3 | Impossible | 91.267% |
| 30 | 17.814 | 17.809 | 16 / 18 / 20 | 0 observed | 40.101% |
| 90 | 25.926 | 25.924 | 24 / 26 / 27 | 9.121% | 6.448% |
| 180 | 27.685 | 27.683 | 27 / 28 / 28 | 72.320% | 0.416% |
| 360 | 27.988 | 27.988 | 28 / 28 / 28 | 98.806% | 0.001729% |

Zero completions observed at 30 draws in this sample does not mean completion is mathematically impossible. At three draws, collecting 28 identities is impossible. Similarly, zero simulated no-Legendary histories at 360 draws is not a guarantee: the exact probability is about 0.001729%, shown above.

The simulated mean is within 0.006 cards of the exact mean at each checkpoint. Finite-sample completion percentages are estimates, not exact odds. The generator and seed make this planning run reproducible; they are not a proposal for the live game's random-number implementation.

**Draw counts are not day numbers.** For example, 90 draws does not mean day 90. The actual path to each checkpoint depends on free prestige rewards, run duration, lifetime catalogue progress, and any paid draws. None of those supplies is simulated here.

## What the first three draws are likely to feel like

Under the confirmed odds:

- At least one Legendary: **8.7327%**.
- No Legendary: **91.2673%**.
- All three cards Common: **42.1875%**; they can include duplicates.
- At least one duplicate identity among the three draws: **11.7816%**.
- Expected distinct cards: **2.880**, with every duplicate still granting its immediate level increase.

The first-prestige experience must communicate the guaranteed permanent income gain clearly even when all three cards are Common or a duplicate appears. The selected design has no pity or guaranteed rarity, so the reveal must not imply a Legendary is the normal opening result. These findings support the separate dependable-income panel and explicit before/after duplicate effects in [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md).

## Design implications

1. **Discovery and long-term power are different curves.** Around 180 draws, the median simulated history owns every identity, yet 27.68% of histories still lack at least one. At 360 draws, most histories have completed discovery. Long-term progress then relies mainly on useful duplicate levels and cash milestones, not constant new-card reveals.
2. **A desired card can remain missing after receiving another Legendary.** At these odds a specific Legendary has probability `0.985^N` of still being absent after N draws. Receiving either Legendary is not equivalent to receiving the desired one.
3. **The opening reward cannot depend on lucky collection effects.** The guaranteed prestige bonus must support a worthwhile rebuild before assuming a GlobalProfit card, a particular product card, or offline improvements.
4. **Free reward supply and effect curves need joint tuning.** Increasing later free draws can exhaust discovery sooner while increasing permanent power and speeding subsequent resets. A fixed per-draw price alone does not control that feedback.
5. **Collection completion is not the three-month target.** The user's horizon concerns major authored progression. No requirement has been selected that all players complete their collection at a particular date.

These are interpretations of this candidate distribution, not a request to reverse the user's no-pity decision. No card rarity, draw price, reward threshold, or effect value is approved by this study.

## What remains to model

- Per-card level distributions and resulting utility for each product/skill/offline play pattern, including unlucky and heavily duplicated histories.
- The combined effect of cash discounts and card discounts on purchase order and recovery time.
- Prestige target T and its progression, Fame/income candidates, and the current-run plus lifetime-catalogue draw calculation.
- Free draws earned over actual 15–25-minute daily schedules, rather than treating assumed daily draw averages as measured supply.
- Fixed Kaju Katli draw price, small free grants, IAP pack pricing, and paid versus unpaid progression. Deferred achievement/objective rewards must not be counted in the initial baseline.
- Repeated short resets versus deeper runs, including manually repurchasing skills and cash upgrades.
- First prestige, first rebuild, and roughly three-month cash-content targets under the full priced economy.

A full economy simulation and observed player sessions have **not** been performed for the redesign. This study reduces uncertainty about the proposed draw distribution only.

## Reproduction script

Wrap the following function body in a function and call it. It returns the simulated checkpoint statistics and exact checks. It creates no project files and uses no Unity runtime.

```javascript
const trials = 100000;
const checkpoints = [3, 30, 90, 180, 360];
let state = 0x72616a61;
function random() {
  state ^= state << 13; state ^= state >>> 17; state ^= state << 5;
  return (state >>> 0) / 4294967296;
}
const stats = checkpoints.map(n => ({
  draws: n, uniqueHistogram: Array(29).fill(0), sumUnique: 0,
  complete: 0, noLegendary: 0
}));
for (let trial = 0; trial < trials; trial++) {
  let ownedMask = 0, unique = 0, anyLegendary = false, checkpoint = 0;
  for (let draw = 1; draw <= checkpoints[checkpoints.length - 1]; draw++) {
    const u = random();
    const id = u < 0.75 ? Math.floor(u / 0.75 * 16)
      : u < 0.97 ? 16 + Math.floor((u - 0.75) / 0.22 * 10)
      : 26 + Math.floor((u - 0.97) / 0.03 * 2);
    const bit = 1 << id;
    if (!(ownedMask & bit)) { ownedMask |= bit; unique++; }
    if (id >= 26) anyLegendary = true;
    if (draw === checkpoints[checkpoint]) {
      const row = stats[checkpoint++];
      row.uniqueHistogram[unique]++;
      row.sumUnique += unique;
      if (unique === 28) row.complete++;
      if (!anyLegendary) row.noLegendary++;
    }
  }
}
function percentile(histogram, fraction) {
  let count = 0;
  for (let i = 0; i < histogram.length; i++) {
    count += histogram[i];
    if (count >= Math.ceil(trials * fraction)) return i;
  }
}
const results = stats.map(row => ({
  draws: row.draws,
  meanUnique: row.sumUnique / trials,
  exactExpectedUnique: 16 * (1 - (1 - 0.75 / 16) ** row.draws)
    + 10 * (1 - (1 - 0.22 / 10) ** row.draws)
    + 2 * (1 - (1 - 0.03 / 2) ** row.draws),
  p10Unique: percentile(row.uniqueHistogram, 0.10),
  medianUnique: percentile(row.uniqueHistogram, 0.50),
  p90Unique: percentile(row.uniqueHistogram, 0.90),
  fullCollectionPercent: row.complete / trials * 100,
  noLegendaryPercent: row.noLegendary / trials * 100,
  exactNoLegendaryPercent: 0.97 ** row.draws * 100
}));
return { trials, seedHex: "0x72616a61", results };
```
