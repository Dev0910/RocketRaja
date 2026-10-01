# Rocket Raja — implementation TODO

Created: 1 October 2026. Status: **implementation not started by this checklist**.

The design decisions are recorded; the redesigned systems still need to be implemented in Unity. This document is the implementation backlog, not an instruction to begin coding or publish anything in this planning turn. All checkboxes start open. Mark a task complete only when its deliverable and relevant acceptance checks exist; attach a commit, test result, screenshot, or playtest note beside it.

## Design baseline

Use [Planning Decisions and Review v1](Planning%20Decisions%20and%20Review%20v1.md), [Economy Specification v1](Economy%20Specification%20v1.md), the [1,000-entry catalogue](1000%20Unlockables%20Catalogue.md), and [Premium Economy v1](Premium%20Economy%20v1.md). The cash-only decision is final: no prestige-count requirements on cash upgrades. Other balance coefficients and detailed UX are the current prototype defaults, subject to measured tuning.

Keep these decisions intact:

- 1,000 distinct run-local cash upgrades; manual buying and scoped Buy All; lifetime U counts first purchase of each ID once.
- Eight products available before first prestige; no compulsory all-eight ownership condition on prestige.
- Permanent Fame income, with x6 at the minimum first prestige; three first-prestige free credits and calculated later credits.
- 28 permanent Talent cards, 75% Common / 22% Rare / 3% Legendary, equal within tier; every copy adds one level, no pity, merge cost, slots, or finite gameplay level cap.
- 100 Kaju Katli per draw; one 100-KK gift after the second prestige; initial spending is draws only.
- Skills and cash improvements are repurchased every run. Base offline income is 50% of normal product production for eight hours.
- Earned decorations persist; run-specific product displays rebuild. Grant the current discounted cost of the first manual Phuljhari purchase on opening/rebuild.
- No rewarded ads or ad-funded income in the initial redesign. Achievements and daily/weekly currency sources remain deferred.

Implementation checklist: **0 / 79 tasks complete** at creation. Existing code and earlier design work are starting points, not completed redesign tasks.

## Delivery order

| Phase | Deliverable | Depends on | Priority |
|---|---|---|---|
| 00 | Baseline, versioning, and save-transition plan | — | First |
| 01 | Authoritative catalogue and numeric foundations | 00 | Core |
| 02 | State, durable commands, and save compatibility | 00, 01 | Core |
| 03 | Products, tapping, discounts, and cash purchases | 01, 02 | Core |
| 04 | Skills, offline/menu timing, and ad retirement | 02, 03 | Core |
| 05 | Talent, draw credits, and Kaju Katli wallet | 01, 02, 03, 04 | Core |
| 06 | New prestige and rebuilding | 02–05 | Core |
| 07 | Reviewed screens and runtime UI | Wireframes can follow 01; integration needs 03–06 | Player experience |
| 08 | Shop/product visuals, onboarding, and feedback | 06, 07 | Player experience |
| 09 | Real IAP fulfillment and premium shop | Authority design follows 02; integration needs 05, 07 | Monetization |
| 10 | Integrated balance and regression evidence | Start checks within each phase; finish after 03–09 | Validation |
| 11 | Device playtests, tuning, and release preparation | 08–10 | Before release |

First delivery milestone: phases 00–06 provide a save-safe, testable core loop. Then connect reviewed UI for a playable prototype, followed by art and sandbox IAP. Checks accompany implementation; phase 10 consolidates them rather than postponing all tests until the end.

## Current project starting points

Read-only inspection found these existing systems; names below are actual files, not promised new modules.

