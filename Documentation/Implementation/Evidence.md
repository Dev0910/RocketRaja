# Implementation evidence

## Baseline — RR-00-01 (1 October 2026)

- Starting branch `main`, commit `579291f352eb39efbda7844ce55c3b4765e739be`; only tracked edit was `Assets/Editor/FirebaseRepositoryUriFix.cs`. Preserved.
- Working branch `codex/redesign-implementation`. Unity 6000.4.9f1; application 0.1.1; package `com.diwali.fireworkstycoon.dev`; economy 1.0.0; gameplay/envelope/checkpoint schema 1; 96 upgrades, 12 Legacy perks, eight products.
- `dotnet run --project Tools/EconomyLab -- --catalog Documentation/ContentCatalog.json`: **308 PASS**, exit 0 (`Baseline-EconomyLab.txt`).
- `dotnet run --project Tools/CloudSaveLab`: **28 PASS**, exit 0 (`Baseline-CloudSaveLab.txt`).
- Unity `RocketRaja.Editor.ValidateGame.Run()`: **7 PASS**, no compilation failure (`Baseline-UnitySaveTests.txt`).
- Pre-existing editor error: AI Game Developer startup complains about spaces in the project path. No game compilation error. Do not hide this tooling diagnostic or relabel it as a redesign regression.
- ADB sees an authorized POCO X2 (phoenixin); the development package is installed. No install, reset or player-save mutation performed for baseline.

## Progress

Core implementation underway. See `Specification.md` and `SaveTransition.md`. Later screens require the explicit existing review workflow. Real-player compensation, storefront configuration/sandbox purchase evidence, uncoached playtesting and two-device acceptance must be recorded honestly; they cannot be inferred from core checks.

## Foundation — RR-00-02 through RR-01-05

- Specification and exact transition/rollback contract: Specification.md and SaveTransition.md. Legacy source files remain unchanged; sanitized inventory found main/backup with 43 upgrades. Fixtures are synthetic.
- Unity-Content-Isolation.txt: 8 passing checks for authored asset/export agreement, exact Natural serialization, separate local profile/preferences and early live-cloud refusal.
- Content-Numbers.txt: 53 passing content/numeric checks. Fixture generation separately passed 2 additional original-schema checks. Source digest C0904702675993BCF0B3F647A8A1DC71E6A4FBFA5A455B97EFE58F43EFEFA5F7.
- Unity JSON roundtrip changes some doubles by one ULP; authored six-digit price strings remain exact, derived numeric parity uses 1e-14 tolerance. No gameplay cap is imposed on copies/Fame/credits. BigInteger decimal strings roundtrip through Unity; reward square roots/log thresholds avoid unsafe double-to-integer conversion.
- Existing legacy runtime remains connected while the redesigned core is developed. RR-01-03/06 remain open until runtime wiring uses the validated new asset/cache. No new screen approval or device deployment claimed.

## Core implementation — 1 October 2026

The redesigned domain implementation is `RedesignCatalog`, `RedesignState`, `RedesignEngine`, `RedesignSaveStore` and `RedesignAccountSession`. It compiles inside Unity and in the standalone .NET lab. This is the core delivery milestone; the existing Main scene still uses the old controller while the new native screens await their required reviews. Runtime integration, ad retirement, full model parity, premium authority and device acceptance remain open. The cached new core resolves definitions by stable index; RR-01-06 is now complete for the new domain, superseding the earlier progress note above.

| Task group | Deliverable and evidence |
|---|---|
| RR-02-01/02 | New persistent/run state, exact counters, manual matching first grant, fixed target, 28 card levels, lifetime U, decorations/settings/topics; fresh state and Unity roundtrips pass. |
| RR-02-03 | Checksummed old/new/future detection before full decoding; corrupt backup recovery; a restricted editor/development-only synthetic-fixture transition preserves source/archive, copies only documented settings/name, and retries into the same new run. Real player conversion stays blocked as specified. |
| RR-02-04/06/07 | Versioned payload validation before cloud replay; separate `redesignUser` collection and `economy-2` checkpoint namespace; immutable in-flight/follow-up recovery, UID/generation fencing, guest promotion timer rebasing, takeover and future-version checks. Recovered backups carry a durable draw-reconciliation block. No live service deployment. |
| RR-02-05 | Staged copies persist before the engine publishes batch/reset/draw/gift results. Injected disk failures preserve old state; failed draw retry keeps the selected identity. Same committed quote cannot spend or reset again. |
| RR-03-01/02/03/05/06 | Correct product and tap costs/profits, signatures, independent/cumulative discounts, links/groups, B/A/M/C, prerequisites/U, and frozen scoped Buy All. All 1,000 entries bought before any prestige in the test fixture. Native input controls and final value/profiling checks remain open. |
| RR-04-01/02/03 | All 72 skill entries; Rush/Hands/Bulk normal-income interactions; cooldown starts at activation; 90 Hands payouts over 30s in both large and small frame steps; 50%/8h offline baseline and card curves. Menu/focus wiring and removal of legacy ad code remain open. |
| RR-05-01–05 | All card definitions/curves; common draw pool with cryptographic live randomness and test-only seeded adapter; exact +1 copies, separate credit/100-KK actions, pending persisted reveal, second-prestige gift marker. The 250,000-draw seeded distribution lies within six-sigma per-card tolerances. This is not real-store fulfillment evidence. |
| RR-06-02/04/05 | First three credits and later reward thresholds/U bonuses; atomic permanent gains/reset; retained lifetime state and discounted manual rebuilding grant. Native previews and consolidated prestige/device acceptance remain open. |
| RR-07-01 | 13-screen inventory/state matrix and editable grayscale wireframes in `UI Builder/Wireframes/Redesign/NOTES.md`. All confirmations are pending. User review requested through the chat; no art/HTML/native reconstruction has been started for these revisions. |

