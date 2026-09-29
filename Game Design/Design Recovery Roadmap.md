# Rocket Raja design recovery roadmap

**Status: proposed experiments, 30 September 2026.** No economy or UI change in this roadmap is shipped by the research task. Read the [Comprehensive Design Audit](Comprehensive%20Design%20Audit.md) for the problem evidence, [Research Paper Review](Research%20Paper%20Review.md) for methods and limits, and [Incremental Game Benchmarks](Incremental%20Game%20Benchmarks.md) for developer-documented precedents. The earlier [Balance Experiment Plan](Balance%20Experiment%20Plan.md) covers the initial portfolio, tap, and prestige concepts; the refinements below address risks discovered in this deeper audit.

## Design objective

Give the player a visible, useful choice across the whole first run and a distinct reason to begin the next one. A stronger income number by itself is insufficient if players cannot tell what caused it, find the action that matters, or see a new goal. Preserve passive progress: making tapping useful should not make rapid tapping mandatory.

## 1. Establish a trustworthy baseline before changing values

The current unmodified simulator fails first- and second-run pacing assertions. Its saved report is older than the current behavior. Treat **46:26 / 12:36 / 9:10** as the current *automatic active-policy diagnostic*, with all eight families at **7:31** and last purchased upgrade at **26:30**. The historical **24:35 / 9:18 / 5:55** values should remain archived for comparison, not appear as a passing current result. The simulator taps at three per second, calls Buy All, and optimizes purchases; it cannot represent an uncoached player. [Simulator](../Tools/EconomyLab/Program.cs#L124), [saved report](../Documentation/EconomySimulation.json)

| Baseline work | Output and acceptance |
|---|---|
| Record build/catalog identity | Each trace includes a code commit, `EconomyVersion`, authored asset/export hash, policy name, and timestamp. A catalog change invalidates the previous balance report. |
| Capture decision cadence | Log first product, first milestone, each family unlock, each upgrade purchase, longest time without a useful new option, prestige preview, reset, and recovery. Do not count a level buy as new authored content merely because it changes a number. |
| Run multiple policies | At minimum: three-tap active, low-tap active, no manual taps after automation, occasional check-in, offline-focused, no ads, and a separate sponsor-assisted policy. Keep the same catalog. |
| Observe real sessions | Run at least five uncoached first-prestige sessions and several returning sessions for qualitative diagnosis. Record exact time, chosen actions, confusion, and short post-session explanations. A small qualitative sample cannot estimate population retention. |
| Resolve target ownership | Decide whether the current 20–30 minute first-run and 5–10 minute rebuild bounds remain the intended product targets. Then make the checked-in assertion/report match the chosen targets and current catalog. Do not silently relax a failing test. |

**Core trace measures.** At each checkpoint compute family contribution `family normal ₹/s ÷ total normal ₹/s`; marginal portfolio return `Δtotal normal ₹/s ÷ cost` for the next available purchase; instantaneous three-tap rate relative to normal production; **actual** manual/auto tap earnings as a share of active-session eligible earnings; time until the next useful buy; and time from reset to the previous pre-reset income rate. These metrics answer different questions. A family can have a tiny direct share but a strong portfolio synergy, and a displayed tap rate can differ from actual player tapping.

## 2. Repair discoverability and reward clarity

These changes can be specified and tested before adding a new currency or major mechanic.

1. **Make all affordable choices visible.** Replace the Player panel's cheapest-ten remaining pool with an accessible list or tabs for product, all-product, tap, and skill upgrades. Preserve an individual selection path. Add “current ₹/s → after purchase” or “applies when owned” where appropriate. Buy All may remain as an explicit convenience, with a cost and effect summary. The engine currently permits upgrades for unowned families and has no non-skill level gates, so the UI must describe those rules truthfully. [Player list](../Assets/Game/Runtime/ImportedPlayerScreen.cs#L132), [engine](../Assets/Game/Core/GameEngine.cs#L133)
2. **Put the most consequential first Legacy choices in view.** Group whole-shop, active-tap, family, and utility perks; show the five-point first-reward budget. For each option, preview both an **early matched state** and a **late matched state**. A preview after only the first product is bought would show zero benefit from a final-family perk and unfairly bias the decision. Compare time to the first few families and to the next prestige under a labeled simulation policy. [Legacy order](../Assets/Game/Runtime/ImportedLegacyScreen.cs#L65), [catalog](../Assets/Game/Core/Catalog.cs#L73)
3. **Clarify the reset decision.** Use one amount notation for gate and progress; show the next reputation breakpoint (`6` points at `1.44 × threshold` after the initial five), expected effect of an intended Legacy purchase, and what resets or persists. Keep any timing estimate explicitly labeled as a model. Add a persistent “prestige ready” entry point. [Prestige UI](../Assets/Game/Runtime/ImportedPrestigeScreen.cs#L68), [reward formula](../Assets/Game/Core/GameEngine.cs#L163)
4. **Make progression art truthful.** The current imported Home uses static product sprites and one backdrop even when `VisualTier` rises. Either implement distinct owned/level/stall art and verify each transition or describe milestones as economic/animated pulses rather than visual display tiers. Art progression is potentially high value in a game whose opening promise is building a brighter store; test it after the core pacing trace. [Home builder](../Assets/Game/Editor/BuildImportedHome.cs#L52), [refresh](../Assets/Game/Runtime/ImportedHomeScreen.cs#L195)
5. **Teach systems where they appear.** A locked Home skill should reveal its family and cash requirement when touched. The first skill and prestige should have short contextual explanations. Test whether account and naming prompts can follow the first reward instead of preceding the first tap. Informational panels should say if they pause production or continue accruing it. [Home skill controls](../Assets/Game/Runtime/ImportedHomeScreen.cs#L214), [modal pause](../Assets/Game/Runtime/UiMotion.cs#L33)

**Review gate:** In uncoached tests, players should be able to identify their next productive purchase, describe why a selected Legacy perk helps the *next* run, find a locked skill's path, and distinguish the prestige threshold from current eligible earnings. Ask for explanation; a successful click alone does not show understanding.

## 3. Prototype economy alternatives separately

### 3A. Old-family relevance

The earlier plan suggests all-eight portfolio thresholds at levels 25/50/100. That is a reasonable prototype, but it has two risks: the **last family to reach each threshold** gets the whole bonus in marginal-payback terms, and after level 100 the starter-family reason disappears again. It can become a compulsory repeated checklist. Compare three variants against the unchanged baseline:

| Variant | Mechanic | Success signal | Failure signal |
|---|---|---|---|
| Shared portfolio milestone | A modest run-wide bonus when all families meet selected thresholds. | Starter purchases sometimes have competitive marginal *total* return; players understand the goal. | Every run follows one required eight-family shopping list, or first prestige accelerates beyond the chosen pacing range. |
| Directed supply link | One or two named early families boost a later family or tap route at authored milestones. | Early families have a distinct job and remain worth revisiting after all are owned. | The link is hidden, one mandatory supplier dominates, or newer family unlocks lose their impact. |
| Late family-specific upgrade | A few new purchases renew early output or unlock a one-time ability. | New mid/late decision arrives before the current 26:30 content cutoff and changes purchase ranking. | It is merely another automatic ×2 with no visible choice or new use. |

Test a family’s **direct income share and marginal portfolio gain** separately. Do not “fix” irrelevance by making every family equally productive: the frontier still needs to feel like a payoff. [AdVenture Capitalist's official Unlock explanation](https://screenzilla.helpshift.com/hc/en/5-adventure-capitalist/faq/506-unlocks-have-incorrect-descriptions/) gives a verified cross-business model, not a recommended multiplier.

### 3B. Active tap value

The prior prototype adds `0.08 × normal product rate ÷ 3` per credited tap after an unlock. It would restore cash from tapping but, **by itself**, would make the existing tap level, twelve Salesmanship purchases, and tap Legacy a smaller fraction of each payout. The goal is to restore the **marginal value of tap investment**, as well as make a tap feel useful. Test at least these alternatives:

| Variant | Rule to prototype | Specific balance risk |
|---|---|---|
| Flat bounded link | Existing tap payout plus a fixed share of normal product output per credited tap; cap credited tap rate. | New cash payoff may render existing tap purchases even less useful. |
| Mastery-scaled link | Multiply the linked portion by a bounded, diminishing factor derived from existing tap levels, Salesmanship ranks, and tap Legacy; keep Rush/ad money out of the base. | Stacking can exceed the intended active share; late tap ranks can still become worthless if the cap is reached too early. |
| Short deliberate burst | A clear skill window or streak goal temporarily raises tap value without continuous tapping. | Timing can become compulsory or fail to help players who cannot tap rapidly. |

For each variant, report **tap cash share**, seconds saved to the next purchase by a realistic tap burst, and marginal `Δtap earnings ÷ cost` for every tap level/upgrade rank at early, all-family, and post-prestige checkpoints. Compare low-tap and three-tap policies. The existing [Balance Experiment Plan](Balance%20Experiment%20Plan.md#experiment-b-active-tap-link) offers initial 8% and six-credited-tap values as test inputs, not research-derived targets.

### 3C. First prestige and run cadence

Start by testing the clearer preview and Legacy navigation without changing the reward curve. Then compare one change at a time: (i) an earlier first gate, (ii) a mid-run objective or family interaction between 7:31 and 26:30, (iii) a new post-prestige specialization or altered starting condition. A one-time first-reset choice could have whole-shop, active-tap, and offline/skill routes, with a respec path; each should change at least one **actual decision**, not just a number. Track both the moment of reset and the first five minutes after it.

The current square-root reward makes the reset/push choice meaningful only if the next-point requirement and its likely wait are visible. Simulate reward-per-minute for resetting at five points, pushing to six, and pushing further under at least two purchase policies. A later reputation sink is justified only if players approach the existing **434-point** fixed tree; until then, a new sink may add complexity without solving the first reset.

**Coupled pass:** Product synergies raise the normal rate; the tap link depends on that rate; global Legacy and skills multiply parts of both. Run all variants alone first, then a combined simulation. Check no-ad and sponsor-assisted outcomes, first three cycles, and offline returns. The [GEEvo economy study](Research%20Paper%20Review.md) supports exploring multiple simulated strategies, while also noting that simulation must be followed by player testing.

## 4. Save and authoring constraints before implementation

The old plan says to add later upgrades through the authored catalog. In this codebase that is **not sufficient**. [`GameState.Valid()`](../Assets/Game/Core/GameState.cs#L49) recognizes purchased upgrade IDs by rebuilding `Catalog.Create().upgrades`, not by the runtime-authored `GameContent.asset`. A new ID present only in the asset/export could make a saved state invalid after purchase. [`AccountSession.ReadState`](../Assets/Game/Core/AccountSession.cs#L35) rejects an economy version mismatch. Any new upgrade, Legacy field, or persisted specialization therefore requires:

1. Update the default catalog, authored asset, and simulator export together; reconcile ID validation with the actual catalog the save uses.
2. Define a save-compatible migration/version rule for existing local and cloud saves before release. Do not let a new content ID strand a saved purchase.
3. Test old-save restore, new purchase, prestige, cloud load, offline return, bulk buy, and all three policy traces on the same content version.
4. Update player-facing descriptions and visual feedback in the same implementation change as the mechanic.

This is a delivery constraint, not a reason to postpone design experiments. Catalog-only temporary simulations can still compare candidate rules before shipping code or migrations.

## 5. Playtest and decision sheet

Use the same sheet for baseline and each candidate. Short qualitative studies uncover misunderstandings; they do not support a retention percentage claim. If telemetry is added for a larger test, collect only what is needed for design decisions and follow the project's privacy/consent requirements.

| Moment | Observe/ask | Quantitative record |
|---|---|---|
| First playable minute | Did the player know where to tap, buy, and look next? Did account/naming interrupt the first reward? | Time to first tap, product, first useful choice; dismissed prompts. |
| After all eight families | Which earlier family would they buy now, and why? Is the Home art changing enough to signal growth? | Family shares, marginal returns, upgrade visibility, time until next authored choice. |
| When tapping fades | Do they keep tapping, stop intentionally, or think the popup still matters? | Actual manual tap earnings, tap cadence, next-purchase time saved. |
| First prestige preview | Can they state the gate, reward, resets, retained perk, and next-run effect? | Preview opened, next reward threshold, chosen perk, reset timing. |
| Five minutes after reset | Did the run feel changed and worth repeating? Is an old purchase path chore-like? | Recovery time, first new decision, second-run product order and skill usage. |
| Returning after hours | Is the offline amount understandable and is there a useful next action? | Absence, payout/cap, ad attempt and failure, time to playable signed-in session. |

**Candidate acceptance bands, not academic facts:** Retain the project's intended **20–30 minute first prestige and 5–10 minute first rebuild** only if the owner confirms those targets after baseline playtests. The earlier **5–15% active tap share** is a prototype band to compare with observed effort, not a universal idle-game rule. For all variants, require a clear new decision during the current long post-unlock interval, no broken or invalid saves, no forced ad route, and no collapse of occasional/offline play. Reject an intervention when it improves one headline number but makes the user's stated product, tap, or prestige experience worse in observed sessions.

## Recommended work order

1. **Evidence repair:** current versioned simulation, multiple policies, and uncoached baseline sessions.
2. **Clarity repair:** full upgrade visibility, Legacy preview/order, consistent prestige units, locked-skill explanation, modal/offline copy, and truthful visual-tier claims.
3. **Economy prototypes:** separately test old-family linkage, tap value with existing-upgrade ROI, and first-prestige timing/choice.
4. **Combined balance:** simulate coupled mechanics, ads, offline, Legacy caps, and save behavior; then repeat observed playtests.
5. **Long tail:** only after players reach and understand the existing loop, consider later authored product beats, rotating local objectives, and a diminishing reputation sink.

This order addresses the present evidence and choice gaps while keeping each experiment attributable. It also avoids taking Tap Tycoon, another game's patch note, or an academic paper as a ready-made Rocket Raja balance table.
