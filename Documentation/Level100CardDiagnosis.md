# Level-100 card balance diagnosis — 16 October 2026

Follow-up: the user selected revised card growth and Helping Hands base payout, with independent tapping and approximately ×2 product-card profit at level 100. [The experiment notes](CardReworkExperiments.md) track this work; the active balance below has not yet been replaced.

The balance is still incomplete. The previous 170 passing comparisons covered no-card and earned-card careers. The synthetic level-100 collections were not included in the coefficient fit: only 1 of their 15 comparisons passed. Together this is **171/185**, not a general card-balance pass.

No runtime economy, card effect, upgrade multiplier, save or skill behavior was changed by this investigation. The lab now reports income attribution, final product/tap levels, purchased upgrade counts and Helping Hands' nominal average rate. The stress fixtures are shared between the matrix and the offline fitter.

## What fails and why

The fixtures put selected cards at level 100 at Fame 1, 5, 10, 20 and 33. These are deliberately synthetic starting states, not claims about naturally earning those collections at that Fame. Each paired run starts with identical cards and progression; only manual tapping differs. Management occurs every 20 seconds, with an average of one manual tap per second.

At card level 100, the existing production formulas give:

| Card effect | Factor |
|---|---:|
| Each product's profit | ×21 |
| Tap profit | ×1.952381 |
| Global profit, affecting both | ×26 |
| Product/tap remaining level cost | ×0.615385 |
| Remaining cash-upgrade price | ×0.519231 |
| Skill duration | ×1.714286 |
| Skill cooldown | ×0.714286 |

The duration/cooldown combination increases nominal skill uptime by ×2.4. Tap payout plus Helping Hands uptime therefore increases its average earning capacity by approximately ×4.6857 before cheaper tap levels and purchase decisions are considered. These are existing card formulas, not new diminishing returns introduced by this investigation.

The asymmetry makes a single Fame tap coefficient inadequate for the tested builds: more tap income helps product-heavy collections but can worsen tap-heavy ones. Global profit does not directly favor tapping; it multiplies both sources. Discounts and additional cash do change when purchases and milestones become affordable.

### Purchase strategy contributes substantially

The default simulator buys all affordable cash upgrades, then selects up to two level purchases by immediate projected income per cost. A tap-enabled run includes manual taps in that selection; the no-manual run does not. It is the same decision algorithm, but it can choose different investments. Negative measured speedup does not mean `Tap()` removes money; it means the simulated purchasing paths diverged.

The all-cards tapping arm at Fame 33 finishes with product levels `[1, 1, 1, 1954, 3415, 1, 3457, 2605]` and tap level 3617. Four products stay at level 1, preventing every all-eight synergy. Helping Hands accounts for almost all earned cash in that run. The comparison without manual taps levels all eight products above 3400. This is evidence of a strategy-dependent failure, not proof that a well-managed all-cards collection must be tap-dominated.

Changing the policy confirms that distinction, but does not fix the complete acceptance criteria:

| Policy, same runtime economy | Passing stress pairs | All-cards Fame 33 product share | All-cards Fame 33 speedup |
|---|---:|---:|---:|
| Default Max, cash upgrades first | 1/15 | less than 0.001% | 32.67% |
| Max, levels before cash upgrades | 2/15 | 95.52% | 13.15% |
| Cash first, fixed 100/10/1 level batches | 0/15 | 95.73% | 0.00% rounded |

The alternate policies must not replace the failing default results or be described as a gameplay fix. Several levels-first runs also fail to reach prestige within the one-hour limit.

### Helping Hands competes with manual tapping

At maximum cash skill ranks and level-100 skill cards, Helping Hands has 16 taps/second, a 77.142857-second duration and a 160.714286-second cooldown. Its nominal average is **7.68 full-value automatic taps per second**, before management delays and Rush timing.

For a simple steady-income interval with one manual tap per second, no purchases and no Rush, let `P` be product cash/second and `T` a full tap payout. Total active income is `P + 7.68T + T`. If products must exceed half of total income, manual tapping supplies less than `0.5 / 8.68 = 5.76%` of that total. Consequently manual tapping alone cannot shorten that steady interval by 10% while products remain the majority.

This is a local steady-income constraint, **not a proof about complete runs**: earlier purchases, milestone jumps, skill cycles and decision timing can amplify or suppress run-time benefits. It explains why relying on purchase thresholds to achieve 10–20% is fragile with these collections. Every ×32 upgrade still multiplies both manual and automatic tap payout by exactly ×32; increasing it does not improve manual tapping relative to Helping Hands.

## Can fixed coefficients solve it?

An offline search at Fame 1 tested 15 fixed tap scales from 0.01 to 160 and target offsets from −3 to +6 log10 in 0.01 steps. Traces used the production engine for up to 2,400 seconds. Of **9,362 candidates where all four builds reached the target**, none met both 10–20% speedup and product majority in both arms for all four builds. At best, three of four passed together.

The four builds were no cards, tap-heavy level 100, product-heavy level 100 and all cards level 100. This is a finite search, not proof that every possible coefficient or different price curve is infeasible. It is evidence against claiming that another small tap/Fame adjustment fixes the problem.

Full authored upgrade multiplication can be retained in a broader redesign, but the design choice remains unresolved: test revised card growth and a separately authored Helping Hands base payout, or test a production-linked manual bonus for product-heavy builds. Neither has been applied or validated. The earlier instruction to keep skill behavior unchanged is why these are presented for a design decision rather than silently implemented.

## Evidence and reproduction

- [Detailed baseline stress results](TapBalanceStressDiagnosis.json), including source attribution and final levels.
- [Levels-first comparison](TapBalanceExperiments/Stress-LevelsFirst.json).
- [Fixed-batch comparison](TapBalanceExperiments/Stress-FixedBulk.json).
- [Finite coefficient search](TapBalanceExperiments/Stress-Fame1-Feasibility.json).
- [Core checks](TapStressCoreChecks.txt): all 757 passed, including full ×32 manual/Helping Hands payout with level-100 tap cards.

Run from the project root:

```powershell
dotnet run -c Release --project Tools/RedesignLab -- --tap-matrix --stress-only
dotnet run -c Release --project Tools/RedesignLab -- --tap-matrix --stress-only --levels-first
dotnet run -c Release --project Tools/RedesignLab -- --tap-matrix --stress-only --fixed-bulk
dotnet run -c Release --project Tools/RedesignLab -- --fit-tap-targets --stress-only --fame 1 --grid --require-ratios --scales 0.01,0.03,0.1,0.3,0.6,1,1.5,2,3,5,10,20,40,80,160
```

These are simulated measurements. No new Unity or human playtest is claimed; runtime behavior was not changed.