Results at the initial core milestone (superseded by the expanded checks below):

- `dotnet run --project Tools/RedesignLab`: **174 PASS**, including core, migration, cloud, failure injection and seeded draw distribution (`Core-Redesign.txt`). Seed 739103; 250,000 draws; each identity checked against its tier's uniform probability at six standard deviations.
- Unity `RedesignCoreValidation.Run()`: **12 PASS** including actual JsonUtility/file writes, all 1,000 IDs, prestige/draw/offline reloads, and payload size (`Unity-Core.txt`). Re-run after changes before closing the milestone.
- Existing economy regression: **308 PASS** (`Regression-LegacyCore.txt`). Existing cloud regression: **28 PASS** (`Regression-LegacyCloud.txt`).
- Demo Firestore emulator: **14 PASS, zero failed** (`Firestore-Rules.txt`), including new collection isolation and denied access to arbitrary wallet/receipt collections. Local rules/index changes are not deployed.
- The emulator needed Java 21; a portable Temurin JRE was downloaded under ignored `Temp/RedesignTestRuntime/` using the official Adoptium API. Archive SHA256 verified as `d35f31e712f0fcf6ac5a093edc90204fbff22f720ba3950bd09d331d5e621636`. This does not change Unity's bundled Java or machine configuration. Reproduce rules with `Tools/CloudSaveTests/run.ps1 -JavaHome '<project>/Temp/RedesignTestRuntime/jdk-21.0.12.1+1-jre'`.

## Remaining integration boundaries

1. Bind the new domain to the runtime/session lifecycle and replace legacy/ad-era wiring. The Main scene remains on the old controller pending reviewed new UI integration. Do not report it as the new playable game yet.
2. RR-07-02: wireframe v1 confirmation requested; art, HTML review and separately directed Unity stages follow the recorded workflow. Review silence is not approval.
3. The undeployed trusted ledger, gift/promotion protocol, separated card/wallet cache, authenticated transport and Purchasing adapter now exist (see the expanded checks below). Paid currency still needs configured authenticated hosting and App Check, native scene/account integration, verified storefront metadata and sandbox/device evidence. Client-authored saves cannot establish paid entitlements.
4. No claim of human opening/rebuild observation, two-device acceptance, long-run runtime/model parity or new device build/install. The connected device and existing player saves are unchanged.
5. Final commit/build/replacement occur after all required implementation and acceptance tasks, as requested. Work remains on `codex/redesign-implementation`; no commit has been made yet.

## Expanded boundaries, model comparison and purchasing foundation — 1 October 2026

Checklist: 42/79 complete. New completed tasks: RR-03-07, RR-05-06, RR-06-06, RR-09-01, RR-10-02, RR-10-03 and RR-10-04. Remaining screen-dependent/runtime and store/device acceptance tasks remain open.