| Area | Existing entry points | Required transition |
|---|---|---|
| Economy and definitions | [Catalog.cs](../Assets/Game/Core/Catalog.cs), [GameEngine.cs](../Assets/Game/Core/GameEngine.cs), [Amount.cs](../Assets/Game/Core/Amount.cs) | Current catalogue has 96 upgrades, fixed prestige eligibility, and Legacy multipliers. Extend the existing large-number type and replace the old economy rules. |
| Authored content | [GameContent.cs](../Assets/Game/Runtime/GameContent.cs), [GameContent.asset](../Assets/Resources/GameContent.asset), [BuildGame.cs](../Assets/Game/Editor/BuildGame.cs), [ContentCatalog.json](../Documentation/ContentCatalog.json) | Keep runtime, authoring, generated asset, and simulator export aligned. BuildGame currently creates content only if absent and fills old Legacy defaults. |
| State and saves | [GameState.cs](../Assets/Game/Core/GameState.cs), [SaveStore.cs](../Assets/Game/Core/SaveStore.cs), [CloudSaveContracts.cs](../Assets/Game/Core/CloudSaveContracts.cs), [AccountSession.cs](../Assets/Game/Core/AccountSession.cs) | Current schema validates 12 Legacy perks, old IDs, and limited onboarding states. Version and migrate deliberately. |
| Screens and lifecycle | [GameController.cs](../Assets/Game/Runtime/GameController.cs), [GameView.cs](../Assets/Game/Runtime/GameView.cs), [ImportedHomeScreen.cs](../Assets/Game/Runtime/ImportedHomeScreen.cs), [ImportedPrestigeScreen.cs](../Assets/Game/Runtime/ImportedPrestigeScreen.cs) | Replace Legacy navigation/rewards, add Upgrades/Talent, preserve account/recovery routes, and separate browsing from suspension. |
| Offline and sponsors | [ImportedOfflineScreen.cs](../Assets/Game/Runtime/ImportedOfflineScreen.cs), [ImportedSponsorScreen.cs](../Assets/Game/Runtime/ImportedSponsorScreen.cs), [RewardedAds.cs](../Assets/Game/Runtime/RewardedAds.cs) | Normal offline credit already settles in the engine; remove the ad offer and prevent double credit. Remove sponsor income/entry points. |
| Purchases and cloud | [manifest.json](../Packages/manifest.json), [CloudSaves.md](../Documentation/CloudSaves.md), [FirebaseSDK.md](../Documentation/FirebaseSDK.md) | Unity Purchasing 5.4.3 is already declared. No premium fulfillment adapter was found in Assets/Game. Existing cloud saves are client-authored snapshots, not an authoritative purchase ledger. |
| Validation | [EconomyLab](../Tools/EconomyLab/Program.cs), [CloudSaveLab](../Tools/CloudSaveLab/Program.cs), [ValidateGame.cs](../Assets/Game/Editor/ValidateGame.cs), [SmokeHarness.cs](../Assets/Game/Runtime/SmokeHarness.cs) | Reuse these tools; replace old 96-upgrade/Legacy assertions while retaining unrelated regression coverage. |

Preserve unrelated workspace changes. In particular, the existing Firebase repository-path fix is unrelated to this redesign. Reuse the local Firebase SDK and dependency manager described in [AGENTS.md](../AGENTS.md) and FirebaseSDK.md if integration requires SDK work.

## 00 — Baseline, versions, and existing players

- [ ] **RR-00-01 — Record the starting baseline.** Capture current branch/status, content/economy/schema versions, and the existing test results in an implementation evidence log. Record known failures without disguising them as redesign regressions.
- [ ] **RR-00-02 — Establish an isolated development route.** Use disposable guest/account test profiles and a development build/configuration so a partial redesign never rewrites real player saves or runs against the live economy by accident.
- [ ] **RR-00-03 — Inventory old-save compatibility.** Inspect representative guest saves, backups, cloud checkpoints, pending uploads, old unlockable IDs, reputation/Legacy ranks, settings, names, and any real entitlements. Use sanitized fixtures; do not assume there are no existing players.
- [ ] **RR-00-04 — Specify the transition policy.** Define exact mappings, retained fields, and recovery behavior. Old reputation must not silently become new Fame or random cards. If deployed progress needs compensation or reset, prepare a concrete user-reviewed mapping; a test-only reset is allowed only for identified disposable fixtures.
- [ ] **RR-00-05 — Version the new economy and saves.** Define accepted old/new versions, rejection of unsupported future versions, backup-before-migration, and rollback behavior. Document what a previous app version sees after a migration.
- [ ] **RR-00-06 — Establish one implementation specification.** Record approved decisions, tunable defaults, module boundaries, and evidence locations. Do not introduce alternative prestige gates, automatic buying, or extra currency sources while implementing.

