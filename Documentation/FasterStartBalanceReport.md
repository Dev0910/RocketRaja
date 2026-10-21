# Faster-start balance measurements

**Historical coefficients:** the [15 October tap calibration report](TapBalanceReport.md) supersedes the tapping/Fame/target measurements below. The original mechanics and investigation remain recorded here.

Economy **3.0.0**, content **faster-start-v1**. Simulations recorded 7 October 2026; final core and Unity interaction validation repeated 12 October 2026.

The implemented mechanics and the primary simulated route meet the opening and successive-prestige timing targets. **Balance acceptance remains partial:** tapping's benefit varies substantially between stages, card outcomes can make tapping the largest income source, and alternative spending policies can miss the timing targets. These results do not establish human playtest pacing.

## Reproducible inputs

The catalogue and Talent roster source SHA256 is `588A7EC32CC558550CEF85DF246491F5804E27DB4B1C894C3613708A9F8FDB0D`. The catalogue, Unity content asset, JSON export and manifest were regenerated together and validated. Every linked simulation uses this source hash.

The simulator calls production `RedesignEngine` earning, pricing, milestone, purchase, skill, draw and prestige methods. It advances time in one-second steps. No separate approximation of the economy is used.

- Relaxed management: one decision every 20 seconds; attentive management: every 10 seconds.
- Manual tapping: two taps per second during the second half of each decision interval, averaging one per second. No-tap comparisons remove manual taps; Helping Hands remains available.
- At a decision, buy affordable cash upgrades first, then make up to two level purchases. Prioritize stocking an unowned product; otherwise choose the highest projected incremental income per cash spent, including expected Helping Hands income. Stock one initial level, then use Max.
- Activate available skills at each decision. There are no ads, purchases, offline earnings, missed sessions or animation/input delays in the model.
- Prestige at eligibility, or at four times the current target for the extended-run profile. Card-enabled profiles immediately spend free credits, using the recorded random seed.
- A meaningful purchase is a cash upgrade or a level-purchase batch increasing baseline product-plus-tap payout by at least 10%. Gap measurements include the unfinished interval before prestige. This is a measurement convention, not a new gameplay rule.

“Relaxed” describes decision frequency, not imperfect purchase selection: the simulator uses exact projections. A person may choose differently. Max is now the initial quantity selection, and the UI advises purchasing cash upgrades before using Max for levels.

## Published coefficients

| Parameter | Implemented value |
|---|---|
| Catalogue price, zero-based position `i` | `20 × exp(0.36i + 0.00005i²)` |
| Product base cost, zero-based product `p` | `30 × 14^p` |
| Product base income per second | `0.85 × 2.5^p` |
| Product/tap level-price growth | `1.12` |
| Tap-training initial cost | `40` |
| Direct tap base and level factor | `2.6 × (1 + 1.4 × level)` |
| First prestige target | `6e21` |
| Target at one total Fame | `3e23` |
| Target growth per calibrated Fame stage | `100000` |
| Income at zero / one Fame | `×1 / ×6` |
| Subsequent Fame income | Authored log10 table in [FasterStartPrestigeFit.json](FasterStartPrestigeFit.json) |

The source of these constants is [EconomyBalance.cs](../Assets/Game/Core/EconomyBalance.cs). All authored upgrade multipliers are in the [1,000-entry catalogue](../Game%20Design/1000%20Unlockables%20Catalogue.md). The [implemented design](../Game%20Design/Faster-start%20Economy.md) specifies strongest-discount rules, card stacking, milestones, synergy and save migration.

The Fame table was fitted offline alongside the price and income curves. It is fixed at runtime. Extrapolation beyond the authored table is numerical continuity, not a measured endgame prediction. There is no total-completion deadline.

## Simulation results

Times are minutes per run. Later ranges exclude runs one and two. Catalogue progress is lifetime unique discoveries at the end of each sample, not a promise of completion time.

| Profile | Runs | All eight, first run | First prestige | Second prestige | Later range | Unique upgrades |
|---|---:|---:|---:|---:|---:|---:|
| [Relaxed, no cards](FasterStartRelaxed.json) | 34 | 7.0 | 27.8 | 12.5 | 19.6–22.5 | 1,000 |
| [Relaxed, seeded cards](FasterStartCards.json) | 34 | 7.0 | 27.8 | 12.4 | 13.0–22.5 | 1,000 |
| [No manual taps](FasterStartNoTap.json) | 34 | 13.3 | 33.7 | 13.6 | 23.0–31.7 | 1,000 |
| [Attentive, seeded cards](FasterStartAttentive.json) | 12 | 6.2 | 28.4 | 11.8 | 15.8–30.2 | 432 |
| [Continue to four times target](FasterStartPush.json) | 12 | 7.0 | 43.7 | 24.8 | 17.7–28.3 | 738 |
| [Fixed ×100/×10/×1 purchases](FasterStartFixedBulk.json) | 6 | 9.7 | 45.3 | 27.0 | 25.8–40.7 | 257 |
| [Levels before cash upgrades](FasterStartLevelsFirst.json) | 1 | 15.7 | Not reached within 60 | — | — | 61 |

The more frequent greedy policy is not consistently faster: purchases alter later affordability and milestone timing. The fixed-bundle and levels-first profiles expose real strategy sensitivity; they are not accepted as meeting the opening target.