- `dotnet run --project Tools/RedesignLab -- --parity-export`: **361 PASS**. Every card is tested at levels 1/2/100 against independent expected target/category behavior; covers exact and adjacent representable prestige boundaries at N=0/1/7/19/147/148, capped targets, Max at level 25, high-level prices, stale batch state, lifecycle recovery and fulfillment callback fencing.
- The expanded tests found a one-ULP threshold error: a canonical 9T value could produce only two Fame. Reward arithmetic now recognizes the exact `Amount` threshold representation within the double exact-integer range, without a broad epsilon. A representable value immediately below the canonical threshold stays below; enormous ratios retain arbitrary-precision arithmetic. No economy coefficients changed.
- `node Tools/RedesignLab/CheckModelParity.cjs`: **7,744 comparisons PASS** across **87 fixed snapshots**. Numeric tolerance 1e-9; observed maximum relative difference 2.145735588296984e-15. Covers target construction, product/manual/Hands income, individual/bulk level costs, cash discounts, skill times, reward/U boundaries and reset state. Canonical saved T is shared as a fixed input for boundary checks; independently computed T is compared numerically because log/power arithmetic can differ by one ULP. The probe suppresses only the model policy's automatic post-reset draws so reset state is compared before the player's voluntary draw action. Original model formulas and source Markdown remain unchanged. Exact PRNG sequence equivalence is not claimed; the shared tier/identity mapping and statistical distribution have separate tests.
- `node Tools/RedesignLab/RunReferenceModel.cjs`: **125 saved scenarios**, followed by `CheckReferenceReport.cjs`: **28 published-result checks PASS**. The 32 free histories, 16 paired histories for each pack, 11 behavior policies, 16 no-Legendary histories and two rebuilding diagnostics reproduce the published nearest-rank results. Files retain seeds/options/events/daily data in `ReferenceModel/`. These are planning-model reruns, not a runtime long-horizon simulation or human playtest.
- Unity `RedesignCoreValidation.Run()`: **18 PASS**; `RedesignAuthoring.Validate()`: **9 PASS**. Includes the new scene-independent runtime driver, real JsonUtility saves, browsing/input separation, suspension/resume and cached content loading. Generation writes a runtime hash manifest; a build preprocessor rejects source/asset/export drift. Main scene replacement still awaits reviewed screens, so RR-01-03 and RR-04-04/05 remain open at integration scope.
- Purchasing: `PremiumAuthority.md` defines ownership, gift/promotion, restore and refund recovery. The installed Unity Purchasing **5.4.3** adapter compiles without a package change; it does not initialize until an authenticated authority is configured. Its coordinator persists/reconciles before acknowledging and fences account/generation/disposal changes. The new `Backend/Premium/` receipt/wallet/draw service, Google verification adapter and authenticated HTTP handler remain **undeployed**. **33 Node tests PASS**; all receipts/identities are synthetic.
- Demo Firestore emulator: **18 PASS**, comprising 14 save/rules checks and 4 actual Firebase Admin transaction checks. Concurrent receipt grants, stale wallet spends, restart replay, ownership timestamps and once-only revocation are exercised against `demo-rocket-raja` on localhost. No production rules or data were modified.
- Backend Admin SDK 14.5.0 is pinned with its lockfile. The gaxios transitive UUID dependency is overridden to patched 11.1.1; gaxios uses the compatible `v4()` API. `npm audit --omit=dev`: **zero vulnerabilities** (`Premium-Dependency-Audit.json`). No Unity Firebase SDK import, Unity package upgrade or machine-wide runtime change was required.

Official integration references and product configuration boundaries are linked in `PremiumAuthority.md`. API unit/emulator results do not establish actual receipt acceptance, Play price availability, distribution signing, consumed-product restore on a phone, or platform approval. The asynchronous requests for wireframe review and existing Google Play sandbox configuration are still unanswered at this checkpoint.

Additional runtime fence: account reinitialization invalidates earlier engine bindings even if a server generation is reused. The lifecycle preserves that underlying permission check rather than replacing it. Three added regression checks cover durable routing, stale bindings and ownership interruption.

Preliminary Unity editor profiling found full Buy All quotes at 160.2160 ms and state validation at 31.9364 ms. Replacing repeated scans with a dependency queue and set membership reduced the sampled means to 8.7127 ms and 1.0271 ms. Full-run JSON is 24,225 UTF-8 bytes versus the 750,000-byte limit. Repeated prestige/three-draw/rebuild averaged 9.3154 ms after the change. Before/after files are Unity-Core-Performance-Before.txt and Unity-Core-Performance.txt; different bounded sample counts are recorded, and these desktop measurements do not close target-device profiling. The first longer profiling MCP request timed out after producing its report; the shorter optimized capture and Unity validation completed normally.

## Authoritative wallet integration — 1 October 2026

Checklist now **44/79** complete; RR-09-04/05 are closed at the tested client/server implementation boundary. The native account/store flow, deployment and physical-device purchase acceptance remain open in their separate tasks. No payments, deployment, final commit, game APK build or replacement occurred at this checkpoint.

