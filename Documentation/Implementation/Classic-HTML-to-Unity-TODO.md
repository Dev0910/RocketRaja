# Classic HTML → Unity TODO

### H2U-017 — Achievement Reward ledger, 21 October 2026 — Done

User selected Option 1 and explicitly requested implementation of the existing Achievements page. Applied its trophy header, three-column claimed/ready/KK overview, illustrated compact ledger rows, progress bars, separate Kaju Katli reward column and direct Claim actions. READY, in-progress and claimed states use the existing fifteen definitions and atomic reward command.

HTML: `Classic/achievements-ledger.js`, `achievements-ledger.css`, current `classic.js` and `index.html`, plus the focused `UI Builder/HTML/Achievements/` entry and functionality metadata. Generated art is saved as a shared transparent atlas, with provenance and prompt in the focused package. Player's HTML shortcut now mirrors the existing native Achievements shortcut beside Skills.

Native: `ClassicAchievementLedgerAuthoring` creates and serializes the replacement in `Assets/Scenes/Main.unity`; `ClassicUI.Achievements.cs` binds its live overview and states. Original popup preserved complete and inactive as `Achievements__Legacy_20261021`. No runtime UI construction or new reward calculation was added. Existing enter/exit, claim feedback, modal input guards and reduced motion are retained.

Verification passes four phone sizes in browser and Unity, simulated notch insets, real claim and duplicate callbacks, native save reload, close/reopen, interrupted motion, fixed hierarchy and actual Player navigation through saved Main with a temporary guest profile. All 805 core checks pass. Full evidence: `Documentation/AchievementValidation/RewardLedger/`; current player data and live services were not used.

Follow-up correction, 21 October: READY now uses the selected notched gold plaque with small ornaments; Claim uses a rounded cobalt surface with a fine gold edge. Ledger CSS explicitly resets the shared button border image, forced square corners and disabled pseudo-element. Three independent action surfaces preserve pale blue In progress and muted Claimed states. The four small geometric surface PNGs are shared byte-for-byte by HTML and the Editor-authored native rows; live labels and 48px Claim targets are retained. Saved Main and the idempotent authoring command include the correction. Targeted evidence uses the `controls-` prefix in the same validation folder.

Started **19 October 2026 (Asia/Calcutta)** at the user's request. This is the running list of HTML changes to bring into Unity later.

Preview: <http://127.0.0.1:8765/Classic/index.html?capture=1>  
HTML entry: `UI Builder/HTML/Classic/index.html`  
Unity destination: the existing Classic UI in `Assets/Scenes/Main.unity`, with startup in `Assets/Scenes/Bootstrap.unity`.

## Starting baseline

- [x] Record the user's starting point: the current HTML and Unity presentation match. This is the agreed starting baseline, not a new claim of pixel-identical rendering or gameplay validation.
- [x] Preserve current HTML source bytes in [the baseline archive](ClassicHTMLSync/2026-10-19-baseline-sources.zip), including Classic scripts/styles and supporting Home/Shared styles/scripts.
- [x] Record SHA-256 hashes and exact capture time in [the baseline manifest](ClassicHTMLSync/2026-10-19-baseline.json). It also inventories browser asset files and Unity's `Assets`, `Packages`, and `ProjectSettings` at capture time.
- [x] Use the current working files, including existing uncommitted edits. Git HEAD alone is not this baseline.

Existing source-to-native comparison: [19 October parity notes](../../UI%20Builder/HTML/Classic/parity/README.md). Existing native ownership: [Classic import](Classic-UI-Import.md), [production integration](Classic-Production-Integration.md), and [presentation integration](Classic-Presentation-Integration.md).

The source archive supports later text diffs; binary assets are inventoried by hash, not copied into it. The snapshot is comparison evidence, not a runnable standalone preview or a Unity backup. Keep it unchanged while this change batch is open.

## Tracked changes

**Thirteen changes recorded across Home, Player, Skills and shared popups on 19 October 2026.** The user authorized native transfer on 19 October 2026. All thirteen tracked changes are implemented and verified in the saved Main and Bootstrap presentation. See [the native transfer report](ClassicHTMLSync/2026-10-19-native-sync.md). Earlier HTML-only and pending statements below describe the historical review stage.

| ID | Screen / component | Required change from baseline | HTML evidence | Unity target | Status |
| --- | --- | --- | --- | --- | --- |
| H2U-001 | Home / stall hint (comment 1) | Remove the visible “TAP THE STALL TO EARN” strip. Preserve the stall's tappable area and accessible label. | `UI Builder/HTML/Classic/index.html`; removed `.tap-hint` | Saved stall hint object in Main; confirm owner in `Editor/Classic/BuildClassicUI.cs` and `Runtime/Classic/ClassicLayout.cs` | Done |
| H2U-002 | Home / earnings icon (comment 2) | Replace the coin-stack illustration with a gold rising earnings chart, shown beside the combined rate's heading. Gold chart is an editable inline SVG in HTML. | `UI Builder/HTML/Classic/classic.js` (`earningsIcon`); `parity/native.css` | Existing income icon slot / new native icon asset and authoring; check `Editor/Classic/BuildClassicPanels.cs` | Done |
| H2U-003 | Home / rate labels (comments 3–5) | Remove the separate “/ SECOND”, “Normal sales” and “Counter taps” lines. Show compact currency with `/sec` for automatic sales and `/tap` for tap payout, alongside each amount. | `UI Builder/HTML/Classic/classic.js` (`home`, `rateAmount`); `parity/native.css` | `Runtime/Classic/ClassicUI.Bindings.cs`; saved income labels and `Editor/Classic/BuildClassicPanels.cs` | Done |
| H2U-004 | Home / income layout and combined rate (comment 6) | Stack automatic sales and per-tap payout on the left. On the right, show “EARNING NOW”: current automatic income plus accepted manual tap earnings in the preceding one second. Update as taps occur and expire; at rest show automatic income. | `UI Builder/HTML/Classic/classic.js` (`homeNormal`, `homeTap`, `totalEarnings`, `updateTotalEarnings`, `doTap`, binding metadata); `parity/native.css` | `Runtime/Classic/ClassicUI.Bindings.cs`, `ClassicLayout.cs`; use accepted game tap earnings and current engine income, not raw UI clicks | Done |
| H2U-005 | Home / skill buttons (comment 7) | Ready Home skill buttons activate the corresponding skill directly without opening SkillDetail. Locked, already active, cooldown and save-blocked buttons remain disabled. Detail screens remain reachable through other routes. | `UI Builder/HTML/Classic/classic.js` (`home`, `ActivateSkill`); `parity/native.css` | Saved Home button bindings in `Editor/Classic/BuildClassicPanels.cs`; `Runtime/Classic/ClassicUI.Bindings.cs`, existing `RedesignEngine.ActivateSkill` | Done |
| H2U-006 | Stall / shop-name alignment (second batch, comment 1) | Center the shop-name text horizontally and vertically inside the painted plaque. Correct the line box and vertical anchor; keep the name editable. | `UI Builder/HTML/Classic/parity/native.css` (`.shop-title`) | Saved shop-name RectTransform and TMP alignment in Main; `Editor/Classic/BuildClassicUI.cs`, `Runtime/Classic/ClassicLayout.cs` | Done |
| H2U-007 | Home / skill duration and cooldown rings (second batch, comment 2) | All ready skills have full gold rings. Activation drains the gold ring as a duration clock. When the duration expires, grey the icon and fill a grey radial cooldown; return to full gold and enable the button at readiness. Bulk Order is instant and starts grey cooldown immediately. Use actual remaining mm:ss and independent clocks; locked skills have no gold ready ring. Depends on H2U-005. | `UI Builder/HTML/Classic/classic.js` (`skillClock`, `skillPresentation`, `skillRing`, `tickSkills`, `ActivateSkill`, binding metadata); `parity/native.css` | Editor-authored radial Filled Images around saved Home skill icons in `Editor/Classic/BuildClassicPanels.cs`; live timers from `RedesignState.activeUntil` / `readyAt`, bound by `Runtime/Classic/ClassicUI.Bindings.cs`; reduced-motion handling in Classic motion owners | Done |
| H2U-008 | Player / Stats and Skills shortcuts (third batch, comment 1) | Remove the painted button frames/backgrounds; retain the icons, text, clickable areas and existing navigation actions. | `UI Builder/HTML/Classic/parity/native.css` (`.classic-toolbar .player-icon`) | Saved Stats/Skills button background Images in `Editor/Classic/BuildClassicPanels.cs`; preserve Button/listener and icon/text objects | Done |
| H2U-009 | Player / upgrade purchase-button gutter (third batch, comment 2) | Bring upgrade purchase buttons inside their row frames with a consistent 10-logical-pixel right inset. Include padding within the button width and reserve space for the upgrade text. Apply the same rule to the shared catalogue upgrade-row template. | `UI Builder/HTML/Classic/parity/native.css` (`.upgrade-row .row-action`, `.upgrade-row .plain-detail`) | Upgrade-row RectTransforms and right anchors in `Editor/Classic/BuildClassicPanels.cs`; existing shared catalogue row authoring and `Runtime/Classic/ClassicLayout.cs` | Done |
| H2U-010 | Skills / selected Quick action list popup | Implement selected concept 1: three stacked painted-icon rows, each with name, live ready/active/cooldown/locked status, effect, duration/cooldown, direct Activate and Details. Preserve the shared gold/grey clocks from H2U-007. Replace the oversized guidance banner and Back footer with a subtitle, close X and compact prestige note. Details returns to its invoking row without resetting timers. | `UI Builder/HTML/Classic/index.html`, `classic.js`, new `skills-popup.css`; selected reference `UI Builder/Reference Images/Skills/Popup Variants/2026-10-19/01-quick-action-list.png` | Existing Classic Skills popup in `Editor/Classic/BuildClassicDialogs.cs`; `Runtime/Classic/ClassicUI.cs` / `ClassicUI.Bindings.cs`; existing core `PreviewSkill`, `ActivateSkill`, `activeUntil` and `readyAt` | Done |
| H2U-011 | Individual skill / selected Hero card | Apply selected concept 1 to all three skills: large painted art, live ready/active/cooldown/locked ring and status, two skill-specific effect tiles, duration/cooldown and one activation button. Remove View skill upgrades for every skill. Keep close/back-to-list, timing semantics and existing activation guards. | `UI Builder/HTML/Classic/classic.js`, new `skill-hero.css`, `index.html`; selected `UI Builder/Reference Images/SkillDetail/Popup Variants/2026-10-19/01-hero-skill-card.png` | Existing Classic SkillDetail authoring and view binding; reuse `RedesignEngine.PreviewSkill` / `ActivateSkill` / `activeUntil` / `readyAt` | Done |

