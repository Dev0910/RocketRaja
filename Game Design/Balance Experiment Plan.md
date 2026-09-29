# Rocket Raja balance experiment plan

Status: design proposal, 30 September 2026. No changes in this document have been applied to the game. The implemented baseline is in [Rocket Raja Economy Audit](Rocket%20Raja%20Economy%20Audit.md), and the design rationale is in [Comparison and Recommendations](Comparison%20and%20Recommendations.md).

**Deep-audit amendment.** The [Comprehensive Design Audit](Comprehensive%20Design%20Audit.md) and [Design Recovery Roadmap](Design%20Recovery%20Roadmap.md) add a first-run content-cadence trace, hidden upgrade/Legacy choices, static Home art despite tier counters, and save-validation constraints. The prototypes below remain candidate experiments, not confirmed fixes.

## What success should feel like

- Unlocking a new fireworks family gives a noticeable production jump, while upgrading an older family occasionally remains a smart way to improve the whole stall.
- Tapping remains a worthwhile *active* choice after all eight families are open, without making sustained high-speed tapping necessary for offline or accessibility-friendly play.
- The first prestige makes the next run visibly different and faster. Players can name what they kept, what they gained, and the next goal.
- Each economy change preserves the game's stable saves, clear reward disclosures, and testable pricing rules.

These are outcome criteria. The numbers below are prototype starting values, not inferred Tap Tycoon constants or approved shipping balance.

## Baseline to preserve and measure