- Latest `dotnet run --project Tools/RedesignLab -- --parity-export`: **413 PASS**, including wallet component replacement, account UID/profile fences, second-prestige gift activation, spent guest gift import, stale/late responses, persisted pending operations, lost cloud/server acknowledgments, reconnect and takeover, and exact HTTP wire counters. Every account engine binding invalidates earlier bindings. Authoritative schema 3 rejects downgrade into local-only schema 2, even with a larger gameplay revision.
- `npm test` in `Backend/Premium`: **61 PASS**. Activation atomically binds a profile and imports one free gift; paid spending requires a matching cloud-confirmed operation. Includes hosting configuration, shared throttling, independently verified refund notifications, refund-before/during-grant races, exact-token pagination and malformed/irrelevant notification handling. Synthetic receipt verification, ownership, refund, replay, API authentication and failure checks remain isolated from release configuration.
- Local demo Firestore emulator: **22 PASS**, comprising 14 rules/save checks and 8 real Admin transaction tests. Added concurrent gift promotion, refusal of an unconfirmed operation, shared function-instance throttling and pre-delivery revocation; clients cannot access profile/rate/revocation records. No live Firestore change.
- Latest model comparison remains **7,744 PASS** across 87 snapshots, maximum relative difference **2.145735588296984e-15**. Wallet changes do not change authored prices or economy formulas.
- Unity: **23 PASS**, validating schema-3 wallet and paid operation/reveal persistence through actual JsonUtility, free/server copy separation, and wire revisions beyond double integer precision. Content validation remains **9 PASS**. The earlier 24,225-byte performance sample predates the optional wallet fields and is retained as dated evidence, not a new paid-state payload measurement.
- `FirebasePremiumConnection` compiles against the already-installed Auth/App Check 13.17.0 packages. It requires an explicit HTTPS endpoint, valid Google owner, ID and App Check headers, no redirects, bounded request/response sizes and a 20-second request timeout. Provider initialization/console acceptance is still required; no endpoint is configured. The transport does not turn network/5xx failures into definitive rejected draws.
- No game compiler failure was observed. Editor logs still include the existing MCP package warning about spaces in the project path and Unity importer graphics API dependency-mask messages during refresh. These are recorded rather than reported as a completely empty error console; they did not fail compilation or the validation run.

Compatibility finding fixed during this work: Unity JsonUtility expands a null inline cache into an empty object. Authority is therefore determined by explicit schema 3 plus validated contents, never object non-nullness. Older local-only schema-2 saves with empty inline cache fields remain loadable.

Hosting is now concrete but undeployed: Firebase Functions 7.4.0, Admin 14.5.0 and Google Auth 11.1.0 are pinned. The HTTP and private Pub/Sub exports import without credentials or network calls. `firebase.premium.review.json` is separate from ordinary project configuration, requires explicit hosting/store parameters and runs server tests before deployment. A shared rate limiter fails closed, and the refund worker verifies the Google Voided Purchases API before changing a ledger. Live topic/Play/App Check configuration, monitoring, prolonged-outage reconciliation and real sandbox evidence remain in `PremiumDeploymentReview.md`. Updated dependency audit: zero reported vulnerabilities.

## Isolated Android diagnostic build

Built the exact core/catalogue in a separate temporary Unity project, ARM64 IL2CPP development APK, package `com.diwali.rocketrajaredesign.qa`. Build succeeded with **0 errors**, 141,145,208 bytes, 3m 30.829s. The package contains no Firebase, store SDK, game scenes or player data. Its probe is ready to measure core/save performance and force-stop recovery, but **Android rejected installation** with `INSTALL_FAILED_USER_RESTRICTED: Install canceled by user`. No device runtime measurements or acceptance claims are made. Original game APK hashes before/after are identical. Awaiting phone approval; no automatic retry. See `DeviceCore/README.md` for exact sources, APK hash, build result and failure evidence. Final game commit/build/replacement remains pending all required tasks.

## Reproducibility index published

Checklist now **45/79** complete, including RR-10-07. `README.md` is the current result/command/seed/tolerance/limitation index. `EvidenceManifest.json` inventories normalized-text source and evidence hashes; regenerate with `node Tools/RedesignLab/PublishEvidence.cjs` and verify with `--verify`. Original planning-model results remain labeled as such, and the Android installation rejection is retained as an unresolved acceptance limitation. No economy coefficient or original model formula changed, so no pacing-tuning revision was made to the simulation report. Exact-byte checksummed fixtures are protected from Git line-ending conversion by scoped `.gitattributes` entries.

## Repeatable content generation — RR-01-03

Checklist now **46/79** complete. Prepare Project always selects the redesigned authoring route, independent of save/network isolation. It no longer repairs Legacy defaults, configures test ads, exports the retired catalogue or silently recreates the old controller scene. The importer no longer constructs the old catalogue just to obtain product metadata. Existing scenes and legacy content are preserved until the separately tracked native integration work.

`RedesignGenerationValidation.Run`: **8 PASS** inside Unity. Covers creation from source, no retired catalogue population, stable GUID/bytes/write time on repeat, rejection of invalid data and content drift, preservation of a wrong-type target asset, preservation of existing legacy data/scene, and identical simulator/manifest exports. Only the two uniquely named temporary test assets are removed afterward. Validation checks source agreement even when row counts and version strings still match.

