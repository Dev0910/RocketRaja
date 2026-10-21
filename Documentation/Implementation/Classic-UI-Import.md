# Classic HTML to native uGUI

**Production integration follow-up (6 October 2026):** the original importer remains an isolated authoring/review scene. The approved hierarchy is now also saved in Main and connected through Bootstrap to the real guest/account owners. The old Editor Play override described later in this historical record has been cleared. See [Classic production integration](Classic-Production-Integration.md) for the current route, profile boundaries, recovery tests and remaining device/store gates.

## Authorized reconstruction

Source: `UI Builder/HTML/Classic/index.html`, including the festival motion revision and the connected panels/dialogs. The artwork gallery is supporting material, not a second screen authority. Its relative return link was made explicit so package validation passes.

Mode: Create. Destination: `Assets/Scenes/HTML Importer.unity`. The current production scene and build settings are outside this import. Existing importer canvases stay present; the new `ClassicImporterCanvas` is the active preview. No stable root of that name exists in the destination baseline.

All UI is authored and saved in the Editor: native Canvas, RectTransforms, Images, TMP labels, Buttons, ScrollRects, dialogs, 28 collection cards, product rows, reusable upgrade rows, and a fixed effects pool. Runtime code only binds data, switches saved states and animates existing properties.

## Functionality assessment

| HTML behavior | Native owner / change | Verification |
|---|---|---|
| Manual taps, idle sales, products, quantity selection | Reuse RedesignEngine/RedesignLifecycle; new Classic presentation bindings | Input guards, purchases, live balances |
| Ten next upgrades / Buy all | Reuse eligible catalogue and authoritative Buy All quote/commit | Prerequisites, affordability, stale/save failure |
| Skills and timers | Reuse PreviewSkill/ActivateSkill; native state binding | Locked, ready, active, cooldown |
| 28 cards, disabled undiscovered cards, outside dismissal | Reuse catalogue/card levels; new saved card views | All IDs, no unowned interaction |
| 1x/5x pulls | Reuse single draw; add atomic five-card quote/commit and persisted reveal queue | 4 credits or 400 KK, no mixed payment, failed save rollback, one grant |
| Prestige, offline, name, preferences | Reuse game-owned commands and persistence | Cancel/confirm, retained rewards, reduced motion |
| Shop/account/service outcomes | Reuse service ownership; importer uses isolated development purchase authority, no live payment/authentication | Explicit development outcomes and unavailable state |
| Search/catalogue | Fixed Editor-authored row pool with paging; no runtime instantiation | All 1,000 IDs reachable |

New code/assets are scoped to `Assets/Game/ClassicUI/`, `Assets/Game/Runtime/Classic/`, `Assets/Game/Editor/Classic/`, a focused `RedesignClassicDraws.cs` core extension, persisted batch fields/validation in `RedesignState.cs`, and `Tools/ClassicUiImport/`. Existing scene and core files are backed up outside the project before mutation. Shared sprites and fonts are reused without altering their import settings/materials; new art/fonts receive scoped assets. Source hashes are recorded for imported artwork.

Batch paid draws through a server-authoritative wallet require its server operation; the importer must not synthesize paid entitlements locally. Browser example balances/reward sequences are not production rules. The isolated test setup can seed review progress, separate from actual player saves.

## Reconstruction contract

Reference: 360 × 640, width-matched CanvasScaler, full viewport dimmer and safe-area content. The upper shop uses normalized illustration regions; lower panels stretch between it and the bottom navigation. Fixed-height headers/footers surround scrollable content. Home income and skills attach to the lower edge. Centered dialogs use height limits and scrolling; full cards preserve their portrait aspect. Test 390 × 844 and 360 × 540 plus a notched safe area.

Saved root structure: ClassicImporterCanvas → SafeArea → Shop / Panels (Home, Player, Products, Legacy, Shop) / Navigation; sibling LocalDialogs / Toast / Effects. One main panel active; all transient dialogs/effects inactive at rest. Browser review controls and fixture selectors are excluded.

## Interaction and motion matrix

