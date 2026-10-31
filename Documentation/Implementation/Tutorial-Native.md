# Approved shopkeeper tutorial — native handoff

User approved the current HTML on 30 October 2026 and explicitly requested production Unity integration with `html-to-ugui`. Canonical authority: `UI Builder/HTML/Tutorial/index.html`, `classic-tutorial.css`, `tutorial.js`, and `renders-r12/centered-hand.png`. `revision-1` is an archived rejected presentation. This is production augmentation of the existing Classic flow, rather than a separate importer screen.

## Assessment and reconstruction contract

- Reuse: real economy quotes, staged save-before-publish commands, existing Player/Products/Legacy navigation, achievement claims, prestige preview/confirmation, committed card reveal, safe area, existing input/motion and authored material slots.
- Adapt: suppress generic contextual banners during the course; Home's existing next-display strip becomes the persistent current objective. Normal navigation remains usable.
- New: optional versioned tutorial receipts on fresh saves, one-time readiness routing, outcomes from committed before/after values, compact shopkeeper dialogue, exact approved hand, Settings resume/replay/skip. Old missing/version-zero fields never auto-enroll.
- Destination: `Assets/Scenes/Main.unity`, active `ClassicProductionCanvas`; existing `ClassicProductionCanvas__LegacyDisabled_20261019` remains intact/inactive. Bootstrap and build settings are outside this task.
- Preserve CanvasScaler 360×640, width match, and existing safe-area adapter. Bubble anchors to Shop at horizontal .19–.93, vertical .26 with bottom pivot and fixed 92 logical-unit height. Live headings/body reflow inside authored bounds. Hand is decorative, 44–64 logical units (48.6 at width 360), with actual opaque fingertip pivot (.205630, .951356). It tracks the CTA center through panel animation/scroll and rotates/shrinks at safe edges. No pointer pulse or new runtime UI objects.
- Saved additions: `SafeArea/Shop/Ivory and gold dialogue` (frame, input surface, live heading/body, saved TMP slots); `SafeArea/Shopkeeper tutorial/Centered pointing hand`; Home objective's replay-tip question mark; `Local dialogs/TutorialHelp` and Settings entry. Overlay defaults inactive. Initial note is data-driven Welcome for a newly created profile only.
- Browser-only review desk, fixtures and seeded currency are excluded. No changes to achievement thresholds, prices, rewards, draw RNG or paid fallback.

Exact existing mutation scope: `RedesignState.cs`, `RedesignEngine.cs`, `ClassicUI.cs`, `ClassicUI.Bindings.cs`, `ClassicGuidance.cs`, Main scene and its meta if Unity needs it, this ledger and Architecture documentation. New files: `RedesignTutorial.cs`, `ClassicTutorial.cs`, scoped Editor authoring/validation, and `Assets/Game/ClassicUI/Tutorial/` art/font/manifest. Supporting inventory correction: inspector now resolves HTML `<base>` correctly, with regression tests; archived HTML/CSS relative links were repaired. Validation includes the established `UI Builder` references directory. All package resources and 37 declared Unity paths passed before Unity authoring.

Exact-byte baseline backups and hashes were taken outside the project before editing, including the already-dirty Main, affected source files, and TMP assets. No restoration from Git HEAD is permitted. Original scene setup: clean Bootstrap; Main opened additively. Console baseline: existing project-path advisory, Input Manager deprecation and existing Rising arrow missing glyph warning.

## Interaction and motion matrix