Rechecked **413 C# core checks**, **7,744 model comparisons**, **9 Unity content checks**, and **23 Unity core/save checks**. Default `ValidateGame.Run` now chooses the new core; the seven original save tests remain explicitly available through `RunLegacy`. Source content hash and economy coefficients are unchanged. The existing MCP path-space diagnostic remains; Unity reported no compilation failure. Native scene and smoke-harness conversion remain open under RR-07-06 and RR-10-01/05.

## Ad retirement — RR-04-06/07

Checklist now **48/79** complete. Removed AdMob/UMP initialization and packages, sponsor/offline ad grants, saved ad controls and consent rows, the Sponsor canvas/prefab, and the corresponding builder/production configuration requirements. Legacy serialized fields remain readable but cannot grant a bonus. Firebase SDK 13.17.0, embedded EDM4U 1.2.189, identity dependencies and analytics settings are preserved. The new economy and its fixture migration have no ad grant path; real-player migration remains protected.

Evidence: **414 core checks**, **310 legacy compatibility checks**, **28 cloud protocol checks**, **19 focused Play Mode checks**, **141 full existing interaction checks**, **6 prefab/compile checks**, **23 Unity core/save checks**, **8 generation checks**, and **9 content checks**. Actual Home/Offline captures and the package/dependency audit are in `AdRetirement/README.md`. The full smoke run found and fixed a test's positional recovery-screen assumption after Sponsor removal; its first failed report is retained alongside the complete successful rerun. No final game build/install, live cloud request or real save mutation occurred.

Two TMP OnValidate exceptions occurred during editor reload/asset processing; a follow-up inspection found no orphan submesh, the edited prefabs have no missing scripts, and both successful runtime test reports recorded no runtime errors. Existing MCP path-space and importer dependency-mask diagnostics are also retained honestly. The remaining 31 tasks still include new-screen review/integration, Play configuration and sandbox evidence, device installation/acceptance and human playtests. They are not closed by existing-screen ad removal.

## Rollout/recovery preparation — RR-11-06

Checklist now **49/79** complete. `RolloutRecovery.md` records exact current versions/content hash, fixture results, retained data namespaces, unresolved deployed-player mapping, draft release notes, signing/update sequence and a rollback plan that preserves paid receipts/draw operations/revocations. The old release runbook's ad/Legacy instructions and calendar promise are replaced. Privacy draft advertising claims are corrected to the source code; publisher/support/retention details and full release disclosure review remain open under RR-11-05.

Found and fixed a release-build bypass: the custom Production menu enforced isolation, but the common Unity pre-build callback previously only validated content. The callback now rejects all non-development builds while the prototype is isolated. **Four Unity build-mode checks PASS**. A compile-time guard also rejects `REDESIGN_LAB` in a non-development Unity player; **four actual core compiler/type checks PASS** for ordinary player exclusion, forced-lab rejection, permitted development diagnostics and the standalone lab. The latter uses simulated Unity symbols in a temporary .NET project; it is not an IL2CPP artifact inspection.

This closes the prepared rollout/recovery deliverable, not actual release acceptance. Remaining work still requires reviewed native screens, store/account/backend configuration, phone installation approval, actual sandbox/device/two-device results and player observations. No final commit, game build, installation, deployment or store publication was performed.

## 2 October scope, phone diagnostic and development shop

The user waived UI approval pauses, authorized phone installation, and deferred actual Google Play/IAP linking in favor of a placeholder test shop. Prior approval/installation/setup blockers above are historical. The user subsequently requested another installation retry; the separate ARM64 diagnostic installed successfully. POCO X2 Android 11 reports **14 first-launch + 4 force-stop/relaunch PASS**. The full 1,000-item quote averaged 5.2991 ms; save was 24,541 UTF-8 bytes. The original game's installed APK hash remained unchanged. Exact source/APK hashes, build result and phone reports are in [DeviceCore](DeviceCore/README.md). This tests the core snapshot recorded there, not the later native UI or the final game replacement.

DevelopmentShop.cs implements a visibly simulated, isolated persisted purchase operation for all four authored packs. Success, pending, cancellation, failure, duplicate/older callbacks and save failures are covered by 38 core checks; the Unity codec adds 11. Ordinary release symbols exclude grant methods and reject marked profiles. The initial expanded core suite had 452 checks; the current browser work increases that to 468. Real IAP remains disconnected and deferred, with no live server deployment or charge.

## 2 October ordered visual handoff — RR-07-02