**Done when:** implementation has a versioned baseline and a testable save-transition specification. Any deployed-save conversion decision is resolved before shipping that conversion; it does not block isolated core development.

## 01 — Catalogue and numeric foundations

- [ ] **RR-01-01 — Define typed content schemas.** Cover all cash effect families, prerequisite IDs, product/skill targets, cumulative discounts, signature flags, 28 card definitions, and premium pack metadata. Map PHU/ANA/CHA/ROC/SMC/STR/AER/BOX to the existing runtime product IDs explicitly.
- [ ] **RR-01-02 — Import all 1,000 cash entries.** Preserve exact IDs, names, individual requirements, and the current six-significant-digit base prices. Use the selected late band-opening ratio x31. Store no prestige-count or real-money access condition on a cash row.
- [ ] **RR-01-03 — Make content generation repeatable.** Update the authoring/generation route, serialized GameContent asset, exported catalogue, and simulator input together. Detect an outdated asset rather than silently falling back to the old 96-entry catalogue or repopulating Legacy.
- [ ] **RR-01-04 — Validate content before use.** Enforce 1,000 unique IDs; 480 PP, 128 PC, 120 tap-profit, 40 tap-cost, 80 global-profit, 32 named-upgrade-cost, 72 skill, 32 link, and 16 group entries. Check 80 signatures, three skill unlocks included in the 72, all prerequisites present and acyclic, valid targets, and positive finite prices.
- [ ] **RR-01-05 — Audit numeric ranges and boundaries.** Reuse Amount for growing cash/rates. Choose exact checked representations for KK, credits, copies, counters, and receipts. Implement Fame/reward calculations without unsafe double-to-integer casts, reward truncation, NaN/Infinity, or an accidental gameplay cap on card levels.
- [ ] **RR-01-06 — Index and cache definitions.** Avoid rebuilding Catalog.Create or scanning the full catalogue inside every validation/income/UI operation. Keep exact raw values separate from abbreviated display values and provide enough effect precision to show duplicate improvements.

**Done when:** every authored row and card is reachable through a stable ID, generated outputs agree, and data validation catches invalid definitions without touching live saves.

## 02 — Player state, durable operations, and save migration

- [ ] **RR-02-01 — Add persistent progression state.** Store Fame, completed prestiges, all 28 card levels, KK balance, unused free credits, the set of cash IDs ever bought (U), second-prestige gift delivery, learned tutorial topics, and retained decoration progress.
- [ ] **RR-02-02 — Define run-local state and creation.** Store current purchases, product/tap levels, owned skills, timing, eligible earned cash E, and the fixed run target T. A new account starts with enough cash for the current first Phuljhari quote, with no product bought automatically.
- [ ] **RR-02-03 — Migrate and validate guest saves.** Implement the phase-00 policy with backups and recovery fixtures; distinguish missing defaults from malformed values. Do not discard an old save just because its old IDs/Legacy fields fail the new validator.
- [ ] **RR-02-04 — Extend cloud payloads and checkpoints.** Update serialization/version checks, immutable pending snapshots, interrupted acknowledgments, account replacement, and recovery paths. Preserve UID/session-generation fencing and guest/account separation.
- [ ] **RR-02-05 — Commit important actions once.** Make cash batches, prestige rewards/reset, free-credit consumption, currency-funded draw results, and one-time gifts durable operations. A crash or retry must restore the same committed outcome without losing payment, duplicating rewards, or rerolling a draw.
- [ ] **RR-02-06 — Preserve account mutation gates.** Every new command respects CanMutate/session ownership. Account switching or device takeover invalidates stale quotes/callbacks; recovery copies cannot overwrite newer progress or restore already-spent premium currency.
- [ ] **RR-02-07 — Test state round trips and transitions.** Cover fresh/old/new/corrupt saves, backup recovery, reload after each critical action, guest/account transfer, and a first purchase followed by prestige and rebuy. U must advance exactly once per ID.