| Trigger | Native owner and visible result | Commit/failure behavior | Motion and verification |
| --- | --- | --- | --- |
| New save | Version 1 Welcome above shopkeeper, counter hand | Version zero/missing stays unenrolled | Immediate coach, matching HTML; inherited panel effects honor reduced motion |
| First training affordable | Offer receipt; Player, Salesmanship hand, actual cost/payout note | No automatic purchase; save failure blocks presentation | Existing panel entrance; pointer follows center |
| Training commits | Atomic result note with actual tap before/after; Home objective | No result on failed save | Existing purchase feedback; immediate coach |
| First product affordable | Products once, first stock CTA and actual cost/income | Passive and tap earnings observed; manual purchase | Existing panel entrance and scroll, tracked hand |
| Product commits | Actual income result on Home | Outcome retained until dismissed | Existing purchase feedback |
| First useful upgrade affordable | Player once; PP-PHU-01 CTA, authoritative projection | Manual buy; no fixture price authority | Existing rows/scroll, tracked hand |
| Upgrade commits | Actual automatic income before/after | Atomic progress and result | Existing success feedback |
| Any real achievement first ready | Player once; Achievements entry | Existing staged claim and duplicate guard | Existing achievement modal; hand suspended |
| First prestige eligible | Player once; review prestige CTA | Existing reset/keep review, explicit confirm/cancel | Existing modal; no tutorial auto-reset |
| First completed prestige and credit | Legacy once; free 1× draw | No credit spend until user; no paid fallback | Existing panel and reveal animation |
| Committed free draw revealed and continued | One-time actual card/duplicate benefit, permanence | No second draw, grant, or reveal acknowledgement | Existing reveal takes priority; then shopkeeper note |
| Note tap/Enter/Space/Escape | Dismiss note only | Persist acknowledgement; never executes CTA | Hide; ordinary navigation stays available |
| Recovery/offline/modal/ad | Hide bubble/hand and queue guidance | Do not offer or route until normal gameplay resumes | Existing higher-priority lifecycle |
| Settings tutorial controls | Continue, replay at current progress, skip | Staged commands; no economy reset or rewards | Existing production popup shell |

## Verification

Completed 30 October 2026:

- [Core checks](Tutorial/CoreChecks.txt): 550 checks/actions, including real earned opening progression, optional old-save compatibility, failed-save rollback, one-time presentation across reload, passive affordability, actual achievement claim/duplicate guard, earned prestige, free draw/reveal restoration, card benefit, replay and durable skip. The long prestige journey advances authoritative engine time and uses JSON checkpoints before normal disk persistence; no cash or rewards are seeded. Disposable disk writes tolerate the already-known intermittent Windows scanner lock; explicit injected failures still test rollback.
- [Production Play Mode](Tutorial/PlayChecks.txt): all 68 checks passed through actual Bootstrap → Main, with a disposable profile override restored afterward. Includes pointer center checks while panel motion is active, settled captures, 360×640 / 390×844 / 360×540, text fit, free navigation, manual purchases/claim/prestige/free draw, cancellation, modal/reveal priority, actual card result, replay, skip, and deliberately locked-save recovery/retry. Component identities remain the same through the entire course; no runtime UI construction.
- [Saved-scene checks](Tutorial/SavedChecks.txt): 24 checks after reloading Main from disk, plus eight Edit Mode renders at 360×640, 390×844, 360×540 and 440×884, each with full and simulated notched safe areas. Dialogue fits without presentation initialization. References, inactive defaults, Home entry, hand pivot and CanvasScaler survive serialization.
- [Scope audit](Tutorial/ScopeChecks.json): no existing scene objects/components removed; 10,702 serialized objects in the preserved legacy subtree retain their original data (aside from the new empty optional tutorial reference on ClassicUI). All 12 protected shared TMP font assets are byte-identical to their pre-task backups. Bootstrap is byte-identical. No build-settings change, new production scene, WebView or runtime HTML was introduced.
- [Package inventory](Tutorial/PackageInspection.json) passes; 12 inspector regression tests pass, including local `<base>` resolution and boundary checks. The approved hand PNG is byte-identical to its source. Static SVG frame is rendered at 720×256. Fraunces is instantiated from the approved variable font at weight 700 / optical size 15 and baked into a scoped static TMP atlas; OFL and source/output hashes ship beside it.

Final compilation succeeds. The passing run produced no tutorial exceptions. Existing project-path, deprecated project API and missing Rising arrow glyph diagnostics remain outside this change. Android hardware, physical device insets and live ad/store services were not exercised; all tutorial economy actions and persistence above are local production paths.

The native art, controls and layout are editable in Main. Existing players can choose **Settings → Tutorial guidance → Replay guidance**; the Home question mark repeats the current tip without resetting progress. Real player files were not used for validation.