Checklist now **50/79** complete. All 13 wireframes were followed by 13 individually generated/inspected Gouache Storybook references and then the editable HTML package. Reference prompts/provenance are saved; [the screen register](../../UI%20Builder/Wireframes/Redesign/NOTES.md) records the explicit waiver for each stage. [HTML handoff](../../UI%20Builder/HTML/Redesign/ART-DIRECTION.md) records 31 actions, 22 bindings, offline fixture limits, four viewport sizes, 36 retained captures and reconstruction direction. Browser checks include search/empty/retention, input isolation, test-shop outcomes, selected-item identity, blocked balances/eligibility and reduced motion. Zero browser errors. No human visual acceptance or native billing is inferred.

## 2 October native Upgrades implementation in progress

Authored the first saved native screen and shared shell through the verified Unity Editor into a scoped prefab/preview scene. All 50 protected scene/controller/prefab/font/material baseline files stayed byte-identical. The list reuses eight Editor-preplaced rows for all 1,000 authored choices; category/product/skill/search scopes are shared with immutable Buy All. Sixteen new core checks cover filtering, complete membership, passive-income cache reuse and quote scope stability. **468 core checks PASS**; all four compiler-symbol cases still pass.

**20 saved-asset/Edit Mode native checks PASS**, including actual core row purchase/review dispatch, last-item reachability, empty/blocked state and unchanged object identities. Four aspect-ratio images and a simulated-notch image were inspected. The first camera helper produced blank images and was corrected to select the preview scene; final capture now rejects blank output. Price text height and touch sizes were corrected after image inspection. An intermediate obsolete prefab API compile error and newly introduced obsolete-ID warnings were fixed; the current native code compiles.

[Native reconstruction evidence and remaining limits](NativeUI.md). Header/navigation still need the common presenter and remaining destinations; Main has not been switched. These isolated checks do not establish Play Mode raycasting/listener lifetime, integrated device performance, player understanding or final acceptance. No native integration task, final commit or final game build/replacement is claimed complete.
# Native catalogue, goals and level routes — 2 October 2026

The staged native preview now includes the full browser, upgrade details, frozen Buy All review, one persistent goal with alternatives, and product/tap level destinations. RR-07-03 and RR-07-04 are complete as native components; Main integration is still tracked separately. Checklist: **52/79**. See [NativeUI.md](NativeUI.md), [64 Play Mode checks](Unity-Catalogue-Play.txt), [20 catalogue asset checks](Unity-Native-UI.txt), [9 detail/review checks](Unity-Details-UI.txt) and [7 product/tap checks](Unity-Levels-UI.txt).

Native testing found and fixed TMP's lazy text submesh/caret construction by serializing those UI objects. Startup inventories and actual navigation identities now pass without excluding TMP objects. The Play Mode interaction run has no error/exception/assertion. Core tests pass **496**, including goal and level-projection checks; the **7,744 model comparisons** still pass. All 50 protected baseline files remain byte-identical. These results are isolated preview evidence; Main, integrated phone acceptance, remaining screens and final commit/build/install are still outstanding. Real IAP remains deferred; the earlier separate Android diagnostic installation succeeded.


## Native Talent and Prestige — 2 October 2026

RR-06-03/07-05 now complete; checklist **54/79**. The saved native collection, card details, reveal and Prestige pages pass **43 render/binding checks** and **52 two-session Play Mode checks**. Actual persisted reveal recovery preserves the operation/card/debit across a driver reload, and free/100-KK actions, save failure, duplicates, skip and reduced motion agree with the engine. Prestige preserves a reviewed quote through passive accrual and rejects intervening command state. [Native evidence and captures](NativeUI.md).

The expanded-prefab catalogue regression passes **64 checks** after fixing QA synchronization to wait for actual rendered frames before its raycast. Core **496** and release symbols **4** pass. All **50 protected baseline files** still match. The Talent runner records the installed MCP package's path-space startup advisory separately and reports no gameplay error/exception/assertion. Main integration, remaining four native states, integrated device/player acceptance and final delivery are still open. Actual IAP remains deferred by user.

## Native development shop — 2 October 2026

RR-09-02/03/06 complete for the user's explicitly requested placeholder scope; checklist **57/79**. Ten of 13 native states are authored. The saved shop binds all four packs and example prices, keeps No real payment visible, and implements ready/loading/unavailable, persisted pending, success/cancel/failure, leaving pending and save retry. **34 native binding/render** and **58 two-session Play Mode checks** pass; the latter reloads a pending operation from a real file and proves exact-once pack grants and a later explicit 100-KK draw. No real payment, live service or automatic currency spending occurs.

[DevelopmentShop.md](DevelopmentShop.md) records the initial recoverable startup write failure and the successful fresh rerun, without claiming a known cause. All 50 protected baseline assets remain unchanged; current core 496 and release symbols 4 pass. The runtime I/O diagnostic preserves future failure detail while the player's save message stays concise. Main/device and comprehensive recovery acceptance remain open, as do the remaining screens and final commit/build/replacement.

## Native return and skill routes — 2 October 2026