**Done when:** new state survives local/cloud recovery, migrations are repeatable, and operations either commit once or leave the prior valid state intact.

## 03 — Products, tapping, and cash purchases

- [ ] **RR-03-01 — Implement product costs and normal production.** Use base cost 30×2^i, base income 0.85×1.6^i, level growth 1.12, the specified milestone multipliers, and sequential family availability. Replace the old first-unit tutorial discount with the confirmed matching cash grant.
- [ ] **RR-03-02 — Apply cash and permanent effects correctly.** Implement supporting/signature profits, global profit, links, group bonuses, and category-specific discounts. A signature replaces its ordinary effect; cumulative discount ranks replace their prior target; independent remaining-price factors multiply.
- [ ] **RR-03-03 — Implement direct and production-linked tapping.** Port B, A, M, and C exactly from the economy specification. Change tap-level price growth from the current 1.17 to 1.12. Apply global/Fame/card factors once, preserving the distinction between manual and Helping Hands payouts.
- [ ] **RR-03-04 — Implement manual input and quantity controls.** Support x1/x10/x100/Max for product and tap levels. Prototype the specified one-full-value-tap-per-1/3-second ceiling without banked idle allowance; UI taps do not earn cash and Helping Hands has its own cadence.
- [ ] **RR-03-05 — Implement prerequisite-aware purchases.** Check ownership, required levels, and predecessor IDs; reject already-owned/unknown/blocked rows. Buying a new ID changes run ownership and U; rebuying changes only run ownership.
- [ ] **RR-03-06 — Implement scoped Buy All quotes.** Snapshot the available cash budget and active filters, choose eligible entries in stable cheapest order, recalculate after discounts/prerequisite unlocks, then freeze the list for review. Commit only that reviewed list once; do not spend KK, credits, or money on product/tap levels.
- [ ] **RR-03-07 — Verify pricing and value boundaries.** Test exact affordability, insufficient cash, sequential versus bulk costs, Max near a milestone, combined discounts, new eligibility inside a batch, stale batch state, and high-level numeric behavior. Provide per-family contribution and before/after projections for the UI.

**Done when:** all cash effects and buying routes agree with the specification, never purchase automatically, and never impose completed-prestige access requirements.

## 04 — Skills, offline income, menus, and removal of ad rewards

- [ ] **RR-04-01 — Implement all 72 skill entries.** Include the three base unlocks in that count. Separate power, duration, and cooldown ranks; a permanent skill card has no effect until the base skill is bought in the current run.
- [ ] **RR-04-02 — Implement the skill interactions.** Rush changes products and gives the separate x1.25 tap/Hands bonus; the linked tap reference and Bulk Order use normal P. Apply timing/payout cards once and begin cooldown at activation. Test overlap and expiry at frame boundaries.
- [ ] **RR-04-03 — Implement offline efficiency/storage cards.** Preserve the 50%/8h baseline and the specified card curves. Use the departure normal product rate, exclude temporary boosts/skills, and perform no offline buying, tapping, or resets.
- [ ] **RR-04-04 — Settle offline income once before interaction.** The engine already credits the base reward: adapt that path rather than awarding it again from a new summary. Persist payout/time together, settle before a prestige preview, and test duplicate focus/resume callbacks and long/negative clock deltas.
- [ ] **RR-04-05 — Separate browsing from true suspension.** Let production and cooldowns continue in ordinary menus and Buy All review while gameplay tap input is blocked there. Keep actual background, account/ownership loss, and save-recovery blocks intact. Preserve the reviewed purchase list as passive cash changes.
- [ ] **RR-04-06 — Retire sponsor and rewarded-ad behavior.** Disable initialization/entry points and remove sponsor multipliers, offline-double offers, reward buttons, and stale callback grants from this version. Explicitly handle old sponsorUntil/pending-ad state during migration; old bonuses must not leak into the new model.
- [ ] **RR-04-07 — Update ad-dependent wiring and checks.** GameView currently requires a Sponsor screen, and Home has sponsor controls. Remove those requirements and update authoring/smoke checks. Audit whether ad SDK/consent dependencies can be removed safely; preserve the shared dependency manager used by Firebase.