The current authored catalog is [GameContent.asset](../Assets/Resources/GameContent.asset); the exported simulation copy is [ContentCatalog.json](../Documentation/ContentCatalog.json). The built-in defaults in [Catalog.cs](../Assets/Game/Core/Catalog.cs#L50) are not automatically written over an existing authored asset. Any experiment must update the intended asset/export pair and run the simulator against the exported catalog.

[Balance.md](../Documentation/Balance.md) stores deterministic active-policy results of first product 0:48, all families 5:07, first prestige 24:35, first rebuild 9:18, and third cycle 5:55. **Those saved results are stale for the current code:** the unmodified simulator now fails its first-run pacing assertion. A temporary diagnostic copy with only assertion logging produced first product 0:48, all families 7:31, first prestige **46:26**, second run **12:36**, and third run **9:10** against the current exported catalog. See [Rocket Raja Economy Audit](Rocket%20Raja%20Economy%20Audit.md#7-what-the-existing-simulator-shows-and-the-current-pacing-failure) for the method and limits. The policy taps three times each second, buys upgrades automatically in display order, selects product levels by marginal income per rupee, caps tap investment at level 60, and uses skills when ready. Human sessions may differ sharply. [Validation.md](../Documentation/Validation.md) still calls for uncoached three-cycle playtesting.

Before editing values, collect a baseline trace at several moments: first product, first level 25, last family unlock, 10 and 20 minutes, first prestige preview, immediately after prestige, and when the former earning rate is recovered. For each point capture:

| Measure | Definition | Why it matters |
|---|---|---|
| Family share | `family normal ₹/s ÷ total normal product ₹/s` | Shows what the player sees as a contribution. |
| Marginal portfolio return | `increase in total normal ₹/s ÷ cash cost` for the next purchase, including milestone effects | Finds whether an older family can ever be a good *decision*, even with low direct share. |
| Tap share | Cash earned from manual and auto taps ÷ total *active-session* eligible cash | Avoids diluting active play with hours of offline production. Record manual and auto separately. |
| Time to next useful buy | Cash gap ÷ normal rate, with and without an active burst | Shows dead zones and whether skills create a short-term plan. |
| Prestige recovery | Time after reset to regain the pre-reset normal ₹/s and time to reach the next prestige gate | Measures permanent reward value better than an abstract multiplier alone. |
| Reward understanding | In an uncoached test, can the player explain the exact reputation gain, retained perks, and intended next purchase? | Distinguishes a math problem from a presentation problem. |

Use no-ad and rewarded-ad paths separately. Sponsor extra cash and offline ad doubling are excluded from prestige-eligible income, while duplicate ad receipts are rejected without payout ([GameEngine.cs](../Assets/Game/Core/GameEngine.cs#L163); [Architecture.md](../Documentation/Architecture.md)). Do not make those placements the only path to a reasonable first reset.

## Experiment A: portfolio milestones

**Hypothesis:** A small shared bonus for bringing all eight families to a level threshold will keep the first families strategically relevant after Celebration boxes unlock.

Prototype thresholds: every family at levels **25, 50, and 100**, granting **1.2×, 1.2×, and 1.3×** normal product income respectively. The cumulative prototype boost is 1.872× after all three. Use a separate portfolio multiplier rather than changing each family's self milestone, so the source of the gain is clear and reversible. Show “X of 8 families at level 50” and the expected total output increase. Keep the current personal doubles intact for this first test.

The key measure is **marginal portfolio return**. Record how often a purchase of one of the first three families becomes competitive with a purchase of the last three; also record whether all eight become a rote checklist. If the bonus is always obligatory and collapses the early run into identical steps, try differentiated supply links instead: one early family boosts another's rate, one boosts tap-linked earnings, one reduces a category's level cost. Test one at a time.

The all-eight 25/50/100 proposal also ends at level 100; it does not by itself give the starting family a role later. Because the final family crossing each threshold triggers the entire bonus, its marginal return can dwarf the other seven threshold purchases. Compare a directed family link and a later authored upgrade before adopting the shared threshold as the permanent solution.

**Balance guardrails:** First repair or intentionally revise the simulator's failing pacing assertions; the current 46:26 / 12:36 trace already misses the documented 20–30 / 5–10 minute targets. Re-run first and second prestige timings after each experiment. Occasional/offline pacing must not become punitive. Check the exact threshold purchase for discontinuities and that buying in bulk grants the same reward as single-level buying.

## Experiment B: active tap link

**Hypothesis:** Linking a bounded part of tapping to the stall's normal production rate will prevent the tap button from fading as products grow.

Candidate starting rule, after a clear unlock such as owning four families:

```text
normalRate = normal product income per second, before temporary skills or ads
existingTap = current GameEngine tap payout, including tap upgrades and tap Legacy
linkedTap = 0.08 × normalRate ÷ 3
newTap = existingTap + linkedTap
```

At a normal rate of ₹1,000,000/s, this adds about ₹26,667 per tap. Three credited taps per second add ₹80,000/s, which is 8% of passive production before the existing tap payout. Prototype a maximum of **six credited taps per second for the linked component** (manual and auto combined); leave the current base tap behavior alone. That makes the linked contribution no more than 16% of normal production at the initial 8% coefficient. A distinct tap mastery upgrade can raise the coefficient in small steps, with an explicit ceiling. Existing Salesmanship purchases should not apply their full ×2-per-purchase effect to the linked term without a separate balance test.

This flat linked component could make existing tap levels, Salesmanship ranks, and tap Legacy **less important to the marginal value of each tap**, even while the tap popup rises. Measure the return on every existing tap investment. In a second variant, allow those investments to improve the linked component through a bounded, diminishing factor; check that no rank becomes useless because the factor hits its cap too early. Adding a new tap mastery track alone would not repair the value of the existing purchases.

Start with normal production, not the Festival Rush rate, to prevent a product buff from silently multiplying the tap link. A later combo experiment could let Festival Rush raise the linked term by a small fixed amount for its 30-second window. Show separate UI amounts for the current tap base and the stall-derived bonus. Explain the unlock in plain language: “Every tap now earns a share of your shop's production.”

**Measure:** At three taps per second, aim to test an active-session tap share around **5–15%** after the final product is unlocked; this is an initial playtest band, not a genre standard. Track whether bursts make taps too dominant, whether Helping Hands consumes the link cap, and whether players with slower tapping still progress via products. If the existing base already pushes tap share above the band, lower the linked coefficient rather than nerfing earned purchases without cause.

## Experiment C: prestige meaning

**Hypothesis:** The present reputation math already shortens runs, but a clearer forecast and a first persistent specialization will make the reset feel worthwhile.

Start with presentation before changing rewards. In the prestige panel, retain the existing reset/keep disclosure and add a before/after preview for the next run at **matched early and late checkpoints**, after spending the projected reputation on a selected Legacy perk, plus the next permanent milestone. Product production is zero at the literal start of a fresh run; a first-product-only comparison would also show zero immediate value for a Celebration boxes perk. Compare the same product states across options and add an estimated rebuild-time delta under a named policy. Label any timing estimate as a simulation, not a promise. The current [prestige screen](../Assets/Game/Runtime/ImportedPrestigeScreen.cs#L68) displays the reward and gate but does not forecast the rebuild numerically.

Then prototype a **single first-prestige Legacy choice** separate from raw reputation: choose one of (1) a portfolio threshold bonus, (2) a bounded tap-link improvement, or (3) a shorter skill cooldown/offline cap benefit. Give each a different play consequence, show it before reset, and allow a later respec. This borrows Tap Tycoon's persistent-specialization principle without duplicating random Tech Card acquisition or its premium currency.

The present formula gives `floor(5√(eligible earnings / ₹1.7×10^17))` reputation at the reset gate, with five at the threshold and ten at four times the threshold ([GameEngine.cs](../Assets/Game/Core/GameEngine.cs#L163)). Keeping this formula through the first experiment isolates presentation and specialization effects. If human tests still find the first prestige unrewarding despite seeing and using its permanent gain, compare a lower gate, an earlier small preview reward, or a one-time first-prestige bonus. Change only one of these at a time.

The fixed Legacy tree currently costs **434 reputation** to complete. Record when, or whether, testers approach that ceiling before designing a repeatable sink. A later sink should have diminishing marginal rewards and a clear cap on any tap/product conversion; a perpetual flat multiplier can trivialize the next-run pacing.

## Validation sequence

1. **Baseline capture.** Save the authored catalog version and export, run the no-ad simulator, capture per-family/tap traces, and conduct at least five uncoached first-prestige sessions. Ask players to describe their own purchase choices, what prestige grants, and why they would reset again. A tiny study cannot estimate population retention, but it can expose comprehension failures.
2. **A/B/C separately.** Introduce each prototype on its own branch or catalog variant. For every variant run active, occasional, and offline policies, including the first three prestige cycles. Compare income shares, purchase ranking, time to all families, reset timing, and post-reset recovery to the baseline.
3. **Combined pass.** Portfolio bonuses raise normal product income; the tap link uses that income; Legacy can multiply one or both. Simulate the combined effect rather than summing individual results. Check timed skill combinations and tap-rate caps explicitly.
4. **Human playtest.** After the combined pass, test fresh players and returning players. Observe where they stop tapping, which product they buy after the last unlock, whether they can predict portfolio and Legacy gains, and whether the second run feels distinct. Record their words alongside telemetry.
5. **Regression and migration.** Check single/bulk purchase equivalence, price rounding, prestige quote/confirm consistency, ad exclusion, offline cap, save restoration, and cloud ownership paths. New persisted specialization or run-goal fields need an explicit save migration plan rather than a silent schema change. `GameState.Valid()` currently validates purchased upgrade IDs against `Catalog.Create().upgrades`, so adding a new upgrade only to the authored asset/export can invalidate saves; update default catalog/validation and version handling together. [GameState](../Assets/Game/Core/GameState.cs#L49), [AccountSession](../Assets/Game/Core/AccountSession.cs#L35)

**Decision rule:** Ship a change only when it improves the targeted behavior without making the opening, offline path, or next prestige substantially worse. Simulator assertions protect technical pacing; player observation decides whether the loop actually feels better.