Additional twelve-run card cohorts, [seed 2](FasterStartCardsSeed2.json), [seed 7](FasterStartCardsSeed7.json) and [seed 42](FasterStartCardsSeed42.json), have later ranges of 15.3–21.9, 18.4–26.8 and 19.3–22.2 minutes respectively. The 34-run seed-1 cohort includes later runs below 15 minutes.

### Tapping and income composition

[Matched comparisons](FasterStartPaired.json) start independently at Fame 0 through 33, with identical empty card collections and purchasing policies. The only changed input is manual tapping. This avoids comparing sequential runs after different Fame rewards have put them at different stages.

Manual tapping reduces run duration by a **median 17.3%**. Individual reductions range from **2.8% to 30.1%**; **18 of 34** pairs fall within the requested 10–20% range. The opening reduction is 17.6%. The target is met at the median, not at every stage.

Products supply at least **60.8%** of total earned cash in every primary no-card run, counting product-based Rush and Bulk Order earnings. Manual taps also accelerate early investment, so their share of final cash is not the same as their pacing benefit. With cards enabled, the minimum product share in the seed-1 cohort is **36.9%**: tap and Helping Hands card combinations can dominate some runs. Keeping existing card and skill mechanics leaves this acceptance criterion unresolved for those outcomes.

### Purchases and prestige value

In the primary cohort, every run's 90th-percentile meaningful-purchase gap is at most **100 seconds**. The longest single gap is **500 seconds**. Frequent purchases meet the intended rhythm, but occasional breakthroughs exceed three minutes. The attentive cohort has a worst per-run 90th percentile of 250 seconds and a longest gap of 580 seconds.

Immediate prestige gives ×6 Fame income after the first run. Lifetime discoveries advance from **123 → 135 → 166 → 196** across the first four no-card runs, and reach all 1,000 in the sampled 34 runs. Progress does not stop at repeatedly replaying the opening.

Continuing to four times the target gives two Fame instead of one. The first run retains its fixed three-credit award; on the second run, pushing gives two credits versus one for immediate prestige. In the extended cohort, discoveries reach **126 → 170 → 228 → 288** across the first four runs. Extra Fame also advances the next run's target, preventing repeated farming of an obsolete low target. These comparisons demonstrate a reward tradeoff; they do not establish a universally optimal prestige strategy.

## Verification and observed Unity behavior

- [Core checks](FasterStartCoreChecks.txt): **744 passed**, including catalogue counts and bounds, signature preservation, authored import values, discount ordering and permanent-card stacking, milestone boundaries and bulk crossing, synergy exclusion/reset, suffix transitions, large values and old/current/future save handling.
- [Unity core validation](Implementation/Unity-Core.txt): regeneration, asset/export agreement, save codec and runtime-driver checks passed on 12 October.
- [Unity milestone/UI checks](FasterStartUnityUiChecks.txt): **25 passed**, including a disposable fresh opening, default Max controls, actual tile/popup milestone boundaries, synergy bottleneck labels and stock-one-then-Max behavior.
- [Unity interaction checks](FasterStartUnityInteractionChecks.txt): **51 passed** on 12 October, including tap raycasts across tabs, drag suppression, modal input isolation and unaffordable Max purchase states.
- [Synergy screenshot](FasterStartSynergy.png) records the inspected presentation.

The Unity opening test purchases the first product with starting cash and observes **0.85 cash per second**. These are automated Editor Play Mode checks against a disposable guest. **No uninterrupted human 20–30-minute playtest or device pacing measurement has been recorded.** Simulation seconds above must not be relabeled as observed player timing.

## Reproduction

Run from the project root:

```powershell
dotnet run --project Tools/RedesignLab
dotnet run --project Tools/RedesignLab -- --balance --runs 34 --no-cards
dotnet run --project Tools/RedesignLab -- --balance --runs 34 --no-cards --no-tap
dotnet run --project Tools/RedesignLab -- --paired-balance
dotnet run --project Tools/RedesignLab -- --balance --runs 34 --seed 1
dotnet run --project Tools/RedesignLab -- --balance --runs 12 --decisions 10
dotnet run --project Tools/RedesignLab -- --balance --runs 12 --push 4
dotnet run --project Tools/RedesignLab -- --balance --runs 6 --fixed-bulk
dotnet run --project Tools/RedesignLab -- --balance --runs 1 --levels-first
```

JSON is written to stdout; compact run summaries go to stderr. Card seeds use the .NET seeded random implementation in the lab. The archived JSON includes the constants and source hash used for each run.

`--fit-prestige` writes candidate Fame coefficients to `FasterStartPrestigeFit.json`; these require review and deliberate application to the source. `--fit-taps` edits authored tap factors in the catalogue. Neither is a read-only check. After changing authored content, run Unity `RedesignAuthoring.Regenerate()`, then `RedesignCoreValidation.Run()`, the lab checks and the measurement profiles again.

## Remaining balance work

Mechanics are implemented and verified. Before declaring the entire balance finished, validate the default purchasing flow with human play, reduce stage-to-stage tapping variance, and address tap-heavy card outcomes and slow alternative spending policies. The primary calibration is usable, but the full acceptance matrix is not uniformly green.