RR-08-06 complete as a native component; checklist **58/79**. Twelve of 13 native states are authored. Actual offline settlement feeds a receipt-only summary with capped time, amount, readiness and useful next actions. Closing/duplicate callbacks/save failure cannot claim or remove the settled reward. Native skill details connect all three base unlocks, purchases, ready/cooldown states and scoped upgrades.

**32 binding/render checks**, **43 actual Play Mode checks**, **505 core checks**, **4 release-symbol checks** and **7,744 model comparisons** pass. [Native evidence](NativeUI.md) includes inspected reference/short/notched captures and reproduction entry points. All 50 protected baseline files still match. Home and Main routing, retained settings/recovery, device/player acceptance and final delivery are outstanding; real IAP remains explicitly deferred.

## Home, preferences and startup retry — 2 October 2026

RR-03-04 complete; checklist **59/79**. All 13 native redesign states are saved. Home passes **25 binding/render** and **31 pointer/Play Mode checks**, including rate-limited manual input, no navigation/drag/outside tap leak, exact first purchase and fixed saved feedback inventory. Existing quantity/cadence checks cover x1/x10/x100/Max and independent Hands timing. [Native evidence](NativeUI.md).

The core suite passes **513**, model parity **7,744**, and release symbols **4**. Seven actual file-lock checks prove the startup retry finishes offline settlement before commands and never credits it twice. All 50 protected original assets matched before the scoped Main integration began; its before-state is recorded in MainUI-Baseline.json. Main, art progression, device/player acceptance and final delivery remain open; real IAP remains deferred.

## Main integration — 2 October 2026

RR-04-04/04-05/06-01/07-06 complete; checklist **63/79**. All 13 native screens and four retained utility dialogs are integrated in saved Main. Isolated Bootstrap bypasses legacy/cloud initialization. Main settles offline credit before input, keeps ordinary menus running, enforces recovery/suspension, and persists preferences/name/reveal across restart. Prepare Project preserves the native owner.

**12 asset/render checks**, **60 checks across two actual Main sessions**, and the **64-check expanded catalogue regression** pass. Home render/binding **25** and real startup file-lock/retry **7** pass again. [Native integration evidence and inspected captures](NativeUI.md) documents corrected canvas geometry/touch bounds, exact scope and reproduction. No gameplay/runtime errors were recorded. Four authorized files changed within the original 50-file protection baseline; all 46 others still match. The additional BootstrapController change has a separate saved before-state.

Core **513**, model comparisons **7,744** and release symbols **4** remain current. Full native Android and player acceptance, visual progression, onboarding/feedback and final commit/build/replacement are outstanding. Real IAP remains deferred and disconnected.

## Product appearance reuse — 2 October 2026

RR-08-02 complete; checklist **64/79**. Eight original atlas textures now provide 32 persistent native sprites. Source pixels are unchanged; transparent-gap regions agree between editable HTML and Unity. Current-run ownership and signature I/IV/X select the look, with faded empty and locked states kept separate. [Reuse review](VisualReuse.md) and [native evidence](NativeUI.md).

Validation: **83 native art checks**, **six HTML checks**, **567 core checks**, **67 actual catalogue Play Mode checks**, **7,744 model comparisons**, **four release-symbol checks**. All four Home tiers and representative detail/short-screen captures were inspected. Purchase, save/reload, reset and rebuy behavior passes; all UI instances remain saved. Permanent shop milestones and device/player acceptance remain open. No final commit or full game APK replacement has occurred; real IAP remains deferred.

## Permanent versus run visuals — 2 October 2026

RR-08-01/03 complete; checklist **66/79**. The original shop now has six cumulative saved stages at U=0/100/250/500/750/1000, with an explicit next-stage label. A new transparent painted atlas supplies five cosmetic motifs; all original shop/product textures remain unchanged. [Ordered art review and exact generation prompt](../../UI%20Builder/Reference%20Images/Redesign/shop-decorations-review.md).

**35 native shop checks**, **35 two-session Play Mode checks**, **seven HTML checks**, **585 core checks**, **7,744 model comparisons** and **four release-symbol checks** pass. Native Buy All crosses all stages once; real driver reload restores the settled result; native Prestige retains the shop, manual rebuilding restores Basic, and signature rebuy restores run art without increasing U. Physical shelf targets and all saved object identities remain intact. Expanded product/Home/Main checks pass 83/25/12, and Main regression passes 60 again. [Native evidence](NativeUI.md).

The original protection baseline still differed only at the four authorized Main/controller/build files at this checkpoint. Android/player acceptance, onboarding/feedback, final commit and full game build/replacement remained open; actual IAP linking is deferred by the user.

## Feedback and guidance — 2 October 2026

