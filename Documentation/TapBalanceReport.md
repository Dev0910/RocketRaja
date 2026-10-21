# Tap and card balance — 15 October 2026

**16 October follow-up:** overall balance remains incomplete. Including the level-100 stress builds gives **171/185 passing pairs**, with 14 unresolved failures. See [the level-100 diagnosis](Level100CardDiagnosis.md) for card-growth asymmetry, purchase-strategy failures, Helping Hands attribution and the unsuccessful fixed-coefficient search. The 170-pair result below is limited to its stated career cohorts.

**170 of 170 matched comparisons pass:** regular manual tapping shortens runs by **10.04%–19.99%**, and products supply at least **55.05%** of earned cash in the tapping arm. The median speedup is **16.06%**. Products also remain the majority in every no-manual-tap arm, with a minimum share of **56.07%**.

This is a measured result for the play policy and card collections below. It is not a mathematical guarantee for every card combination, tap frequency or spending strategy. The user chose to retain full multipliers and calibrate tested builds instead of introducing diminishing returns.

## Implemented changes

Balance revision: `tap-balance-2026-10-15`. Economy/save version remains `3.0.0`; content remains `faster-start-v1`.

- Fame now has separately authored product and tap income bonuses. The product curve is unchanged. A fixed per-Fame scale adjusts the tap bonus, including Helping Hands. Both total bonuses increase with Fame.
- Prestige targets use authored offsets around the existing exponential backbone. This avoids some purchase thresholds where a small tap-income difference produced an unusually large or negligible run-time difference.
- Every tap-profit upgrade still applies its full authored ×2–×32 multiplier. Cards, global upgrades, milestones and skill timing retain their existing effects. There are no product-relative payout caps, hidden tap limits or diminishing returns.
- Player and prestige screens show the product and tap Fame bonuses separately.
- Existing 3.0.0 saves retain progression and the current run's original target. Revised targets begin at the next prestige. This patch does not repeat the previous economy reset.
- Accrual reuses unchanged base income calculations within one `Advance` call; temporary skill boundaries remain exact. The first three original baseline runs reproduced byte-for-byte result rows after this optimization, before changing the balance constants.

The 1,000 catalogue entries, prices, signatures and authored upgrade factors are unchanged. Their source SHA256 remains `588A7EC32CC558550CEF85DF246491F5804E27DB4B1C894C3613708A9F8FDB0D`. Unity content, export and manifest validation passed after applying the new code constants.

## Constants and formulas

[EconomyBalance.cs](../Assets/Game/Core/EconomyBalance.cs) is the runtime source. [TapBalanceCalibration.json](TapBalanceCalibration.json) publishes all tap scales and target offsets plus the fitting comparisons. The unchanged product Fame log table is in [FasterStartPrestigeFit.json](FasterStartPrestigeFit.json).

For total Fame `F`:

```text
product Fame bonus = 10 ^ FameIncomeLogs[F]
tap Fame bonus     = product Fame bonus × TapFameScales[F]
first target       = 6e21
later target       = 3e23 × 100000^(FameStage(F) − 1) × 10^TargetOffsetLogs[F]
```

At one Fame, product income is ×6 and tap income is ×31.8. The second-run target is approximately `1.5386e23`. A lower relative tap scale at some later stages does not lower the total tap Fame bonus: the increasing product Fame bonus is included in the calculation. Monotonicity is checked across the authored stages and their continuation.

These tables were fitted offline. Runtime values do not depend on card ownership, tap activity, income composition or how quickly a player progresses. The unchanged product/upgrade price formulas and milestone rules are in the [current design](../Game%20Design/Faster-start%20Economy.md).

## Test scope

[TapBalanceMatrix.json](TapBalanceMatrix.json) contains the final 170 comparisons and the exact card levels, Fame, target, timings, income shares and pass/fail outcome for each pair:

| Cohort | Runs / pairs | Card policy |
|---|---:|---|
| No cards | 34 | Free credits remain unused |
| Seed 1 | 34 | Spend free credits immediately |
| Seed 2 | 34 | Spend free credits immediately |
| Seed 7 | 34 | Spend free credits immediately |
| Seed 42 | 34 | Spend free credits immediately |

Each cohort first plays a sequential career using the actual production engine. Every run-start state is then replayed twice with identical Fame, cards, discoveries, currency and target. One arm taps; the other does not. Equal run numbers from independently diverging careers are never treated as matched states.

The policy uses one-second simulation steps, management every 20 seconds, Buy All before up to two level purchases, exact projected income-per-price selection, priority for stocking unowned products, and Max for subsequent levels. Available skills are activated at decisions. Manual input is two taps per second during half of each management interval: **one tap per second on average**. Helping Hands remains active in both arms. There are no paid purchases, offline earnings, ads or input/animation delays.

The card seeds used for fitting are also the explicit regression coverage. Passing this matrix establishes reproducibility on these builds, not independent validation on unseen builds. Human play timing has not been measured.