| H2U-012 | Shared popup dismissal | Clicking or tapping anywhere outside the top popup card invokes the existing Close path once, restoring its parent popup or invoking screen and focus. Inside clicks keep it open. Consume the outside click to prevent underlying navigation or stall taps. Retain existing mandatory SaveRecovery / RecoveryExit choice guards. | `UI Builder/HTML/Classic/classic.js`, `index.html` navigation/QA metadata | Shared modal backdrop listener and close/history/focus behavior in `Runtime/Classic/ClassicUI.cs`; saved dialog backdrop in `Editor/Classic/BuildClassicDialogs.cs` | Done |

| H2U-013 | All existing popups / generated-reference refresh | Cover all 29 popup routes with the cream/cobalt/gold Gouache Storybook treatment, readable flowing copy, painted artwork, safe title/close clearance, semantic content and scrolling on short screens. Use 26 new distinct ChatGPT-generated references; Account shares Welcome, and retain the selected Skills list/Hero references. Introduce illustrated confirmations, product/milestone panels, statistics ledger, catalogue/receipt rows and appearance previews. Make Welcome/Account compact dismissible modals, show CardDetail close X, preserve rarity frames and all existing action parameters. Remove the bottom Back button from Settings at the user’s explicit follow-up; retain X and outside close. Native recovery/quote/purchase/reveal/skill policies remain unchanged. | `UI Builder/HTML/Classic/popup-refresh.css`, `popup-refresh.js`, `classic.js`, `index.html`, generated `assets/popup-refresh/`; references/screenshots under `UI Builder/Reference Images/PopupRefresh/2026-10-19/` | Existing saved dialogs and shared backdrop/close in `Editor/Classic/BuildClassicDialogs.cs`, `Runtime/Classic/ClassicUI.cs`, `ClassicUI.Bindings.cs` and layout/motion owners. Reuse existing core/action ownership and adapt generated frame/icon assets in the separately requested native stage. | Done, including 21 October shared close-clearance in HTML and Unity |

| H2U-014 | Settings / sliding switches | Replace the flat Music, Sound effects and Reduced motion ON/OFF buttons with left/off and right/on pill switches. Preserve the existing Setting actions, stable IDs, keyboard behavior and accessible names. Keep a 74×48 hit region, readable state labels, focus styling and a 180 ms thumb transition that settles immediately for reduced motion. | `UI Builder/HTML/Classic/classic.js`, `popup-refresh.css`, cache versions in `index.html`; browser `settingsSwitches` binding | Existing Settings controls in `ClassicHTMLSyncAuthoring.SyncSettings`, `ClassicUI.Handle("Setting")`, `ClassicUI.HTMLSync.cs`; a future native change must use saved Editor-authored tracks/thumbs and the same settings owner | Done in HTML and Unity on 25 October 2026; current transfer request supersedes the earlier HTML-only stage. |

| H2U-015 | Shop / redundant collection and draw information | Remove the Your collection shortcut, Card draws information card and See card odds button from Shop. Retain the KK balance, store availability notice, four pack rows, and Legacy navigation/information entry. Preserve surviving control IDs and purchase guards. | `UI Builder/HTML/Classic/classic.js`, cache version in `index.html` | Saved Main Shop panel; `ClassicPanelControlsAuthoring` deactivates Balance/Collection, Card draws and Odds. `BuildClassicPanels.BuildStore` omits them on future authoring. | Done in HTML and Unity for the requested Android build |

| H2U-016 | Player, Products, Legacy and Shop / collapse controls | Replace all four thin text arrows with the selected SVG down chevron on a cobalt circular face with a gold rim. Keep each existing collapse-to-Home action, accessible name, stable ID, keyboard behavior and 48×48 logical target. | `UI Builder/HTML/Classic/index.html`, shared `parity/native.css`, `classic.js` metadata | Saved panel header/Collapse controls in Main; `ClassicPanelControlsAuthoring` and `BuildClassicPanels.BuildPanel`; existing Tab handler retained. | Done in HTML and Unity for all four panels in the requested Android build |

### Skills Quick action list: selection and HTML checks

The user selected generated concept 1 and explicitly directed HTML implementation on 19 October 2026. The reference is approved for this popup revision; the resulting HTML remains a candidate. All text, controls, states and decorative elements are separate HTML/CSS/SVG elements using the existing local paper texture, fonts and painted skill icons; the concept screenshot is not used as a flattened control surface.

Verified ready-state fit at **360×540, 360×640, 390×844 and 450×916**: three rows, no internal or horizontal scroll, all buttons inside the popup, and minimum 48 px button height. Verified direct activation with live countdown/ring, instant Bulk Order cooldown, Details/back with retained timer and focus, close returning focus to Player's Skills shortcut, locked buttons, and save-error blocking. Embedded metadata has one JSON block, three stable skill keys, existing action mappings, valid binding targets and no duplicate IDs.

Evidence: [390×844](ClassicHTMLSync/2026-10-19-skills-quick-list-390x844.png), [360×540](ClassicHTMLSync/2026-10-19-skills-quick-list-360x540.png), [running clocks](ClassicHTMLSync/2026-10-19-skills-quick-list-running.png), and [focused checks](ClassicHTMLSync/2026-10-19-skills-quick-list-checks.json). These are browser fixture checks, not native gameplay or reward/save validation.

### Player revision: shortcut frames and button spacing

At 964×884, the upgrade buttons previously extended about 6.1 px past their row frames because the absolute percentage width excluded padding. Using border-box width and a scaled right inset now leaves about 14.3 px inside each frame. Checked all ten Player upgrade rows: no right overflow or overlap with the text column. Stats and Skills retain 102×64 px clickable areas with no border-image/background. Page width has no horizontal overflow. No action or gameplay binding changed.