| Trigger | Native presentation | Timing / settled state | Failure / interruption |
|---|---|---|---|
| Shop visible | Preplaced glows, 12 motes, shopkeeper idle | 4.7–8 s loops; ≤1 px keeper movement | Pause behind dialogs; reduced motion settles |
| Button press / hover | Existing RectTransform scale and graphic tint | 120–180 ms; scale 1 | Disabled controls stay inert; reset on disable |
| Tab | Panel group entrance, visible-row stagger | 320 ms + ≤160 ms delay; authored position | Input remains available; latest transition wins |
| Modal open/close | Dimmer and saved dialog CanvasGroup | 330 ms open / 160 ms close | No cloned runtime hierarchy; restore authored pose |
| Counter tap | Fixed coin trail and sparkle pool | 650–850 ms; pooled hosts hidden | No drag taps, no second reward path |
| Successful purchase/skill | Scale pulse, bounded particle burst, milestone fill | 380–850 ms; authored pose/current fill | No effect on rejected command |
| Card reveal | Saved shared back/front, rotating card, tier-colored particles | 1.65 s; front visible and Continue enabled | Close commits dismissal only; reduced motion settles |
| Prestige | Existing pool celebrates committed result | ≤850 ms | Confirmation/cancel remains game-owned |

Validation and final evidence are appended after native authoring and save/reload checks.

## Completed reconstruction and evidence

- Saved and reloaded `ClassicImporterCanvas` in `Assets/Scenes/HTML Importer.unity`: 2,351 objects, 499 TMP labels with preplaced material slots, 27 dialogs, eight product rows, 28 Talent cards, ten next-upgrade rows, a 20-row paged catalogue, and 64 reusable effects. No missing scripts, fonts, material slots, or filled-image sprites. Home is the sole active main panel at rest; dialogs start inactive. The previous importer canvases remain present and inactive.
- Core verification: `dotnet run --project Tools/ClassicUiImport/ClassicUiImport.csproj` passed 66 checks, including durable-save rollback, no reroll after failed persistence, exact four-credit/400-KK five-card costs, replay prevention, and persisted reveal queues.
- Native Play Mode review passed 29 checks across 46 captured states. Product/tap purchases, Buy All, unowned-card blocking, outside-card dismissal, free/paid batches, continuation and reduced-motion settling were exercised through saved control bindings. All UI/component identities remained unchanged. An additional interruption check closed the reveal immediately and confirmed exactly five grants, no pending result, and return to Legacy.
- Visual checks used 360×640, 360×540, 390×844, and simulated 44px top/34px bottom insets. Short Home overlap, popup sizing, product title/close positioning, full-card footer positioning, and image-fill rendering were corrected. Dialog dimmers cover the viewport while content follows the safe area. Browser-only review controls are omitted.
- Native motion includes shopkeeper breathing, floating motes, lantern/skill pulses, pressed states, staggered row/panel entrances, dialog entrances/exits, purchase bursts, flying coins, animated milestone fills, and shared-back card rotation with tier-colored particles. Start, rotation, settled, reduced-motion and interrupted reveal states were checked.
- Artwork uses 48 manifest entries (46 new assets, two exact shared matches). New product/card textures are capped at 512px, full borders at 1024px. No shared sprite import settings were changed.
- Typography uses scoped static Nunito/Baloo and licensed Noto Serif fonts; Noto Serif substitutes for browser Georgia. Decorative stars are middle-dot separators. A scoped Liberation Sans symbol atlas supplies arrows. Two automatically cached arrows in the shared TMP fallback were removed using the existing identical-configuration backup; the import retains zero references to that shared fallback. Its remaining preexisting working-tree changes are retained.
- The last focused Console check showed no Classic warning/error; the existing AI Game Developer warning about spaces in the project path remains.

## Preview and limits

Open `Assets/Scenes/HTML Importer.unity` alone and press Play. `ClassicPreviewSession` supplies a separate temporary guest profile, four free pulls and 400 KK each launch. Stop Play Mode to edit `ClassicImporterCanvas`. Production Main, build settings, real progress, authentication, receipts and payment services are not connected by this Create-mode import. Shop exercises the existing development purchase lifecycle, and account controls show their unavailable-service state.