**Done when:** skills and offline rewards match normal-income rules, menus behave as designed, and no ad route can grant income in the redesigned version.

## 05 — Talent, draw credits, and Kaju Katli

- [ ] **RR-05-01 — Create the 28-card definitions.** Use all accepted IDs/names, targets, tier assignments (16/10/2), and level-effect curves. Cards are permanent, automatically applicable, and have no equipment slots.
- [ ] **RR-05-02 — Implement common draw resolution.** Use 75/22/3 tier odds, then uniform identity selection within tier, for both free and currency-funded draws. No pity, tier guarantee, escalating cost, or ownership-dependent odds. Use injectable seeded randomness for tests; keep recorded test seeds out of live draw selection.
- [ ] **RR-05-03 — Implement copy-to-level progression.** First copy sets level 1; every duplicate adds exactly one level immediately. Persist the result before the reveal and show current→next effects with sufficient precision. A reopen must replay a result rather than draw again.
- [ ] **RR-05-04 — Add separate balances and actions.** Free credits persist and are not KK. Currency-funded draws cost exactly 100 KK; debit and grant are one durable operation. Lock Talent until first prestige and prevent the last free draw button from silently becoming a paid action.
- [ ] **RR-05-05 — Add the one-time gift path.** After the second completed prestige, grant 100 KK once and retain a durable delivery marker. First prestige remains exactly three free credits; there are no recurring grants, achievement rewards, or ad payouts in this scope.
- [ ] **RR-05-06 — Validate the collection and wallet.** Test each card's target/curve, duplicate behavior, insufficient balances, repeated taps, interrupted reveals, multiple draws, missing skill ownership, high card levels, account switches, and the expected distribution with a statistical tolerance rather than demanding exact counts.

**Done when:** free and currency-funded draws share the same pool, copies/levels and balances stay consistent through recovery, and all permanent effects compose correctly with phase 03/04.

## 06 — Fame, prestige rewards, and rebuilding

- [ ] **RR-06-01 — Implement run eligibility and income.** Freeze T=min(1e36,1e10×1.5^N) at run start. Use Fame gain=floor(sqrt(E/T)); permanent multiplier is 1 at F=0, otherwise 6+3×(sqrt(F)−1). Remove the current all-eight ownership gate and old reputation/Legacy calculations.
- [ ] **RR-06-02 — Implement free-draw rewards.** First completed prestige gives exactly three credits. Later rewards are 1+floor(log4(E/T)), plus floor(U/250) only at E>=4T. Rebuys, product/tap levels, Fame, and card levels never increase U.
- [ ] **RR-06-03 — Implement deterministic previews.** Show current/next T, Fame gain and resulting income, draw components, and the next Fame/draw/catalogue-bonus milestones. Validate run/quote identity on confirmation; ordinary accrual must not create an unconfirmable preview-refresh loop.
- [ ] **RR-06-04 — Commit rewards and reset together.** Clear cash, product/tap levels, run upgrades, skill ownership/effects, and run earnings; advance N and award Fame/credits once. Retain cards, KK, unused credits, U, learned topics, settings/name, and earned decorations.
- [ ] **RR-06-05 — Rebuild with a manual first purchase.** Calculate the new first-Phuljhari grant using current permanent discounts; do not count it in E. Clear product displays, retain decorations, and begin repurchased base skills ready as the current prototype default.
- [ ] **RR-06-06 — Test prestige thresholds and recovery.** Cover just below/at T, 4T, 9T, 16T; U=249/250/499/500/749/750/999/1000; high/deep earnings; same quote twice; crash between reward/reset; second-prestige gift; and earned-versus-spent cash. Confirm the minimum opening x6 does not multiply another x6 on every reset.

**Done when:** first and later resets are deterministic except for subsequently chosen draws, with no lost/duplicated rewards, no hidden family/count gate, and a valid rebuilt state.

