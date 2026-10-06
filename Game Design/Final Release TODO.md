# Rocket Raja — final release TODO

Updated: **6 October 2026**. Target: the approved **Classic UI on Android**, connected to the existing game and startup systems.

This is the remaining release checklist. Existing core features should be reused; a checked task in the earlier implementation plan does not establish that its integration into Classic or its final device acceptance is complete. Mark an item complete only with a source change/configuration record and relevant evidence against the release candidate. No completion percentage is assigned yet.

The working release scope includes guest play, Google/cloud progress, and the planned KK shop. Real billing was deferred for the prototype and remains unfinished release work. If the first public release will omit accounts or real purchases, record that scope decision and remove their unavailable entry points before treating the corresponding release gates as deferred.

## Existing work to build on

- The same versioned engine/content powers Classic and redesigned Main: eight products, 1,000 cash upgrades, three skills, Fame/prestige, 28 Talent cards, and offline income.
- The approved Classic hierarchy, product/card artwork, dialogs, controls and native animation system are saved in `HTML Importer.unity`. Previous Editor interaction and motion checks are recorded in [Classic UI import](../Documentation/Implementation/Classic-UI-Import.md).
- The separate Classic development APK installed and ran on the POCO X2. Manual navigation, purchases, free/paid five-card grants and rotating reveals were observed with zero probe-reported Unity runtime errors. This was a boosted test profile, not fresh-player or release acceptance. See [actual Android results and untested cases](../Documentation/Implementation/ClassicAndroid/DeviceTest.md).
- Bootstrap account/session coordination, save recovery, guidance, audio and visual progression have existing implementations to integrate. Premium service/client code has local evidence but is not deployed or store-accepted. See the [existing implementation index](../Documentation/Implementation/README.md).

## 1. Connect Classic to the actual game — first priority

Implementation update, 6 October 2026: Bootstrap/Main now use the saved Classic production hierarchy and selected guest/account driver. Native startup/recovery/account fixtures and core checks pass; see [integration changes and evidence](../Documentation/Implementation/Classic-Production-Integration.md). Checkboxes below remain open until their required device/store acceptance is recorded. The confirmed Android package for this work is `com.diwali.fireworkstycoon.dev`.

- [ ] **REL-01 — Establish one normal startup route.** Make Bootstrap enter the approved Classic presentation through the intended production scene/prefab. Verify Editor Play, a cold Android launch and a store-installed launch reach the same intended flow. Remove reliance on the local Editor Play start-scene override. Preserve the previous UI as inactive compatibility content while references are migrated.
- [ ] **REL-02 — Bind the selected session.** Connect Classic to the existing guest/account session selected by Bootstrap, including return-to-account options, reconnect and sign-out. Retire `ClassicPreviewSession` from the release startup path; there must be one live gameplay session and one owner of saving/time settlement.
- [ ] **REL-03 — Restore normal starting progress.** Start a new player with the designed first manual Phuljhari purchase grant, no automatically owned products, Fame, cards or test currency. Load returning progress from the intended persistent profile. Keep boosted fixtures confined to development tools and verify they cannot enter a release or real account.
- [ ] **REL-04 — Integrate startup and recovery states.** Wire initialization, loading, failed load/save, retry, account options, offline settlement and background/resume to the existing owners. Block gameplay when the session cannot safely mutate. Do not fall back to a new save when existing progress cannot load.
- [ ] **REL-05 — Reconcile every approved screen and action.** Audit the connected Classic flow against the latest HTML/user decisions, including four navigation tabs, direct purchases, Buy All/next ten upgrades, product milestone bars, inline four-column Talent cards, disabled unowned cards, outside-card dismissal and Continue behavior. Keep removed upgrade-detail/collection popups removed. Account/service screens must perform their actual action rather than preview navigation.

Completion evidence: cold launch → choose/load profile → Classic Home → gameplay → account/recovery → return works with the approved UI and saved hierarchy. No runtime UI factories or growing effect pools are introduced.

## 2. Complete game design and presentation integration