## Pacing after the change

All figures below are simulated minutes. All eight products are acquired at **7.0 minutes** in each first run. All five careers reach all 1,000 lifetime discoveries by the end of the sampled 34 runs.

| Cohort | First prestige | Second prestige | Runs 3–33 | Catalogue-completion run |
|---|---:|---:|---:|---:|
| No cards | 27.8 | 10.7 | 16.8–30.0 | 9.6 |
| Seed 1 | 27.8 | 10.5 | 14.6–25.6 | 9.0 |
| Seed 2 | 27.8 | 10.4 | 15.3–25.4 | 9.4 |
| Seed 7 | 27.8 | 10.5 | 15.3–28.0 | 8.8 |
| Seed 42 | 27.8 | 10.3 | 15.2–27.7 | 9.4 |

The longest no-card run is 1,801 seconds, one second over 30 minutes. One ordinary seeded-card run is below 15 minutes. The final sampled run at Fame 33 deliberately finishes faster: moving its target earlier avoids a long wait after catalogue completion. It still passes the same tapping and product-majority checks. This is an explicit exception to the usual 15–30-minute target, not a total-completion deadline.

For the no-card cohort, every run's 90th-percentile meaningful-purchase gap is at most **180 seconds**, and the longest isolated gap is **461 seconds**. Across all seeded cohorts, those maxima are **200 seconds** and **521 seconds**. A meaningful purchase retains the earlier definition: a cash upgrade, or a level-purchase batch increasing baseline product-plus-tap payout by at least 10%.

The final catalogue-completion run's longest meaningful-purchase gap is now **80–140 seconds** across the five cohorts. An earlier candidate had a 1,004-second gap and was not retained.

## Other policies and limits

- [Attentive play](TapBalanceAttentive.json), using 10-second decisions and seed 1 over 12 runs, reaches first prestige in 28.4 minutes, second in 7.6 minutes, and later runs in 12.4–29.1 minutes. Product income remains at least 66.0%. This profile is not part of the 170-pair speedup guarantee.
- [Extended runs](TapBalanceExtended.json), continuing to four times the target with seed 1, take 43.7 minutes for the first run and 22.0 minutes for the second. Subsequent sampled runs take 14.1–33.4 minutes. Product income remains at least 76.7%. These runs trade time for greater Fame and later-run card rewards.
- [Extreme synthetic card builds](TapBalanceStress.json) set selected cards to level 100 at Fame 1, 5, 10, 20 and 33. Only **1 of 15** stress comparisons passes both requirements. Some tap-heavy configurations still let taps/Helping Hands dominate, while product-heavy configurations can make manual tapping negligible. These are recorded failures outside the calibrated seeded-career scope; retaining full multipliers does not guarantee balance for arbitrary collections.
- The earlier levels-first and fixed-small-bundle strategy findings have not been re-certified by this patch. No human pacing or device performance claim is made. Extrapolation beyond the authored Fame stages remains uncalibrated endgame behavior.

## Verification

- [Core checks](TapCalibrationCoreChecks.txt): **757 passed**, including full ×32 manual/Helping Hands upgrades with tap-heavy cards at multiple Fame stages, matching previews, increasing Fame bonuses and targets, old-target save retention, and adopting/persisting the revised target on the next prestige.
- [Unity core validation](Implementation/Unity-Core.txt): content regeneration and runtime/save checks passed with the final constants.
- [Unity interaction checks](TapCalibrationUnityUiChecks.txt): **54 passed**, including the new separate Fame labels and overflow checks against the saved UI.
- [Prestige screenshot](TapCalibrationPrestige.png): inspected after the Play Mode check. Tests use a disposable guest and restore the original scene.

## Reproduction

Run from the project root:

```powershell
dotnet run -c Release --project Tools/RedesignLab
dotnet run -c Release --project Tools/RedesignLab -- --tap-matrix --runs 34 --no-stress --verify
dotnet run -c Release --project Tools/RedesignLab -- --tap-matrix --stress-only
dotnet run -c Release --project Tools/RedesignLab -- --balance --runs 12 --decisions 10
dotnet run -c Release --project Tools/RedesignLab -- --balance --runs 12 --push 4
```

The matrix verification command returns a nonzero exit code if any selected pair misses 10–20%, either arm fails to reach prestige, or either arm's product share is not greater than 50%. The stress command intentionally reports results without asserting they pass. Output JSON includes the balance revision and fixed arrays.

`--fit-tap-targets --grid` is an offline experiment: it searches tap scales and target thresholds using production-engine traces, then writes candidate JSON. It does not apply changes to game source. Cache/refinement flags support repeating individual-stage searches; cached starts are disposable lab fixtures. Candidate files are research evidence, not active balance data. Do not run the older `--fit-taps` command as a validation step: it edits catalogue factors and optimizes the previous income-share objective.