Visual references: [borderless Stats/Skills](ClassicHTMLSync/2026-10-19-player-shortcuts.png), [upgrade purchase-button inset](ClassicHTMLSync/2026-10-19-player-upgrade-spacing.png). Native implementation remains pending.

### Second Home revision: clocks and alignment

The browser now runs presentation deadlines using the exported `PreviewSkill` duration and cooldown, replacing the first revision's static 00:20 / 02:00 labels. Timers survive navigation, refresh their displayed seconds and radial progress every 100 ms, reset with the preview fixture, and catch up from absolute deadlines after background throttling. Reduced motion removes decorative interpolation without hiding timing information. These are disposable visual clocks; the browser still grants no income/rewards and writes no player save.

The native engine sets `readyAt` at activation, so cooldown is not added again after the active duration. For the established-shop Festival Rush example, activation gives 30 seconds of active gold, then 255 seconds of grey cooldown (285 seconds total). Bulk Order has no active duration and enters its 600-second cooldown immediately. Native implementation must bind existing authoritative deadlines rather than introducing a second gameplay timer.

[Focused clock checks](ClassicHTMLSync/2026-10-19-skill-clock-checks.json) cover active start/midpoint/expiry, cooldown midpoint/readiness, background catch-up and instant-skill timing. Visual references: [ready rings and centered name](ClassicHTMLSync/2026-10-19-skill-rings-ready.png), [active countdown](ClassicHTMLSync/2026-10-19-skill-ring-active.png), and [grey cooldown](ClassicHTMLSync/2026-10-19-skill-ring-cooldown.png).

### First Home revision: decisions and checks

All five items are HTML-only; no native file or saved scene was edited. H2U-004 uses a rolling one-second sample (the implementation choice for the requested earnings/sec display). HTML samples disposable accepted stall taps against the frozen exported automatic-income rate; it does not accrue or save cash, simulate Helping Hands events or instant Bulk Order rewards, or introduce a separate gameplay economy. Existing fixture-based skill state demonstrations remain in use. The native stage must use authoritative gameplay results and reconcile skill-generated income with the reviewed definition of the combined rate.

Browser checks: the hint is gone while the stall button remains; one tap changes ₹10.24K/sec to ₹13.67K/sec and the total returns to the baseline when the sample expires. Festival Rush activates directly and updates automatic/tap rates to ₹20.48K/sec and ₹4.29K/tap. Helping Hands activates directly and Bulk Order enters cooldown without a popup. New-shop skills are all locked; save-error skills are disabled. Reviewed the layout at the user's 964×884 browser viewport. The four phone-size comparisons remain a later verification task, not a claimed result of this edit.

Review screenshot: [Home HTML revision](ClassicHTMLSync/2026-10-19-home-revision.png). Syntax, rate-window arithmetic and metadata checks are recorded in [the focused check results](ClassicHTMLSync/2026-10-19-home-checks.json).

[Native inventory comparison](ClassicHTMLSync/2026-10-19-home-unity-readonly.json): all 1,385 baseline files under `Assets`, `Packages` and `ProjectSettings` remain unchanged, with no additions or removals.

Statuses: `HTML in progress` → `Awaiting HTML review` → `Ready for Unity` → `Unity in progress` → `Verify in Unity` → `Done`. Use `Superseded` when a later request replaces an item, retaining its replacement ID.

For each new item, use the next stable ID (`H2U-001`, `H2U-002`, …) and record:

- The request and date, and the visible before/after behavior, copy, layout, artwork, or motion.
- The exact HTML/CSS/JS/asset paths and affected screens/states. A shared change may affect several screens.
- Likely native scripts, saved UI objects, and assets; mark unconfirmed mappings as such.
- Dependencies on other items, HTML review status, and any decision still needed.
- Native completion evidence: changed files/objects, screenshots, relevant behavior checks, and outstanding differences.

## Selected individual skill Hero card

19 October 2026: the user separately requested three ChatGPT-only image-generation alternatives for the individual **Your skill / Festival Rush** detail popup, allowing UX changes. [Images and prompts](../../UI%20Builder/Reference%20Images/SkillDetail/Popup%20Variants/2026-10-19/README.md): **1. Hero skill card**, **2. Earnings comparison**, **3. Power and recharge dial**. The user selected **1. Hero skill card**, requested implementation and explicitly removed **View skill upgrades**. Implemented in HTML as **H2U-011**, preserving all three skill mechanics, direct activation, close/back navigation and cooldown-from-activation policy. Options 2 and 3 remain unselected; Unity implementation is pending.

## Selected Skills popup concept

19 October 2026: the user requested three versions of the existing **Your skills** popup, using ChatGPT image generation only, and allowed UX changes. Created [three generated concepts and their prompts](../../UI%20Builder/Reference%20Images/Skills/Popup%20Variants/2026-10-19/README.md): **1. Quick action list**, **2. Skill workshop**, **3. Festival tickets**. The user then selected **1** and directed its HTML implementation, now recorded as **H2U-010**. Options 2 and 3 remain unselected. No Unity implementation is claimed.

## How to maintain this list

1. Update this list in the same work session as each requested HTML revision, including small follow-ups. Update an existing item when the same component changes again; retain the final requirement and any superseded decisions.
2. Compare against the saved baseline, including shared styles, script behavior, fonts and image assets. Do not mistake preexisting working-tree changes or old parity overrides for new work.
3. HTML completion, user review, Unity implementation and Unity verification are separate statuses. Record actual review instructions; carry forward existing authorizations and any explicit workflow waivers.
4. When asked to bring the changes into Unity, implement the listed batch against the current native project. Reconcile any native changes since this snapshot first.
5. Mark an item `Done` only after native implementation and relevant native checks. Keep completed rows as history. Establish a new baseline only after the batch has been reconciled.

## Native mapping for later implementation

Paths below are relative to `Assets/Game/`. Confirm the specific saved object before editing.

| HTML area | Existing Unity owners / authoring |
| --- | --- |
| Stall, shared frame, safe area, panel sizes and navigation | `Runtime/Classic/ClassicLayout.cs`, `Runtime/Classic/ClassicUI.cs`, `Editor/Classic/BuildClassicUI.cs` |
| Home, Player, Products, Legacy and Shop panels | `Editor/Classic/BuildClassicPanels.cs`, `Runtime/Classic/ClassicUI.Bindings.cs` |
| Popups, detail views, Talent/reveal, utilities and confirmation flows | `Editor/Classic/BuildClassicDialogs.cs`, `Runtime/Classic/ClassicUI.cs`, `Runtime/Classic/ClassicUI.Bindings.cs` |
| Product appearances and permanent shop decorations | `Runtime/Classic/ClassicProgression.cs`, `Editor/Classic/ClassicPresentationAuthoring.cs` |
| Contextual tips and Help | `Runtime/Classic/ClassicGuidance.cs`, `Editor/Classic/ClassicPresentationAuthoring.cs` |
| Transitions, press feedback, ambient effects and reduced motion | `Runtime/Classic/ClassicMotion.cs`, `Runtime/Classic/ClassicMotion.Presentation.cs`, `Editor/Classic/ClassicMotionAuthoring.cs` |
| Startup, guest/account and save recovery | `Runtime/Classic/ClassicApplication.cs`, existing Bootstrap/Main service owners; see production and save/account integration notes |
| Reused or new sprites, fonts and materials | `ClassicUI/`, existing shared native resources; HTML equivalents under Classic, Home and Shared |

Browser fixtures and sample numbers describe preview states. Record intended gameplay changes explicitly if requested; native commands, quotes, saves, account flows and purchases continue to use their existing game owners.

## Checklist when the Unity batch is requested

- [ ] Reconcile the final HTML diff with every open `H2U` item; include removals, shared styles/assets, text changes, interactions and motion.
- [ ] Resolve affected native owners and dependencies, then implement the reviewed changes in the existing saved native UI.
- [ ] Compare affected screens at 360×540, 360×640, 390×844 and 450×916, with relevant safe-area and scrolling cases.
- [ ] Check affected interactions and states: navigation/back/close, enabled/disabled controls, locked/available/owned states, active/cooldown, errors and reduced motion where applicable.
- [ ] Run relevant existing native validation and gameplay checks for the changed behavior; record visual differences or remaining device checks.
- [ ] Add native evidence to each item, finish the batch, and record the next shared baseline.

## Batch history