Implementation update, 6 October 2026: Classic now has saved contextual guidance, signature-based product appearances, retained decoration layers, music/action audio, and touch/accessibility refinements. Unboosted engine and native Classic replays cover two prestiges and retained-progress reload; see [presentation integration and evidence](../Documentation/Implementation/Classic-Presentation-Integration.md). REL-06–11 are complete against the installed development candidate, with user-confirmed audio/touch checks and a retained-progress cold restart. Exact APK identity and evidence are linked in that report. The unboosted runs use simulated time; final non-development/store acceptance, broader device coverage and uncoached balance sessions remain in sections 5–6.

- [x] **REL-06 — Verify the final economy/content binding.** Validate the content manifest, all 1,000 IDs/prerequisites, eight products and 28 cards against the approved design and the exact release asset. Confirm displayed quotes, income and effects come from the same engine that commits actions.
- [x] **REL-07 — Exercise a complete fresh run and prestige rebuild.** Check first manual purchase, product unlocks, quantities, Buy All, lifetime discovery counting, skill unlock/repurchase, Fame rewards, free credits and the second-prestige gift. Verify reset/retained fields and the next run's opening cash. Cash upgrades must keep the approved cash-only prerequisites.
- [x] **REL-08 — Restore contextual onboarding and guidance.** Adapt the existing learned-topic/tutorial system to Classic. Cover first purchase/tap, upgrades, skills, Talent, prestige and return play; persist learned topics and avoid repeating completed guidance. Static Help alone is insufficient.
- [x] **REL-09 — Restore visible progression in the approved art style.** Connect run-specific product appearances and retained shop decoration stages to their real triggers. Adapt the existing four looks per product and six shop stages to the approved Classic art/layout; do not silently revert to the rejected UI. Verify signature purchases update the artwork, prestige resets run looks, and earned shop decorations persist.
- [x] **REL-10 — Restore music and sound playback.** Place and wire the existing audio owners/sources in the saved scene. Connect tap, purchase, skill, reveal and prestige feedback; verify persisted music/sound preferences, background behavior and no duplicated playback after scene changes.
- [x] **REL-11 — Finish motion, touch and accessibility acceptance.** Verify all carried-over animation triggers, interruption/reopen behavior, reduced motion, disabled controls, legible text/contrast, scrollable short screens and touch targets. Avoid gameplay taps leaking through UI or earning cash during a drag. Verify animations never grant rewards or charge currency.

Completion evidence: an unboosted end-to-end run, retained-progress reload and prestige rebuild with correct guidance, visuals, sound and animation behavior.

## 3. Finish saves, Google accounts and cloud recovery

Implementation update, 6 October 2026: [Classic save/account integration](../Documentation/Implementation/Classic-Save-Account-Integration.md) records the retention/location policy, interrupted/future-save fixes, 702 core checks, 26 native Classic recovery checks, 79 startup/account checks and cloud/emulator regressions. The existing package/signing identity is confirmed; the updated APK built with zero errors and installed while preserving data. REL-13 awaits a visible restart check after unlocking the phone. Remote inventory and Google setup await restored Firebase console access; real two-device acceptance needs a second device. These remaining acceptance checks keep REL-12–16 open; fixture evidence does not close them.

- [ ] **REL-12 — Establish the production save/upgrade policy.** Select final profile locations and version handling; inventory any existing player data and entitlements. Complete the reviewed migration/retention mapping before enabling conversion. Preserve source saves/backups and reject unsupported future data without overwriting it.
- [ ] **REL-13 — Verify local durability through Classic.** Exercise autosave, restart, force-stop, interrupted writes, backup recovery, low storage, offline caps, clock changes and pending reveal restoration. Reuse the existing persistence tests, then prove the new screen flow presents the correct recovery state and does not duplicate a reward.
- [ ] **REL-14 — Complete Google/Firebase configuration for the final app.** Match package, signing certificates, OAuth/Firebase configuration, cloud settings and deployed rules to the final distribution. Reuse the local SDK described in [FirebaseSDK.md](../Documentation/FirebaseSDK.md). Keep setup credentials outside source control and evidence logs.
- [ ] **REL-15 — Validate account choices and ownership.** Test real sign-in/cancellation, new account, existing cloud save, explicit guest promotion, retained guest save, account switching, sign-out and reconnect through Bootstrap + Classic. Simulated-shop/test progress must remain ineligible for real account or premium promotion.
- [ ] **REL-16 — Complete two-device and failure acceptance.** Test ownership transfer, concurrent sessions, airplane mode/network loss, delayed acknowledgments, interrupted uploads, stale callbacks and expired authentication. Verify recovery uses the same account/checkpoint and cannot overwrite newer progress or duplicate offline rewards.