## 07 — Reviewed screens and UI integration

Follow the per-screen workflow in [UI Builder/AGENTS.md](../UI%20Builder/AGENTS.md): editable game-specific wireframe → user confirmation → Gouache Storybook art reference → confirmation → editable HTML when directed → separate user-directed Unity reconstruction after HTML review. Existing Home/style approval does not approve a new screen. This checklist creates no new screen approval; record each approved revision when that stage is performed.

- [ ] **RR-07-01 — Inventory and sequence the screen work.** Cover Home/navigation, unified Upgrades and details, Buy All review, Talent collection/details/reveals, Prestige/reset, KK shop, offline return, and skill/product details. Identify reusable approved components; preserve account/settings/save-recovery access.
- [ ] **RR-07-02 — Complete each required visual-review stage.** Track screen, wireframe revision/confirmation, art revision/confirmation, HTML review, and Unity reconstruction direction. Do not pre-build downstream screen artifacts while waiting for an earlier stage.
- [ ] **RR-07-03 — Implement the Upgrades browser.** One catalogue with category/product/skill filters, text search, Available/Upcoming/Owned/Full views, requirements, first-purchase/rebuy labels, and retained filter/scroll state. Expose all 1,000 choices through pooled/virtualized rows.
- [ ] **RR-07-04 — Implement details, Buy All review, and goals.** Show accurate before/after normal income, product contribution, affected cost category, or skill timings. Show batch scope/cost/count/first-purchase changes; keep one optional pinned goal with useful alternatives and navigation to missing requirements.
- [ ] **RR-07-05 — Implement Talent and Prestige flows.** Include the locked first-prestige state, tier/individual odds, levels, free and paid actions, recoverable skippable reveals, guaranteed versus random rewards, reset/keep summary, and next reward milestones.
- [ ] **RR-07-06 — Replace Legacy and sponsor navigation.** Remove references that require the old screens; connect Home/Upgrades/Talent/Prestige and the KK wallet/shop. Update GameController, GameView, saved prefabs/scenes, and authoring scripts together so regeneration preserves the new UI.
- [ ] **RR-07-07 — Validate interaction and rendering.** Verify TMP text/rupee glyphs, safe areas, long names, large-number expansion, keyboard/back routes, loading/error/empty states, touch targets, reduced motion, scroll performance, and no leaked gameplay taps. Refresh relevant state without rebuilding 1,000 rows every frame.

**Done when:** approved screens are editable native UI, preserve existing account/recovery flows, and their displayed quotes and outcomes agree with the engine.

## 08 — Visual progression, onboarding, and feedback

- [ ] **RR-08-01 — Review reuse and author the shop layers.** Build toward six prototype stages at U=0/100/250/500/750/1000 using the same shop, with stable product/tap positions. Thresholds are presentation defaults; changing them does not change the economy or draw thresholds.
- [ ] **RR-08-02 — Create the eight families' product displays.** Plan four owned looks per family (32 total) at ownership and signatures I/IV/X, plus shared empty/locked states. Reuse suitable existing art; follow the applicable approved reference workflow.
- [ ] **RR-08-03 — Bind permanent versus run visuals.** Retain earned shop layers, reset product displays, and restore a display only when its run requirement is met. Handle loading and several crossed milestones without replaying every old celebration.
- [ ] **RR-08-04 — Add compact milestone feedback.** Surface old-product comebacks, new versus rebought upgrades, skill overlap, first prestige, and card level changes. Buy All uses aggregated/skippable feedback; reduced motion and muted sound remain usable.
- [ ] **RR-08-05 — Implement contextual onboarding.** Teach the first manual product purchase, taps, levels, filters/Buy All, skills, comebacks, prestige, free draws, duplicates, and the second-prestige KK gift when relevant. Keep topics skippable and learned state persistent.
- [ ] **RR-08-06 — Implement the return summary.** Show absence, credited/capped time, efficiency, amount, and a next action after the one-time credit has settled. No ad button or lost reward on close; skill readiness and rebuild guidance remain understandable.