| Date | Record |
| --- | --- |
| 2026-10-19 | Started this tracker and captured the current working baseline before the user's next HTML revisions. No post-baseline Unity changes requested yet. |
| 2026-10-19 | Implemented seven Home browser comments as H2U-001–005 in HTML; awaiting review before the later Unity batch. |
| 2026-10-19 | Implemented shop-name centering and live gold/grey skill clocks as H2U-006–007. The new clocks replace the earlier static skill-state demonstration; native implementation remains pending. |
| 2026-10-19 | Implemented borderless Player Stats/Skills shortcuts and inset upgrade purchase buttons as H2U-008–009; HTML-only, awaiting review. |
| 2026-10-19 | Generated three Skills popup design alternatives with ChatGPT image_gen only. User selection pending; HTML and Unity unchanged by this concept task. |
| 2026-10-19 | User selected Quick action list (concept 1) and directed HTML implementation. Implemented and browser-checked as H2U-010; HTML review and native implementation remain pending. |
| 2026-10-19 | Generated three separate individual skill-detail popup alternatives with ChatGPT image_gen only. Selection pending; HTML and Unity unchanged by this concept task. |
| 2026-10-19 | User selected Hero skill card and removed View skill upgrades. Implemented the HTML card across all three skills as H2U-011; native transfer remains pending. |

### Individual Hero skill card verification

Checked all three skill cards at 360×540, 360×640, 390×844 and 450×916: no internal/horizontal scroll, card and activation button inside the viewport, activation target at least 48 px high, and no upgrade-navigation button. Effect values come from existing exported fixtures; browser clocks remain disposable and grant no rewards.

Verified activation updates both the status and primary-button countdown with the radial clock; Bulk Order immediately shows a disabled Ready in timer. Locked and save-error activation remain blocked. Close returns to the selected Skills-list row and restores its Details-button focus. Metadata has one block, one valid Hero binding target, no duplicate IDs and exactly one footer action. Browser error log is empty.

Captures: [Festival Rush](ClassicHTMLSync/2026-10-19-skill-hero-FestivalRush-390x844.png), [Helping Hands](ClassicHTMLSync/2026-10-19-skill-hero-HelpingHands-390x844.png), [Bulk Order](ClassicHTMLSync/2026-10-19-skill-hero-BulkOrder-390x844.png). Focused checks: [Hero card results](ClassicHTMLSync/2026-10-19-skill-hero-checks.json).

### Shared outside-click dismissal verification

19 October 2026: H2U-012 implemented in HTML. Checked Settings and skill-detail inside clicks stay open; outside clicks in the app backdrop and outer page margins close one popup. Nested Naming returns to Settings and nested SkillDetail returns to Skills, restoring the invoking control focus; a subsequent outside click returns to Player. The combined earnings rate stays unchanged and no underlying navigation opens. Required SaveRecovery remains open on outside click. JavaScript syntax validation passes and browser error log is empty. [Dismissal result](ClassicHTMLSync/2026-10-19-outside-popup-close.png). Unity transfer remains pending.

### All-popup generated-reference refresh

19 October 2026: the user requested chatgpt-ui-to-html across every existing popup, then removed Settings’ bottom Back button. Completed the image-first HTML pass as H2U-013: 29 routes, 26 distinct new references, Account/Welcome shared design, existing selected Skills and Hero card references retained. The reference set includes targeted corrections for missing X controls and the Settings Back removal. New raster artwork used the built-in ChatGPT image_gen tool only.

[Coverage, prompts and visual comparison gallery](../../UI%20Builder/Reference%20Images/PopupRefresh/2026-10-19/README.md). Generated references and the implemented HTML await user visual acceptance. These are existing-popup revisions and create no new game screens or mechanics.

Verified all 29 routes at 390×844 and 360×540 (58 cases): frame containment, no horizontal overflow, no missing images, one metadata block, no duplicate IDs, and visible actions at least 48 px high. Before/after action-parameter comparison retains every action except the requested Settings Back removal. Focused checks cover settings, validation, nested focus return, outside dismissal, stale/save-error guards, all four disposable purchase preview outcomes and skill clocks. Both JavaScript syntax checks pass; browser error log empty. The HTML reuses existing painted game illustrations and simplifies some decorative mockup details; frames and new Settings symbols are separate local assets, never flattened screenshot controls.

Native transfer remains pending. A separate read-only baseline hash report records whether any Unity files differ.


## Native transfer completed — 19 October 2026

H2U-001–H2U-013 are **Done**. The current production canvases contain the revised Home/Player presentation, selected skills list and hero, direct activation and engine clocks, shared dismissal and all 29 styled dialog routes. Bootstrap owns the actual startup/account flow and now has a dismissible, reopenable, content-sized popup with saved TMP material slots. Settings has no bottom Back action.

Verification: **82 connected production-fixture checks**, **66 atomic draw/persistence checks**, **72 base phone captures** at 360×540, 360×640, 390×844 and 450×916, plus focused checks for all hero variants, active/cooldown visuals, safe-area insets, name validation, long rates, receipt/reveal dismissal and focus. Shared fonts and generated-art parity were audited. Both original hierarchies remain inactive, and the temporary QA scene was removed.

Details: [native transfer report](ClassicHTMLSync/2026-10-19-native-sync.md), [machine-readable verification](ClassicHTMLSync/2026-10-19-native-verification.json), and [connected production results](Classic-Production-Validation.txt). Earlier pending statements are retained as the history of the HTML-only stage.

### H2U-010 native Skills popup correction — 21 October 2026

The user identified the Skills list from the browser reference and requested a Unity visual fix. Updated the existing Main popup in Edit Mode: smooth double gold row borders and cobalt/gold outer frame, centered bold serif title, correctly sized decorative close/Activate buttons, aligned icon/copy/action columns, compact status badges, gold portrait discs for Helping Hands/Bulk Order, readable effect/timing copy and the shorter prestige note. Active/cooldown/locked action labels and badge colors follow the existing engine state. The popup height now includes its authored header/body offsets, so the complete list fits without unnecessary scrolling.

Owners: `Assets/Scenes/Main.unity`, `ClassicSkillsPopupAuthoring.cs`, `ClassicUI.HTMLSync.cs`, `ClassicLayout.cs`, and two local sliced surfaces under `Assets/Game/ClassicUI/HTMLSync/`. HTML files were not revised. All UI, badge layouts and bindings are saved before Play Mode; real activation, clocks, Details navigation, motion and dismissal retain their existing owners.

**Implemented and verified in Unity; awaiting user visual review.** Native captures pass at 360×540, 360×640, 390×844, 450×916 and 1080×2400: all text fits, no unnecessary list scrolling, activation targets at least 48 units and compact badges. Additional checks pass active/cooldown/locked states, all three Details routes with focus return, 44 px top / 34 px bottom safe-area insets, blocked/repeated activation and stable runtime hierarchy. The existing isolated behavior suite passes; reloaded Main passes the saved 29-dialog/seven-clock audit with three complete list badge/action-label bindings. Shared font assets match the pre-change backup. The temporary QA scene was removed; Unity is in Edit Mode with Main open and its Skills window selected.

Captures, raw results and pre-change backup: `C:/Users/Dev/.codex/tmp/skills-popup-fix-20261021/`. Final preview: `final-390x844.png`. Console inspection found the existing MCP path-space advisory and Unity import environment/graphics-mask messages; no Skills script compilation or runtime exception was observed. This verifies Editor rendering and interactions, not an Android-device build.

### H2U-013 native Statistics popup correction — 21 October 2026

The user requested that the existing Unity Statistics popup match the selected HTML reference. Updated Main's saved popup with a centered two-line serif title, smooth cream/gold ledger cards, larger side illustrations, ten independent aligned label/value rows with separators, the existing live decoration summary and a fixed Back footer. The body scrolls at short heights while the title, close and Back controls remain visible. Values continue to read the native engine; the existing shop-progression component now targets the new saved summary. The previous ledger remains inactive and inspectable in the same popup.

Owners: `Assets/Scenes/Main.unity`, `Assets/Game/Editor/Classic/ClassicStatisticsPopupAuthoring.cs`, and the individual Statistics bindings in `ClassicUI.HTMLSync.cs`. Reused existing native frame/card surfaces, painted icons and font assets. No HTML, economy, save rules or skill presentation changed in this correction. All metric cells, material slots, artwork and controls are Editor-authored and serialized before Play Mode.