The reconstruction uses native uGUI layout and fixed sprite effects. Browser paper grain, CSS shadows/perspective and blur are approximated with native artwork, alpha, rotation and scale; this is not a claim of pixel-exact parity. No Android/iOS device, live store, sign-in or server-wallet batch test was performed. A production replacement would require a separate routing/service integration step.

Editor-only authoring and review helpers live in `Assets/Game/Editor/Classic/`. Runtime Classic scripts only bind, navigate, resize or animate existing objects; they contain no UI creation/instantiation/component-addition calls. The builder refuses to overwrite an existing Classic root. `ClassicPlayReview.Begin(outputFolder)` runs the isolated review in Play Mode; `ClassicImportQA.Audit()` reports saved references and active roots.

Evidence on this machine: C:/Users/Dev/.codex/visualizations/2026/10/04/01a10668-9c90-7f90-b270-d6094bea89ec/Unity-Classic-20261006/ (HTML references, Native captures, final Home/card images, validation inventory and Console report). Two additional focused checks passed: closing a reveal before its flip finishes, and zero milestone fill at the level-25 boundary after saving/reloading. The importer is left in Edit Mode with Home selected.

## Motion parity follow-up — 6 October 2026

The initial import carried the broad effects but simplified several transitions. A source audit of `motion.js`, `motion.css`, and the reveal keyframes in `classic.css` identified the missing translations, hover states, distinct skill glows, particle palettes, and flip wind-up/overshoot. The table below supersedes the original motion summary.

| HTML presentation | Native implementation and timing |
|---|---|
| Panel and visible content entrance | 14px panel slide/fade, 9px content slide/fade; 320ms, up to ten visible groups, 28ms stagger capped at 160ms |
| Modal entrance/dismissal | 20px slide with .96→1 scale/opacity over 330ms; 220ms backdrop; 160ms exit with 7px movement/.98 scale; original saved objects become non-interactable during exit |
| Notices and full-card illustration | 6px notice entrance; illustration .96→1 scale and .3→1 opacity over 450ms |
| Hover, press and navigation | 120ms press, 180ms hover/release; 1px lift for buttons/product frames; 3px lift/.6° tilt and rarity-tinted shadow for owned cards; 450ms shelf-art nod; 380ms selected navigation pulse |
| Successful commands | 380ms pulse plus burst on product artwork, upgrade count, skill title, shop sign or KK balance; rejected commands do not celebrate |
| Progress | 400ms current-value transition; content entrance sweep over 580ms after 80ms delay; real milestone/collection targets retained |
| Tap effects | Three 11px coins, 650ms flight, 55ms stagger; tap/cash pulses and seven sparks |
| Celebration effects | 7/12/22 particles; 650–845ms; four-point stars every third particle, three rarity palettes, HTML radius/position/alpha/scale keyframes; fixed pool of 64 reused Images |
| Atmosphere | Twelve motes with 5–8s drift cycles, offsets and opacity keyframes; radial lantern glows with 4.7s alternating breath and 2.1s phase offset; 6.5s shopkeeper sway; 5.5s tap-hint brightness cycle |
| Skills | Ready ring pulse every 3.8s with per-skill phase offsets; active cyan glow every 2.2s; cooldown artwork saturation .45 |
| Card reveal | Shared back; 1.65s HTML wind-up, -12° anticipation, 190° overshoot, then settled front; native result fade and rarity glow; hidden result reserves its layout space so the popup does not jump |
| Pause/reduced motion | Ambient time freezes behind dialogs and on focus loss; transient effects cancel/settle on focus loss; in-game reduced motion restores poses, hides particles/atmosphere and reveals committed cards immediately |

Motion composes with responsive layout through a Canvas render callback after layout, restoring offsets before the next layout pass. Repeated effects replace the previous transition for their target. Authored scale/rotation and disabled-card opacity are preserved. Closing/reopening a view cancels stale transitions so an old exit cannot dismiss a newly opened screen. Reward authority remains in the existing commands, outside animation callbacks.