Completion evidence: final-package Google sign-in on physical hardware, two-device tests and local/cloud failure recovery. Fixture adapters and emulator success alone do not close these items. Existing references: [account flow](../Documentation/Implementation/AccountFlow.md), [account recovery](../Documentation/Implementation/AccountRecovery.md), [save transition](../Documentation/Implementation/SaveTransition.md).

## 4. Complete real purchases and premium authority

These items are required when the release includes real KK purchases. The current Shop is a development simulation, and the account/server draw contract currently documented is for single draws.

- [ ] **REL-17 — Finalize store products and purchase ownership.** Confirm the actual package/product IDs, pack amounts, store availability and account-link requirement. Load localized prices from the store; replace example prices and preview outcome buttons in the release UI.
- [ ] **REL-18 — Connect and deploy verified fulfillment.** Complete the configured Unity Purchasing → authenticated verification service → authoritative wallet path, including the required Firebase/App Check setup, store API access, receipt validation, once-only grants and purchase confirmation after durable fulfillment. Complete the existing [deployment review](../Documentation/Implementation/PremiumDeploymentReview.md) before activation.
- [ ] **REL-19 — Extend premium authority for the approved 5x pull.** Persist one operation and all five results atomically. Verify 1x uses one free pull when available, otherwise 100 KK; 5x uses four free pulls when available, otherwise 400 KK, with no mixed payment. Account-funded batches must use the authoritative wallet and retain identical results across retries, device handoff and interrupted reveals.
- [ ] **REL-20 — Finish purchase and refund recovery.** Verify pending/cancelled/failed purchases, duplicate callbacks, unknown network outcomes, kill/relaunch before confirmation, account changes, fulfilled consumable recovery and refunds/revocations. Restore balances from the verified ledger, not from an old client snapshot. Configure retry/alerting and refund reconciliation operations.
- [ ] **REL-21 — Complete sandbox billing acceptance.** Exercise every configured pack on the intended Play test track with license testers and final signing configuration, including the recovery cases above. Record actual store evidence; simulated packs or fabricated receipts cannot close this item.

Completion evidence: real store sandbox purchases, authoritative 1x/5x spending, once-only fulfillment and recovery on the final app identity. Reuse the [premium contract](../Documentation/Implementation/PremiumAuthority.md); do not rebuild already tested modules without a demonstrated gap.

## 5. Complete integrated device, performance and player tests

- [ ] **REL-22 — Close the unfinished Classic phone checks.** Specifically test Home counter taps and rate limits, drag rejection, reduced motion, early reveal close/interruption, app background/resume and force-stop/relaunch persistence. Capture the actual result on the phone; these were not completed in the first manual session.
- [ ] **REL-23 — Test the complete screen/state matrix.** Cover navigation/back, keyboard/name entry, long names, rupee/large-number rendering, loading/error/empty/locked states, product and tap purchases, Buy All, skills, cards, prestige, offline receipts, settings and account/shop routes. Include fresh, established and late-game saves on short/tall/notched devices.
- [ ] **REL-24 — Profile sustained gameplay on target devices.** Set the target frame-time, memory and load-time budgets, then measure scrolling, income updates, Buy All, saves/cloud payloads, repeated prestige/reveal cycles, thermal/battery behavior and memory growth on representative Android hardware. Investigate the observed frame dips and low-memory callbacks with release-like profiling; the development median FPS is not acceptance.
- [ ] **REL-25 — Run uncoached balance and usability sessions.** Measure first purchase, first prestige/rebuild, meaningful upgrade gaps, old-product relevance, deep-run value and duplicate-card clarity. Record whether players understand Fame, free pulls and KK. Tune only from evidence and rerun affected economy comparisons while preserving agreed mechanics.
- [ ] **REL-26 — Run the final integrated regression set.** Recheck core/content, local/cloud saves, premium ownership, UI/input and motion against one exact candidate revision. Include failure injection and repeated operations. Fix player-facing blockers and triage remaining compiler, shader, icon and platform warnings with documented disposition.