**Implemented and verified in Unity; awaiting user visual review.** Native captures and data assertions pass at 360×540, 360×640, 390×844, 450×916 and 1080×2400. Each of the ten values matches the actual engine state, labels and values have separate aligned columns, text fits, and Back/X targets are at least 48 units. Checks cover fresh progress, large numbers, all six decoration milestones, simulated safe-area insets, short-screen scrolling to the decoration card, a real tap followed by updated statistics, modal gameplay blocking, Back/X/outside dismissal, focus return and stable runtime object identities. The existing isolated behavior suite also passes. Reloaded Main retains all ten metric bindings, the live decoration owner and the three cards, and passes the saved 29-dialog/seven-clock audit.

Evidence and the pre-change backup: `C:/Users/Dev/.codex/tmp/statistics-popup-fix-20261021/` (`checks.txt`, `behaviors.txt`, `saved-audit.txt`, phone captures and `final-390x844.png`). Shared font assets match the pre-change backup. The disposable QA scene was removed. Unity is in Edit Mode with Main open and the Statistics window selected. Verification covers Editor rendering and native interactions; no Android-device build was performed.

Statistics follow-up, 21 October 2026: moved the close button to a 22-unit inset from both the top and right edges, clearing the painted border/corner artwork. Retained its 48×48 touch target and centered the two-line title within a narrower reserved region. Saved Main and the scoped authoring method use the same offsets. Game View checks at 390×844 and 360×540 confirm frame/title clearance, the complete touch target and no title overflow; reloading Main retains the inset. Updated preview and pre-change backup: `C:/Users/Dev/.codex/tmp/stats-close-clearance-20261021/final-390x844.png` and the sibling `backup/` directory.

### H2U-014 Settings sliding switches — HTML only, 21 October 2026

The user requested left/right switches instead of flat state buttons, then explicitly selected **HTML preview only**. Music, Sound effects and Reduced motion now use parchment/cobalt pill tracks with separate cream/gold knobs: left is OFF, right is ON. Existing Setting actions and control IDs remain. State changes update the same DOM controls, preserving focus and allowing the 180 ms slide; reduced motion removes the transition immediately. No Unity changes were made for this revision.

Browser checks pass mouse toggling, Music activation with Space, Reduced motion activation with Enter, a visible 3 px keyboard focus outline, independent values, readable ON/OFF labels, 74×48 hit regions and exact left/right knob positions. Reduced motion produces zero-duration track/thumb transitions. Reloaded markup has three switch controls, unique IDs and valid `settingsSwitches` metadata targets; native presentation is explicitly marked `existing-needs-adaptation`. JavaScript syntax checks pass. Evidence: `C:/Users/Dev/.codex/tmp/settings-switches-20261021/html-preview.png` and `review.json`. Unity transfer remains pending under the user's HTML-only scope.

### H2U-013 shared close-button clearance — HTML and Unity follow-up, 21 October 2026

The user reported border/close-button overlap across the popups. Applied a shared fix to all existing HTML popup routes: the close control is now a direct child of its card, rather than positioned against differing headers. `popup-clearance.css` gives dismissible popups a consistent 48×48 close target, a 22 px inset, symmetric title gutters and wrapping. The wider rarity-card artwork uses a 36 px inset plus a reserved internal band above the title/level; its art area adjusts inside the same card. Recovery views that require an explicit choice still have no close button. Existing action IDs, dismissal/focus owners and settings switches remain intact.

Changed `UI Builder/HTML/Classic/classic.js`, added `popup-clearance.css`, and updated stylesheet/script versions in `index.html`. The user subsequently requested the same correction in Unity. `popupCloseClearance` now records the verified native presentation. The separate H2U-014 Settings sliding switches remain HTML-only under the user's explicit scope.

**HTML and native Unity implemented.** Browser checks pass all **29 popup routes**, plus the three skill-detail and common/rare/legendary card variants (34 total checks including required-recovery exclusions). Every dismissible close target is clickable, at least 48×48, inset from its frame and clear of its title/level. No checked body has horizontal overflow. Nested Guidance closes back to ProductDetail and restores the invoking tip's focus; cash stays unchanged. HTML evidence remains at `C:/Users/Dev/.codex/tmp/popup-close-clearance-html-20261021/`, captured at the user's 964×884 browser viewport.

Native changes are saved in `Assets/Scenes/Main.unity` and `Bootstrap.unity`. `ClassicPopupClearanceAuthoring` authors the same 22-unit inset and 48-unit control into every dismissible Main dialog, symmetric 74-unit title gutters, flowing headers, and a 36-unit card inset with a 52-unit internal band above card headings. Bootstrap uses its existing 3× reference scale (66 inset, 144 control) and reserves content below the close. Existing close objects, action owners, recovery exclusions, native Settings controls, sprites, gameplay and services are preserved. The scoped Skills, Statistics and HTML-sync Editor builders apply the shared clearance last so subsequent refinements retain it. No runtime UI construction or geometry mutation was added.

Native verification: all 29 Main routes plus three explicit skill-detail and three rarity-card selections pass **35 geometry checks at each of 1080×2400 and 1080×1620** (the base route checks also include a default skill/card selection). Checks cover insets, control size, title/body separation, viewport fit, native close bindings and mandatory-recovery exclusions. The isolated behavior suite passes nested dismissal/focus restoration, input blocking, recovery guards, all ordinary popup routes, skill guards and stable preplaced hierarchy. Bootstrap's isolated copy passes close raycasting, frame/content clearance and viewport checks at both resolutions; production callback references remain intact. No live account or player profile was used. Full-resolution screenshots and reports are at `C:/Users/Dev/.codex/tmp/shared-popup-clearance-unity-20261021/`. Shared TMP fonts remain byte-identical to this task's baseline; the disposable QA scene is removed after verification.

### H2U-015 Shop cleanup — HTML only, 21 October 2026

Removed the three elements selected in the user's browser comments: **Your collection**, the **Card draws** information card, and **See card odds**. `classic.js` now renders the KK balance, store availability notice and four existing pack rows. Surviving purchase controls keep their original IDs, parameters and disabled guards; Legacy's navigation tab and card-draw information entry remain available. Updated the script cache version in `index.html` and the embedded `shopPresentation` binding, which records native transfer as pending.

Verified in the running preview at the user's 964×884 viewport: all three removed texts are absent from Shop, all four pack controls retain their IDs and parameters, the 100 KK fixture balance remains visible, no horizontal overflow is present, and the browser error log is empty. Captured top and bottom views plus `checks.json` at `C:/Users/Dev/.codex/tmp/shop-cleanup-html-20261021/`. JavaScript syntax and whitespace checks pass. No Unity asset or runtime file was changed for this HTML revision.

### H2U-016 shared collapse chevron — HTML only, 21 October 2026

Applied the selected editable SVG down chevron on a cobalt circular face with a gold rim to **Player, Products, Legacy and Shop**, following the user's request to use it in all four places. Each existing `collapse-[panel]` button, accessible name, `Tab` / `Home` action and keyboard behavior is preserved. Shared CSS retains a 48×48 logical hit area and keeps the 36×36 icon fully inside each header. Updated stylesheet/script cache versions and expanded the embedded `panelCollapse` binding to all four controls; native presentation remains pending a separately requested Unity transfer.

Verified all four panels in the user's 964×884 running browser preview: each control is clickable, collapses its panel to Home, and leaves cash unchanged at the fixture's ₹1.24M. Each hit area measures 47.99×47.99 logical pixels after browser rounding; each icon stays inside its header and receives pointer hits. The earlier Shop click/Enter check also remains recorded. Four panel screenshots and `checks.json` are at `C:/Users/Dev/.codex/tmp/shared-chevrons-html-20261021/`; the initial Shop-only evidence remains at `C:/Users/Dev/.codex/tmp/shop-chevron-html-20261021/`. JavaScript syntax and whitespace checks pass. No native Unity file was changed.

### H2U-015–016 native transfer for the Android build — 21 October 2026