**Done when:** shop growth is visible, resets distinguish retained decorations from rebuilt products, and opening/return sessions teach the loop without replaying forced tutorials.

## 09 — Real-money purchases and premium delivery

These are future implementation tasks. Do not create live products, charge a payment, publish backend changes, or claim platform approval while merely completing this planning document.

- [ ] **RR-09-01 — Design durable purchase ownership and recovery.** Define guest/account purchase ownership, reinstall/restore behavior, and the verified purchase/KK/draw ledger before enabling payments. Existing Firestore JSON saves are client-authored and cannot alone prove a paid entitlement; use a trusted receipt-verification and grant path suited to the supported storefront.
- [ ] **RR-09-02 — Define pack products and store metadata.** Use the confirmed amounts/prices below. Verify available INR price points and supported storefronts when configuring them; map stable product IDs and show localized prices returned by the store. Document any platform-mandated difference before release.
- [ ] **RR-09-03 — Integrate the existing purchasing package.** Inspect installed Unity Purchasing 5.4.3 APIs and use current official documentation during implementation. Handle initialization, unavailable products, pending/deferred purchases, cancellation, and errors; do not reinstall or upgrade packages without a need.
- [ ] **RR-09-04 — Verify and fulfill each transaction once.** Persist verified grant/transaction identity before acknowledgment/consumption, retry uncertain outcomes safely, bind delivery to the correct player, and reconcile refund/revocation outcomes. Deduplication must survive reinstall and not reuse the old bounded ad-receipt list.
- [ ] **RR-09-05 — Keep paid currency and draw outcomes consistent across devices.** Prevent stale snapshots, retries, or interrupted draws from restoring spent KK or rerolling a paid result. Keep account/session fencing and offline guest behavior explicit; do not silently require all gameplay to become online-only.
- [ ] **RR-09-06 — Finish the reviewed KK shop.** Display balance, exact amount and real-money price, fixed draw cost, and pool odds. Provide honest loading/pending/failure states. No fake scarcity timer, guaranteed rarity claim, new premium sink, or automatic spending of purchased KK.
- [ ] **RR-09-07 — Complete sandbox and recovery tests.** Cover every pack, duplicate callbacks, verified versus invalid receipts, network loss, kill/relaunch before acknowledgment, account switching, guest recovery, and restoring already-fulfilled consumables without granting them twice. Keep fake test grants isolated from release builds.

| Pack | Kaju Katli | Draws at 100 KK | Confirmed INR design price |
|---|---:|---:|---:|
| Small box | 1,000 | 10 | ₹99 |
| Festival box | 6,000 | 60 | ₹499 |
| Celebration box | 13,000 | 130 | ₹999 |
| Grand celebration box | 28,000 | 280 | ₹1,999 |

**Done when:** sandbox payments deliver the correct currency exactly once and purchases survive interruption/recovery; real store configuration and device results are recorded rather than assumed.

## 10 — Integrated regression and economy validation

- [ ] **RR-10-01 — Update existing test expectations deliberately.** Replace 96-entry/12-Legacy/ad-reward assertions with the new contracts. Preserve unrelated large-number, settings/name, quantity controls, save recovery, cloud ownership, and UI regressions.
- [ ] **RR-10-02 — Check runtime/model parity.** Use the same authored content and fixed test inputs in the C# core and the [Markdown model](Economy%20Model%20v1.md). Compare quotes, income sources, discounts, skill timings, draw/reward boundaries, U, and reset state before comparing long-run completion.
- [ ] **RR-10-03 — Rerun the published policy suite.** Reproduce the 32 free histories, 16 paired histories per pack, one-/two-/three-visit and 15/20/25-minute patterns, no-tap, deep-run, no-reset, Common-only, and no-Legendary cases. Preserve active management/reveal time and real visit gaps.
- [ ] **RR-10-04 — Exercise meaningful failure boundaries.** Test stale purchase/prestige quotes, lost session ownership, duplicate gift/credit/draw delivery, save corruption, interrupted migration, numeric overflow, and unknown content versions. Record expected behavior, not tests that merely mirror an implementation formula.
- [ ] **RR-10-05 — Run Unity and cloud integration checks.** Update ValidateGame, relevant SmokeHarness coverage, CloudSaveLab, and rules tests if schemas/services change. Use temporary profiles and the demo emulator target for destructive/recovery tests; never the production database.
- [ ] **RR-10-06 — Profile the full catalogue and repeated runs.** Measure list scrolling, Buy All, income recalculation, save size/serialization, cloud payload limits, and memory across prestige/reveal cycles on target hardware. Remove repeated allocation/full-catalogue work where it causes visible stalls.
- [ ] **RR-10-07 — Publish reproducible evidence.** Export the selected content version/hash, commands, seeds, policies, outcomes, tolerances, and limitations. Update the simulation report if runtime differences or tuning change the result; never present the old 86-day model median as new human-play evidence.

