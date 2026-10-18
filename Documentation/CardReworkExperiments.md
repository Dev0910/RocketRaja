# Card and Helping Hands redesign experiments — 16 October 2026

**Experimental, not active Unity balance.** The user selected independent tapping, revised card growth and a separate Helping Hands base payout. They then selected a target of approximately **×2 product-card profit at level 100**, rejecting the weaker approximately ×1.24 option as the direction to pursue.

Every cash upgrade retains its authored multiplier. There is no product-linked manual payout, payout cap or adjustment based on the player's income composition. The experiments run the production economy code with explicit `REDESIGN_LAB` parameters; Unity does not compile those overrides. Existing card levels, player saves, content assets and runtime balance remain unchanged until a complete candidate is validated.

## Selected direction

The `balanced` prototype uses `r = L / (L + 5)` for card level `L`:

| Card effect | Experimental formula | Level 1 | Level 100 |
|---|---|---:|---:|
| Product profit | `1 + r` | ×1.1667 | ×1.9524 |
| Tap profit | `1 + r` | ×1.1667 | ×1.9524 |
| Global profit | `1 + 2r` | ×1.3333 | ×2.9048 |
| Product/tap remaining level cost | `1 − 0.25r` | ×0.9583 | ×0.7619 |
| Remaining cash-upgrade cost | `1 − 0.25r` | ×0.9583 | ×0.7619 |
| Skill cooldown | `1 − 0.2r` | ×0.9667 | ×0.8095 |
| Skill duration | `1 + 0.4r` | ×1.0667 | ×1.3810 |

Bulk Order payout and offline cards retain their existing formulas. The exact helper uses the same safe bounded-ratio calculation as the current game. Card levels remain unlimited; the experimental effects approach their asymptotes.

The primary Helping Hands candidate pays **50% of the manual tap base per automatic tap**. Its authored taps/second, base duration, base cooldown and skill ranks remain intact. Manual and automatic payout each still receive the full purchased tap upgrade multiplier: a ×32 purchase changes `T` to `32T` manually and `0.5T` to `16T` automatically, a ×32 increase in both cases. Rush applies its existing tap multiplier to both.

At maximum cash skill ranks and level-100 timing cards, this prototype produces about **2.73 manual-tap-equivalents/second** averaged over the nominal Helping Hands cycle, versus 7.68 in the previous level-100 configuration. Actual simulations include activation delays and skill overlap.

The separate `--card-global-gain` experiment adjusts only the global-profit card's coefficient. For example, gain 1.5 makes the formula `1 + 1.5r`, retaining ×1.25 at level 1 and reaching ×2.4286 at level 100. Such candidates must not be merged with gain-2 results: they are different models.

## Benchmark that was not selected

The `light` curve and 50% Helping Hands payout achieved **15/15** passing synthetic stress comparisons after fitting tap scales and targets. Products supplied at least **60.39%** of income in tapping runs and **73.26%** without manual tapping. Manual speedups ranged from **10.08% to 19.86%**.

This does not make it an accepted final balance. Its level-100 product bonus was only ×1.2381, and some final-stage fits had waits exceeding fifteen minutes without a meaningful purchase. The user selected stronger card rewards instead. The benchmark remains evidence that the direction can preserve independent, fully multiplicative tapping, not permission to weaken the selected card target.

- [Exact weak-card benchmark parameters](TapBalanceExperiments/CardRework-StressCandidate.json)
- [Fresh 15-pair verification](TapBalanceExperiments/CardRework-StressVerified.json)
- [102 semantic checks](TapBalanceExperiments/CardRework-light-HH0.5-Checks.txt)

## Validation requirements

The stronger candidate must be measured against the existing five career cohorts and three synthetic level-100 collections at Fame 1, 5, 10, 20 and 33. Comparisons start from identical saved states and use the same management policy; manual tapping is one tap per second on average. Each pair must reach prestige, show 10–20% manual speedup, and keep products above half of earned cash in both arms.

Typical career pacing remains distinct from extreme synthetic builds: assigning hundreds of permanent card levels at low Fame is not a naturally earned opening. Such builds may finish faster than ordinary careers, but still have to pass the tap/product-share requirements. The catalogue-completion run retains the previously documented shorter-run exception; inserting a long idle wait to make its percentage pass is unacceptable.

Fame income, tap Fame and targets are fitted together when necessary. All total bonuses and targets must remain monotonic. Fit results are only candidates; a fresh sequential-career replay is required, and reported results must distinguish any remaining pacing failures from passing income ratios.

## Reproduction and tooling

`--card-curve balanced --hands-payout 0.5` selects the stronger prototype in the lab. `--card-experiment-checks` checks beneficial card progression, full ×32 manual/automatic effects, independence from product levels, previews and frame-independent Helping Hands accrual. Ordinary core checks without experimental arguments still validate the active game.

`--fit-tap-targets` searches fixed tap scales and targets. `--income-offset` adjusts a stage's Fame income for a candidate trace. `--include-stress` adds the level-100 fixtures to earned-career starts. `--require-primary-pace` and `--require-gaps` prevent selecting a ratio-only pass when the requested timing constraints fail. `--fames` can select a subset of stages for independent searches.

Searches stop traces after crossing their highest searched target; later samples cannot affect any candidate. The retained target, tap scale, loss, candidate count and feasible count matched the prior full-length search in [the trace parity check](TapBalanceExperiments/CardRework-TraceParity.json).

`Tools/RedesignLab/Merge-CardCandidate.ps1` combines completed fit files, rejects mixed card/skill models, checks all 34 stages and increasing total bonuses/targets, and writes a reviewable candidate JSON. `--candidate <path>` loads that candidate into a fresh matrix run. Partial fits are explicitly marked and cannot establish acceptance. None of these commands writes player saves or applies the candidate to Unity.