The user requested an Android build and phone test after reviewing these revisions. Transferred both to saved Main through `ClassicPanelControlsAuthoring`: the three retired Shop objects are preserved inactive, and each panel's original collapse button now has a saved antialiased chevron Image, a 48×48 target and its existing Tab callback. `BuildClassicPanels` and the HTML-sync refinements retain the changes on future authoring. The geometric icon sprite is local at `Assets/Game/ClassicUI/HTMLSync/collapse-chevron.png`; no runtime UI creation was added. Settings switches remain HTML-only under the user's earlier explicit constraint.

Isolated native verification at 1080×2400 passes all four collapse callbacks, all four saved icons and target sizes, the three Shop removals, and unchanged component count through navigation. The disposable QA scene was removed before building; Main retains production startup and no preview driver/probe. Native screenshot and this task's baseline are at `C:/Users/Dev/.codex/tmp/android-phone-build-20261021/`. Android build and physical-device results are recorded separately in the ClassicAndroid build/test report.


### 25 October 2026 — sync HTML to the current project

| ID | Area | Current project behavior mirrored in HTML | Files | Unity owners | Status |
|---|---|---|---|---|---|
| H2U-017 | Fresh economy and developer reset | Re-export six fixtures from the current C# core: fresh cash ₹0, base tap ₹1, initial tap training ₹10. The opened-shop fixture earns ₹30 through native tap commands before buying Phuljhari. Help explains that start. Mirror the development-only Settings reset row and confirmation with guest/session guards, fresh settings and Home return. Default preview represents production; append ?build=development to inspect the Editor/development control. Reset changes disposable browser data only. | Tools/ClassicUiPreview/Program.cs; UI Builder/HTML/Classic/fixtures.js, classic.js, index.html | EconomyBalance; RedesignState; RedesignEngine; RedesignRuntimeDriver.ResetDevelopmentGame; ClassicDeveloperResetAuthoring; ClassicUI | Done: native economy/reset already present; current Help copy now agrees with the HTML. |

The previously requested sliding Settings switches (H2U-014) remain the reviewed HTML variant; their native transfer is still pending. This sync does not change that item's status. Existing baseline snapshots are preserved.

Verification: regenerated six fixtures successfully; 44 viewport/route checks and existing current-UI assertions passed. Focused browser checks passed for exact fresh economy values, 30 native taps before first stock, production hiding/direct-route guard, cancellation, Google/save-error/pending-pack guards, reset defaults and Home return, and confirmation fit at 360×540 and 390×844. No browser exceptions. Settled visual evidence: ClassicHTMLSync/2026-10-25-fresh-home.png and ClassicHTMLSync/2026-10-25-developer-reset.png.


### 25 October 2026 — twelve browser review comments