**Reference checks, not promises:** first prestige 6–10 elapsed mixed hours or 2–3 sustained hours; reference about 8h / 2h 6m. Matched x6 rebuilding is about 3.92x faster mixed and 5.89x sustained. The current three-visit/20-minute free cohort completes on days 75/86/98 (10th/median/90th) with average 1.23/1.38/1.53 resets per day over days 31–90. Actual return gaps can make the first rebuild take 24 elapsed hours; do not force the matched diagnostic into every schedule.

**Done when:** runtime behavior agrees with the intended rules, relevant regressions pass, and all pacing claims have reproducible inputs and clear limits.

## 11 — Player testing and release preparation

- [ ] **RR-11-01 — Observe uncoached opening and first-rebuild sessions.** Check whether players understand manual purchase, cash upgrades versus Talent, Fame versus credits versus KK, and reset/keep rules. Include short check-ins and longer active sessions.
- [ ] **RR-11-02 — Review the known pacing risks.** Test the early arrival of all eight products, the roughly 13.7-active-minute gap between named cash purchases, meaningful old-product comebacks, deep-run advantages, and the usefulness/readability of Common duplicates.
- [ ] **RR-11-03 — Tune only with recorded evidence.** Adjust prototype coefficients, visual thresholds, or detailed UX where tests justify it; rerun affected comparisons. Keep confirmed mechanics intact and explicitly discuss any proposed structural change.
- [ ] **RR-11-04 — Complete real-device acceptance.** Test target aspect ratios/performance, background/resume, sign-in and guest paths, two-device handoff, connectivity loss, save migration/recovery, and sandbox billing on the intended distribution/signing configuration. Preserve prior device results while adding evidence for the redesigned build.
- [ ] **RR-11-05 — Prepare the release package and disclosures.** Review platform requirements at implementation time, odds/price text, purchase failure/support flows, privacy copy after ad removal, and production configuration. Existing analytics collection settings remain unchanged unless an explicit instrumentation/privacy decision calls for a change; local test diagnostics suffice for prototype evidence.
- [ ] **RR-11-06 — Prepare rollout and recovery.** Record supported save versions, migration fixtures/results, content version, release notes, outstanding limitations, and a rollback plan that preserves paid entitlements and cannot overwrite migrated saves. Keep experimental/reset/debug-grant paths out of release builds.
- [ ] **RR-11-07 — Close release acceptance.** Require successful core/save/UI regressions, device and purchase evidence, reviewed screen assets, and resolution of player-facing blockers. Publishing a build or deploying production services is a later user-directed action, not completion of this TODO-writing request.

**Done when:** the redesigned game is validated on its intended devices and storefront, and its release/migration package is reviewable.

## Explicitly outside this implementation scope

Achievements and daily/weekly objectives/rewards; multiplayer/country wars; rewarded ads; extra currencies or premium uses; paid automation; card pity/merging/equipment; new product families; prestige-count content gates; a redesign of unrelated account/authentication features. Keep necessary compatibility fixes small and tied to the redesign.

## Tracking

For a completed task, add its evidence beside the checkbox. Record blockers with the task ID and the smallest missing dependency. Keep confirmed design decisions in the decision register; keep test observations and coefficient changes in the simulation/validation reports. This prevents implementation status, design approval, and shipping validation from being confused.