Completion evidence: reproducible test results and observed player sessions linked to a specific candidate. Existing open tasks carried forward: RR-07-07, RR-09-07, RR-10-06, RR-11-01/02/03/04/07 in [Implementation TODO](Implementation%20TODO.md).

## 6. Prepare the release build and store submission

- [ ] **REL-27 — Complete release configuration and isolation checks.** Replace the current development-only startup/configuration after the integration and acceptance work above. Verify release builds contain no boosted seeds, fake purchases, development grants, read-only test probe, fixture controls, debug-only menus or disposable profile routing. Keep the release guard meaningful rather than deleting it to make a build pass.
- [ ] **REL-28 — Finalize package, signing and update compatibility.** Confirm final application ID, owned signing/Play App Signing configuration, version name/code and supported devices. Verify normal updates retain progress and cannot replace another installed game through an identity or signing workaround. Record the release content/economy/schema versions and tested source commit.
- [ ] **REL-29 — Prepare the store and player-facing release material.** Finalize icon, screenshots, description, support contact, privacy/account-deletion flows where applicable, data disclosures and content rating to match the implemented services. Verify current Play target API, billing, native-library/device compatibility and submission requirements at submission time; do not assume the current development configuration is sufficient.
- [ ] **REL-30 — Build and verify the signed release candidate.** Produce the intended non-development Android artifact/AAB from the recorded commit. Inspect its signature, manifest, permissions, included scene/content, package/version and build report. Save the artifact hash, symbols and test evidence, then install via the intended internal/closed track.
- [ ] **REL-31 — Validate clean install, upgrade and rollback preparation.** Re-run critical startup, save, account, purchase and reveal checks on the distributed candidate. Test upgrading representative existing saves without clearing data. Keep a compatible recovery plan and artifact; do not rely on downgrading away newer saves or paid ledger records.

Completion evidence: signed, reproducible candidate from the intended distribution with clean-install and update acceptance. Existing [rollout/recovery packet](../Documentation/Implementation/RolloutRecovery.md) must be updated to the final identity and Classic flow.

## 7. Release acceptance and launch

- [ ] **REL-32 — Close the release evidence review.** Confirm all applicable items above are complete, any exclusions are explicitly reflected in release scope/UI, and no player-facing blocker remains. Record known limitations, completed device matrix, final source/configuration/artifact identity and support/recovery procedure.
- [ ] **REL-33 — Publish the accepted candidate and monitor rollout.** Complete the final publication decision, use the chosen rollout plan, and monitor crashes/ANRs, save/reconnect failures, purchase fulfillment/refunds and performance. Define who responds and how to halt rollout or disable affected service actions without losing progress or entitlements.

## External inputs needed before those tasks can close

| Input or decision | Needed for |
|---|---|
| Whether public v1 includes Google accounts and real KK purchases | Applicable release scope and REL-14–21 |
| Final app/package identity, signing ownership and Play Console access | REL-14, REL-17, REL-21, REL-28–30 |
| Actual store products, test track/test accounts and private server configuration access | REL-17–21 |
| Existing-player inventory and reviewed migration/retention decision | REL-12, REL-28, REL-31 |
| Second Android device/account and representative performance-test devices | REL-16, REL-23–24 |
| Playtest participants and release/support owner | REL-25, REL-29, REL-32–33 |

Do not paste private keys, tokens, passwords or purchase receipts into this checklist. Record configuration identities and evidence locations instead.