| ID | Area | Request and concrete changes | HTML files | Likely Unity owners | Status |
|---|---|---|---|---|---|
| H2U-018 | Shared header, Player, Products, Legacy, Shop, DrawReveal and ProductDetail | Addressed browser comments 1–12 sequentially: square Settings; correct misplaced disabled buy buttons; replace Player/Products/ProductDetail guidance banners with header info controls; remove Legacy, Shop and reveal banners; remove achievement-count parentheses; reserve product-button space; add twelve generated upgrade-type icons with separate icon/copy/button columns; restore product-title contrast. Existing Tip actions remain. | Classic/classic.js; parity/native.css; popup-refresh.css; popup-clearance.css; index.html; assets/upgrades/*.png and PROVENANCE.json | ClassicUI / ClassicGuidance / ClassicUI.Achievements; saved Main Canvas; BuildClassicPanels; BuildClassicDialogs; ClassicPresentationAuthoring; ClassicPopupClearanceAuthoring. Native owner mapping to be confirmed during transfer. | Done in HTML and saved Unity Main, 25 October 2026. Achievement shortcut is label-only, with no count. |

Per-comment evidence, exact affected files and image provenance: [browser comment record](ClassicHTMLSync/2026-10-25-browser-comments.md). Baseline snapshots preserved.


| H2U-019 | Legacy / Kaju Katli balance icon | User requested three icon variants to replace the KK text and will select one. Generated Single silver diamond, Stack of sweets, and Festival token; review page shows all three at 24/32 px and beside a 100 balance. Follow-up: user requested a diamond look; added a faceted silver-blue gem as a separate review revision (diamond.html, 04-diamond-gem.png, diamond-provenance.json). | Classic/reviews/kaju-katli-20261025/index.html, 01-single-diamond.png, 02-sweet-stack.png, 03-festival-token.png and PROVENANCE.json | ClassicUI.Bindings (legacy-kk), saved Legacy header and associated Editor authoring | Done: selected option 3 imported byte-for-byte and used by native balances, draw costs, packs, rewards and prose. |

H2U-019 implementation: selected source exec-0151ff95-b7d4-48d5-8064-2ea6f747f385.png copied byte-for-byte to Classic/assets/currency/kaju-katli.png. Shared currency.js/currency.css render it in Legacy and Shop balances, draw costs, pack art/amounts, purchase details, achievement balances/rewards and guidance/prose that formerly displayed KK. The full Kaju Katli name remains accessible. Classic/index.html and Achievements/index.html load the shared module; classic.js and achievements-ledger.js use it. Current numeric state, purchase/draw rules and source fixtures are unchanged. Native owners: ClassicUI.Bindings, ClassicUI.Achievements, ClassicGuidance, saved Legacy/Shop/popup/achievement views and their Editor authoring scripts.

H2U-019 verification: 27 currency-specific browser cases passed across 964×884, 360×540 and 390×844 for Legacy, Shop, Achievements, Purchase, Prestige, BuyAll, Odds, Help and standalone Achievements. All rendered currency images use the selected asset; no visible KK abbreviation remains; amounts retain accessible names, fit their controls and retain original balance/draw prices. All fifteen achievement rewards and all four shop packs use the token. A live claim changes 100 to 110, keeps the token and disables the claimed reward. Source PNG hash/bytes match the attachment. Evidence: ClassicHTMLSync/2026-10-25-currency-checks.json; screenshot: ClassicHTMLSync/2026-10-25-selected-currency.png.


| H2U-020 | Shared popup frame | Remove duplicate outer popup outlines and fit the retained inner frame. Standard dialogs use only the painted frame at inset 0, 8 px shell padding and a soft shadow. Skills and SkillDetail retain one inset gold line; outer blue border/gold ring removed while content insets stay equivalent. Common card popup loses the extra painted overlay and keeps its original rarity frame. | Classic/popup-refresh.css, skills-popup.css, skill-hero.css, classic.js metadata, index.html; Achievements/index.html cache revision | Saved shared dialog backgrounds, skill popup and card presentation; BuildClassicDialogs, ClassicHTMLSyncAuthoring, ClassicPopupClearanceAuthoring (owner mapping to confirm at transfer) | Done: single clipped popup frame and one inner gold line on Skills/SkillDetail; original rarity frames retained. |

H2U-020 validation: all 96 popup frame/layout cases passed (32 routes/variants × 964×884, 360×540, 390×844), including common/rare/legendary cards and standalone Achievements. Checks confirm zero outer CSS borders, painted-frame alignment at inset 0, no duplicate outline shadows, original rarity frames retained, body/close containment and no horizontal overflow. Live dismissal/reopening works. Evidence: ClassicHTMLSync/2026-10-25-popup-frame-checks.json; screenshots: 2026-10-25-single-popup-frame.png and 2026-10-25-single-frame-card.png. JavaScript syntax and Git whitespace checks pass.

H2U-020 follow-up: removed the pale strip outside the painted frame requested in the next browser comment. The frame source includes a small exterior margin; its pseudo-element now bleeds 4 px beyond the shell and is clipped by the existing rounded, overflow-hidden card. The visible border meets the popup edge without changing content, controls or the image asset. Updated popup-refresh.css and Classic/Achievements cache revisions; Unity still pending.

Follow-up validation: 10 representative popup checks passed at desktop and short-phone sizes, including Settings, ProductDetail, Welcome and both Achievements previews. Visible border is flush with the clipped shell; controls and body remain contained. Evidence: ClassicHTMLSync/2026-10-25-popup-edge-checks.json and 2026-10-25-popup-frame-flush-edge.png.


| H2U-021 | Shared information controls | User selected the existing Legacy i button as the common style. Player, Products and ProductDetail info controls now match its 23 px blue circle, 2 px outline, bold 16 px Georgia i, and 48 px hit region. Remaining guide-row info glyphs match the same symbol; existing actions, topics, accessible names and locations retained. | Classic/information.css, classic.js binding metadata, index.html; Achievements/index.html shared CSS | ClassicUI / ClassicGuidance, saved header and guide controls; associated Editor authoring | Done: shared 23-unit blue circled i, 48-unit entry targets and existing guidance routes. |

H2U-021 validation: computed styles match Legacy for Player, Products, ProductDetail and remaining guide glyphs (23×23 px circle, 2 px blue ring, Georgia 16 px). Product popup target measured 48×48 px; the shared button rule retains 48 px targets throughout. Clicked Player information (Buy affordable upgrades), Products/ProductDetail information (Choose your quantity), and Legacy information (Every card has a chance). Screenshot: ClassicHTMLSync/2026-10-25-shared-info-button.png.

H2U-018 follow-up: the Player Achievements shortcut now displays only Achievements, with no ready-count number or parentheses. Counts remain inside the achievement ledger. H2U-021 follow-up: removed obsolete gold-circle CSS rules from parity/native.css and popup-clearance.css so information.css is the shared symbol definition for Player, Products, ProductDetail and guide rows. Legacy retains the selected blue circle/i. Cache revisions bumped for all affected presentation sources. HTML complete; Unity pending.


| H2U-022 | AFK / Offline popup | User selected variant 2 Capped Timeline on 25 October 2026: gold stored time, grey overflow, actual cap marker, efficiency ring, normal/ad comparison, compact header i, Watch ad for 2× rewards and Continue. | Classic/classic.js; offline-timeline.css; index.html; parity/guide.js | ClassicOfflineTimelineAuthoring; ClassicOfflineView; ClassicUI.Offline; OfflineTimeline; RedesignEngine.RewardOfflineAd; Main.unity | Selected layout and receipt-safe bonus flow implemented in native production Main and HTML. Active ad SDK/ad unit remains unconfigured; native Watch is disabled with an unavailable message. Continue retains settled cash. |

H2U-022 validation: header info opens return-play guidance; no large banner; exactly one footer Continue; old footer buttons absent. Continue acknowledges the receipt and closes without route navigation (Home before and after), reopening shows Earnings collected and does not change the frozen balance (₹1.24M before/after). Save-error disables Continue. AFK frame/layout checks pass at 964×884 and 360×540. Evidence: ClassicHTMLSync/2026-10-25-afk-continue-checks.json and 2026-10-25-afk-continue.png. JavaScript syntax and whitespace checks pass.

H2U-022 revision: user requested undoing the button changes. Restored View your next step (next-step navigation) and Continue to your shop (close only), removed the new preview collection flag, and restored already-credited receipt/body/help wording. Kept the small shared header i button and removal of the large banner. This revision supersedes the single-Continue footer requirement above. HTML updated; Unity remains pending for the info-button presentation only.

H2U-022 latest revision: replace View your next step with Watch ad for 2× rewards; rename Continue to your shop to Continue. HTML models watching, rewarded, cancelled, failed and unavailable outcomes; only delivered while an active attempt is watching doubles the displayed base reward once. Continue closes in place and cancels any pending attempt; later completion without an active attempt is ignored. Outcomes are controlled through the existing review Interaction state selector; no ad SDK or real reward/save/network call runs in HTML. Copy and return-play help updated. A new unresolved OfflineAd action documents native receipt ownership/idempotency requirements. Native rewarded-ad integration is pending and must add exactly one extra base amount on verified completion, never replay base settlement.

H2U-022 ad revision validation: watch action enters Watching ad without changing base ₹147.45M; delivered changes it to ₹294.90M and disables further reward. Repeating delivered retains ₹294.90M. Failed keeps ₹147.45M. Continue during watching closes; a later delivered outcome after cancellation is ignored, and reopening retains ₹147.45M. Desktop/short-phone popup checks pass. Evidence: ClassicHTMLSync/2026-10-25-afk-rewarded-ad.png and 2026-10-25-afk-rewarded-ad-layout.json. Ad outcomes are simulated through Interaction state in this HTML preview; real SDK and native receipt persistence remain unimplemented.

H2U-022 visual exploration: user requested three variations of the current AFK screen. Generated Coin hero, Shopkeeper welcome, and Reward comparison references under UI Builder/Reference Images/Offline/Variants/2026-10-25; comparison gallery under Classic/reviews/offline-variants-20261025. User selection pending. No replacement screen HTML or Unity layout changes performed. Existing reward values and optional ad/Continue actions preserved in all three concept prompts.

H2U-022 fill-bar exploration: user suggested a fill bar that presents AFK information visually. Added an isolated interactive design study at Classic/reviews/offline-fill-meter-20261025/index.html. The bar measures credited time versus cap; the current example shows 8/8 h stored, 10 h away, 2 h beyond cap, 50% earning efficiency and ₹147.45M base / ₹294.90M with ad. Efficiency uses a small ring and changes earnings rather than falsely showing half the time stored. Review controls vary absence, cap, efficiency and completed-ad state. The study uses the progressed fixture normal income (10239.64453125/s). This is a proposal; user acceptance pending, main Classic HTML and Unity unchanged.

Fill-meter study checks: default 10h absence / 8h cap / 50% efficiency displays full time storage, 2h beyond cap and ₹147.45M. Changing absence to 4h produces a 50% bar and ₹73.73M. Increasing efficiency to 100% and previewing the ad doubles earnings to ₹294.90M while the time bar remains 50%. Only the isolated study was tested/changed; no main gameplay state is involved.

H2U-022 fill-meter references: generated three requested variants, Segmented storage, Capped timeline and Vertical vault. References saved under UI Builder/Reference Images/Offline/FillMeterVariants/2026-10-25 and served in Classic/reviews/offline-fill-variants-20261025 with exact prompts. User selection pending. No main Classic or Unity implementation changes.

H2U-022 accepted native revision — 25 October 2026: the user selected “2. Capped timeline” and explicitly requested implementation in the game. This supersedes the pending-selection notes above. Saved Main now owns a native, editable Offline popup with live absence, credited time, the receipt’s frozen cap, efficiency and reward amounts. The prior complete root is preserved at Local dialogs/Offline__LegacyDisabled_20261025. Continue and X acknowledge the already-paid receipt and close without route changes. The header i opens the existing Guidance reader even after a topic has been learned. The ad callback flow adds one extra base reward at most once, with persistent adRewarded state and receipt/session/request guards; there is no configured live advertising provider. Native Watch stays unavailable, and no simulated provider is shipped into gameplay. Browser ad controls remain explicitly disposable review simulation.

Validation and screenshots: Documentation/Implementation/OfflineCappedTimeline. Core regression: 839 checks passed. Native production-route checks cover saved hierarchy, auto-show, actual receipt data, animation settling, disabled ads, fake-provider cancellation/completion/duplicates/late callbacks, durable bonus, Continue, info reader, short/tall viewports, improved Talent values and reduced motion. Real ad-network/device delivery remains untested and requires a provider and ad unit. Historical parity/before files are preserved unchanged.

Red-dot inspection — 25 October 2026: user requested to see every CTA’s current red-dot state. Added a review-only gallery at Classic/reviews/red-dot-states-20261025, with 27 visible CTA appearances across 14 native screen/scroll views, drawn from 83 existing saved bindings. ON uses actual Unity readiness rules; OFF hides only the marker for comparison. Repeated list rows have representative examples. This does not modify the main HTML preview or native notification behavior; it is an inspection of the current Unity state. Details: Documentation/Implementation/Classic-RedDot-States-20261025.md.

## Native transfer — 25 October 2026

The user requested “Apply the changes to unity” after reviewing the Classic HTML and red-dot states. The remaining Classic presentation revisions are now applied in the existing saved Main scene. This supersedes the earlier HTML-only/pending notes for H2U-014 and H2U-018–021; completed AFK and existing red-dot logic remain in place.

The transfer includes the square Settings target, separate upgrade icon/copy/purchase columns, twelve generated effect icons covering all 1,000 upgrades, product action gutters, compact Player/Products/ProductDetail information, removed Legacy/Shop/reveal banners, label-only Achievements shortcut, selected token currency throughout, single popup frames and the three animated Settings switches. Current Help copy agrees with the zero-cash / one-rupee tap start.

Native rewarded-ad restoration — 26 October 2026: the user explicitly requested working ads and supplied Android AdMob App/Rewarded Unit IDs. This supersedes H2U-022's earlier disabled-provider status. Classic now attaches Google Mobile Ads 11.5.0 with UMP consent, selected-platform test units in Editor/development, bounded loading retries, receipt-preserving native pause/resume and same-owner cloud lease renewal. Settings has an Editor-authored ad privacy entry shown when UMP requires it. The SDK Editor completion flow passed 12 checks, including the Settings scroll layout; the prior offline regression suite passed 30 checks; core suite passed 864 checks. These are integration/simulation checks, not device network delivery. See `RewardedAds/README.md`, `RewardedAds/Validation.txt` and `RewardedAds/AndroidBuild.txt`. No HTML redesign was needed for this native SDK integration.

Implementation: ClassicRevision20261025Authoring, ClassicCurrencyPresentation, ClassicSettingsSwitch, existing ClassicUI/ClassicGuidance bindings and Main.unity. Every UI object and material slot is Editor-authored. Only currency-bearing text receives extra sprite slots; the header balance uses an explicit saved token Image. Full byte backups preserve the prior uncommitted Main and source files under the task temporary directory.

Evidence and reproducible validation: ClassicHTMLSync/Unity20261025. All 839 core checks pass. Native verification covers live currency/rewards, info reentry, actual effect artwork, independent row columns, popup frames, switch motion/persistence, responsive and notched layouts, fresh stocking and stable UI identities. No live user profile, Firebase setup, ad SDK or balance rule was changed. Native ad availability remains as recorded in the earlier AFK implementation.

## Home tap banner — 26 October 2026

User requested a Tap to earn banner in the marked region at the bottom of the shop scene. Added editable TAP TO EARN text in a compact cream banner with cobalt lettering and gold border, centered over the existing stall tap target. Existing pointer/keyboard Tap behavior and motion hook remain in use. Files: UI Builder/HTML/Classic/index.html and parity/native.css. HTML candidate ready for review; native implementation pending separate user direction. Likely Unity owners: saved Main world/stall hint and Editor Classic scene authoring; reuse the existing Tap action binding.


Tap banner art exploration — 26 October 2026: user requested a generated background to improve the banner. Generated a transparent gouache ivory/cobalt/gold plaque with vermilion accents and blank center for editable text. Reference saved at UI Builder/Reference Images/Home/TapBanner/2026-10-26/tap-to-earn-background.png. Art reference ready for review; existing HTML banner unchanged during generation. Native transfer remains pending.


Tap banner art applied — 26 October 2026: user approved the generated background with 'use it in the html'. Copied the approved PNG into Classic/assets/tap-to-earn-banner.png and applied it through parity/native.css to the existing tap-hint, with editable cobalt text, responsive dimensions and transparent painted edges. Updated the stylesheet cache version in index.html. Existing stall Tap pointer/keyboard binding retained. HTML ready for review; Unity hint/scene authoring transfer remains pending separate direction.


Tap banner follow-up — 26 October 2026: user requested matching typography and half-on-image/half-below placement. Updated parity/native.css and index.html cache version: Native Heading typeface, plaque centered across the stall/paper seam, visible overflow, extended existing stall tap target to cover the lower half, and lowered the Home next-display/help row for clearance. HTML candidate pending review; native saved hint font/RectTransform, tap target and Home next/help layout owners remain pending separate transfer.


Welcome concept variants — 26 October 2026: user requested three generated variants of supplied welcome screen. Generated Painted shop hero, Welcome invitation, and Immersive stall with the existing welcome/account copy, close, Google sign-in and guest actions. Saved under UI Builder/Reference Images/Welcome/Variants/2026-10-26. Concept selection pending; no main HTML or Unity changes. Existing Welcome/Account authoring and bindings are likely native owners if a later transfer is requested.


Welcome background exploration — 26 October 2026: user selected Immersive stall as the preferred UI direction and requested more backgrounds. Generated three standalone text-free backgrounds: Lantern bazaar, Sunset courtyard and Festival night, under UI Builder/Reference Images/Welcome/Backgrounds/2026-10-26. Preferred layout recorded; background selection pending. No main HTML or Unity changes.


Welcome background salesman revision — 26 October 2026: user requested the existing salesman in the generated backgrounds. Created three sibling *-salesman.png variants using HTML/Home/assets/gouache-shopkeeper.png as character identity reference. Same kurta/stole, hair and moustache, welcoming gesture, integrated behind counter. Saved in Welcome/Backgrounds/2026-10-26, preserving prior art. Background selection pending; no main HTML or Unity changes.


## Immersive Welcome / Account HTML — 26 October 2026

User explicitly requested the selected festival-night salesman background with the previously approved Immersive stall UI in HTML. Applied to existing Welcome and Account routes: full-screen selected background; separate editable gold/cobalt brand plaque; curved ivory lower welcome panel; large Google and outlined guest controls; existing close and account preview outcomes. Asset: Classic/assets/welcome-festival-salesman.png. Sources: Classic/classic.js, parity/startup.css, popup-refresh.js, index.html cache versions. Bypass the older welcome-card decorator for these two routes. Restored the missing Account action dispatch to its existing accountAction owner so Google opens existing progress choice and guest returns to Home. No real sign-in or account calls were added.

Verified JS syntax; desktop layout; 360x640 no horizontal overflow and both controls approximately 60px tall; salesman visible above panel; Google progress-choice preview, guest return, Close, and shared Account rendering. HTML candidate ready for review. Unity transfer pending separate direction: saved Welcome/Account art, brand and lower-panel layout; existing Classic startup/dialog authoring and ClassicUI account/close bindings. Original reference assets and baseline preserved.


Immersive Welcome frame correction — 26 October 2026: user supplied a crop showing that the first HTML panel did not match the approved reference. Replaced the elliptical top with an editable SVG scalloped crown and central point, nested ivory/cobalt/gold trim, and a separate vermilion/gold floral crest. Changed welcome heading to Native Card Serif with lighter weight and reference-like proportions. Files: Classic/assets/welcome-panel-frame.svg, welcome-floral-accent.svg, parity/startup.css, index.html cache version. Selected salesman background and account actions retained. Verified desktop screenshot plus 360x640: no horizontal overflow, both controls 48px tall, footer contained. HTML corrected candidate ready for review; Unity panel frame/crest and heading authoring remain pending separate transfer.


Welcome title sign correction — 26 October 2026: user supplied an ornate shop-sign crop and requested that header appearance. Generated and applied Classic/assets/welcome-brand-sign.png (built-in image_gen, crop used as style reference): cobalt painted plaque with sculpted gold rim, arched firework crest and floral ornament. Replaced rounded CSS header with this transparent art; retained editable tall gold serif ROCKET RAJA title and small gold subtitle. Sources parity/startup.css and index.html cache version. Existing Welcome/Account actions unchanged. HTML candidate ready for review; Unity saved brand sign art, title font and layout remain pending separate transfer.


Welcome sign follow-up — 26 October 2026: user requested removal of YOUR FIREWORKS STORY STARTS HERE. Removed the subtitle from the shared Welcome/Account nativeStartup markup in Classic/classic.js and refreshed its index.html cache version. Painted sign and ROCKET RAJA heading retained. HTML ready for review; native saved Welcome/Account subtitle removal pending separate transfer.


## Native transfer completed — 26 October 2026 welcome/tap revision

User requested 'Implement this changes in the unity'. Applied this session's reviewed welcome/account design and Home tap banner in the existing production Bootstrap and Main scenes. This supersedes the pending-native statuses for the Home tap banner, Immersive Welcome, scalloped frame, painted title sign and tagline removal above. Existing account callbacks, comparison/loading/recovery states, guarded close/reopen and authoritative tap behavior remain in use. All presentation objects are Editor-authored and saved; complete prior Bootstrap Canvas and old prompt remain inactive for inspection. No live profiles or auth services were used during verification.

22 focused welcome/banner checks and 121 existing production-flow checks passed. Final 360x540, 360x640, 390x844 captures, safe-area audit, source mapping and limitations: Documentation/Implementation/Welcome20261026/README.md. Unity authoring owner: ClassicWelcome20261026Authoring; runtime changes are limited to the existing layout preferred-size input, welcome copy and tap prompt wording. No Android build produced.