RR-08-04/05 complete; checklist **68/79**. [Feedback](Feedback.md) has 4 HTML/28 native checks and updated purchase/skill/Talent/visual Play Mode coverage. [Guidance](Guidance.md) has 35 focused core, 5 HTML, 64 native and 47 two-session physical interaction/reload checks. Learned state survives Prestige and save/load; failed acknowledgement never publishes learning; acknowledgements preserve reviewed batch/Prestige quotes. The reader never opens automatically. Main 60, catalogue 69, Home 31 and Main assets 12 pass with the new saved guide/inset. Core total is 620; parity remains 7,744 and release-symbol checks remain four.

The full native Android playtest build is tracked in [DeviceGame](DeviceGame/README.md). Platform/disclosure requirements were checked against [current official sources](PlatformReview-2026-10-02.md). Device/player results, integration completion and final committed replacement are not yet claimed.

## Integrated contracts and repeated native cycles — 2 October 2026

RR-10-01/05 complete; checklist **70/79**. The ordinary Unity codec entry point and optional development smoke harness select the redesigned economy. Default codec **23**, legacy codec/core/cloud **7/310/28**, premium authority **61** and localhost demo-emulator integration **22** pass. The actual Bootstrap/Main smoke passes **176** checks, including eight additional staged full-catalogue Buy All/Prestige/three-reveal/rebuild cycles with real temporary saves. Scrolling still uses eight saved rows; UI identities remain unchanged. The full-run payload stays below 26 KB. These are Editor measurements, not phone pacing/performance acceptance; [scope and reproduction](NativeSmoke/README.md).

Screenshot review caught an old Rocket illustration containing a neighboring-product sliver. A scoped HTML reuse review and native prefab reference change now use existing clean product regions for all collection/detail/reveal illustrations. Original source image bytes, Main scene bytes and protected fallback-font bytes remain unchanged. The corrected native Rocket capture was inspected; **44** Talent asset checks, **54** two-session Talent checks and **12** Main asset checks pass. No economy coefficients changed.

The full native Android playtest uses a separate package. The phone rejected its latest authorized install retry with `INSTALL_FAILED_USER_RESTRICTED`; a readiness question is pending. Actual device interaction, repeated-run hardware measurements, simulated-shop crash recovery and uncoached player observations remain open. No final commit or original-game replacement has occurred.

## Development artifact/disclosure preparation — 2 October 2026

RR-11-05 complete for the authorized prototype; checklist **71/79**. The current full native APK builds with **zero errors**, **30 reviewed warnings**, and passes signature/API/ARM64 plus all 19 native libraries' 16 KB alignment checks. It includes the 176-check smoke harness and corrected Talent art. A bounded resolver recovery succeeded before BuildPlayer; its failures and successful outcome remain recorded. The actual APK preserves disabled Analytics/Crashlytics flags, retains billing/ad-ID SDK permissions, and does not initialize real checkout. [Development preparation packet](DevelopmentReleasePreparation.md) records exact artifact identity and separates local delivery from unfinished public-release inputs.

The connected phone was observed locked with its screen off after rejecting the authorized install retry. Installation, phone tests, player observations and final acceptance remain open. No final commit or original-game replacement has occurred.

## Native account retry and recovery touch targets — 2 October 2026

The remaining-task audit found that Main could not retry a blocked account binding. Added an explicit account initialization port and exact-owner renewal/recovery without enabling live Google startup. Immutable checkpoint recovery, permanent stale-binding fences and server-timed offline receipts now reach the existing native recovery/modal flow. The isolated core suite passes 627 checks; the new native fixture passes 40 with no runtime errors. Real Google sign-in and device handoff remain unverified.

Physical raycasts exposed a saved recovery footer defect missed by callback-only tests: Exit overlapped Retry. Restored the two original footer rows with a two-offset Editor prefab revision and fixed the sizing authoring pass to preserve anchored positions. HTML inspection and tall/short native captures pass; Main asset checks now total 13. Main scene/meta and the shared fallback font remain unchanged. Details and reproduction: [AccountRecovery.md](AccountRecovery.md).

After these changes, the 60-check two-session guest Main regression and 176-check full-catalogue/repeated-cycle smoke pass again. The Editor left the second Main test session's exact restart callback queued; it was removed from the delay queue and invoked once, preserving the same profile and assertions. Default redesigned/legacy codecs and release guard checks pass; the four compiled release-symbol checks pass. This is current Editor evidence, not phone or uncoached-player evidence. Checklist remains 71/79.

The account-recovery/target-geometry candidate subsequently built in 2m45.705s with zero errors and 30 warnings. This run passed ordinary dependency resolution and generated POM/AAR validation without entering the bounded recovery branch. Current artifact identity and checks are in DevelopmentReleasePreparation.md and DeviceGame/PackageChecks.txt; preceding reports are preserved. No installation attempt was made while the subsequent device check still showed the screen off and keyguard present.