`ClassicMotionAuthoring.Apply()` authors references/components and the single reveal halo into the existing scene. The builder calls it for future imports. Two local 64px procedural UI sprites reproduce the soft radial glow and four-point spark; `Cooldown.shader`/material belongs to this import. No shared font, artwork, production scene or build setting is intentionally modified. All 2,352 UI objects, including effect hosts, exist before Play Mode.

Native limitations: uGUI uses orthographic Y rotation instead of CSS perspective; blur/drop-shadows and hover brightness are approximated with radial sprites, native Shadow components and tint. Ambient easing uses smooth cosine/linear segments. Pool capacity is fixed instead of the browser's five transient effect groups. The reduced-motion setting was tested in-game; mobile OS accessibility integration and device performance are untested.

Motion evidence: `C:/Users/Dev/.codex/visualizations/2026/10/04/01a10668-9c90-7f90-b270-d6094bea89ec/Unity-Classic-Motion-20261006/`. `html-flip-observations.json` records twenty browser transform/bounds samples. `Final/` contains native transition/reveal captures and the focused regression report; `Connected/` contains the repeated 46-state/29-check connected screen review. Frame captures are review samples, not a claim of frame-exact browser/native rendering.

Final verification: 42 motion checks passed, including start/in-flight/settled poses, purchase-frame feedback, owned/unowned hover, interruption/reopen, fixed reveal height, one-time five-card grants, reduced motion, simulated focus loss and unchanged UI/component identities. The connected review passed its 29 checks. After leaving Play Mode, the saved scene was reopened and all eight product effect targets, 28 hover shadows, particle/glow/material references and the sole Home entry state were verified. There are no missing scripts/fonts/material slots or dirty scene changes. Shared TMP fallback bytes and existing target GUIDs match the protected baseline. Main and EditorBuildSettings were not changed. Unity rewrote resolver/build-profile files during reload with no semantic Git diff; their configuration was not edited. Final Console errors are the existing integration path/graphics-API advisories, with no Classic runtime error.

Cleanup limitation: automatic approval review rejected temporary-backup deletion as “blocked by policy,” including a restricted per-file attempt. The verified baseline remains at `C:/Users/Dev/AppData/Local/Temp/classic-motion-20261006-190203/`. No project work is pending because of this cleanup restriction.

## Android build follow-up — 6 October 2026

The Classic scene now has a dedicated Android development startup, enabled only for the `CLASSIC_DEVICE_PREVIEW` build define and `com.diwali.rocketrajaclassic.playtest` application ID. Editor previews still create a temporary profile each launch. The phone build instead retains its separate test profile across relaunches and seeds four free pulls and 400 KK only when that profile is new. A saved development-only probe provides read-only interaction/animation diagnostics without creating UI objects at runtime.

`Builds/Android/RocketRaja-classic-playtest.apk` built successfully with zero errors and passed APK signature/manifest checks. The first installation attempt was rejected while the phone was locked. The user-requested retry succeeded, and manual operation on the POCO X2 verified navigation, purchases, free/paid five-card grants, and native rotation/settled reveal states with no probe-reported Unity runtime errors. Automated taps remained blocked by Android input permission; uncompleted device checks are explicitly listed in the [device test record](ClassicAndroid/DeviceTest.md). See [the build guide](ClassicAndroid/README.md) for the dedicated package workflow.

## Editor Play startup follow-up — 6 October 2026

The Editor had `Bootstrap.unity` open with no Play Mode start-scene override. That entry still routes to the existing Main UI. The new Classic presentation belongs to `HTML Importer.unity`, which was used explicitly for the Android playtest.

The Editor's `EditorSceneManager.playModeStartScene` now points to `Assets/Scenes/HTML Importer.unity`, and that scene was opened and launched through normal Editor Play. Verification found the initialized Classic driver, Home selected, the Classic root active and older importer canvases inactive. The initial offline notice was dismissed; the Editor was left running the new Home screen. Scene files, production EditorBuildSettings and the shared TMP fallback remained byte-identical during this correction. This is an Editor preview setting, not a migration of the production Bootstrap/Main service flow.
