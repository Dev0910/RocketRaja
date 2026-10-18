# Classic HTML → Unity TODO

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

## HTML changes awaiting Unity

**Thirteen changes recorded across Home, Player, Skills and shared popups on 19 October 2026.** HTML is implemented and awaiting the user's review. Unity implementation is pending.

| ID | Screen / component | Required change from baseline | HTML evidence | Unity target | Status |
| --- | --- | --- | --- | --- | --- |
| H2U-001 | Home / stall hint (comment 1) | Remove the visible “TAP THE STALL TO EARN” strip. Preserve the stall's tappable area and accessible label. | `UI Builder/HTML/Classic/index.html`; removed `.tap-hint` | Saved stall hint object in Main; confirm owner in `Editor/Classic/BuildClassicUI.cs` and `Runtime/Classic/ClassicLayout.cs` | Awaiting HTML review |
| H2U-002 | Home / earnings icon (comment 2) | Replace the coin-stack illustration with a gold rising earnings chart, shown beside the combined rate's heading. Gold chart is an editable inline SVG in HTML. | `UI Builder/HTML/Classic/classic.js` (`earningsIcon`); `parity/native.css` | Existing income icon slot / new native icon asset and authoring; check `Editor/Classic/BuildClassicPanels.cs` | Awaiting HTML review |
| H2U-003 | Home / rate labels (comments 3–5) | Remove the separate “/ SECOND”, “Normal sales” and “Counter taps” lines. Show compact currency with `/sec` for automatic sales and `/tap` for tap payout, alongside each amount. | `UI Builder/HTML/Classic/classic.js` (`home`, `rateAmount`); `parity/native.css` | `Runtime/Classic/ClassicUI.Bindings.cs`; saved income labels and `Editor/Classic/BuildClassicPanels.cs` | Awaiting HTML review |
| H2U-004 | Home / income layout and combined rate (comment 6) | Stack automatic sales and per-tap payout on the left. On the right, show “EARNING NOW”: current automatic income plus accepted manual tap earnings in the preceding one second. Update as taps occur and expire; at rest show automatic income. | `UI Builder/HTML/Classic/classic.js` (`homeNormal`, `homeTap`, `totalEarnings`, `updateTotalEarnings`, `doTap`, binding metadata); `parity/native.css` | `Runtime/Classic/ClassicUI.Bindings.cs`, `ClassicLayout.cs`; use accepted game tap earnings and current engine income, not raw UI clicks | Awaiting HTML review |
| H2U-005 | Home / skill buttons (comment 7) | Ready Home skill buttons activate the corresponding skill directly without opening SkillDetail. Locked, already active, cooldown and save-blocked buttons remain disabled. Detail screens remain reachable through other routes. | `UI Builder/HTML/Classic/classic.js` (`home`, `ActivateSkill`); `parity/native.css` | Saved Home button bindings in `Editor/Classic/BuildClassicPanels.cs`; `Runtime/Classic/ClassicUI.Bindings.cs`, existing `RedesignEngine.ActivateSkill` | Awaiting HTML review |
| H2U-006 | Stall / shop-name alignment (second batch, comment 1) | Center the shop-name text horizontally and vertically inside the painted plaque. Correct the line box and vertical anchor; keep the name editable. | `UI Builder/HTML/Classic/parity/native.css` (`.shop-title`) | Saved shop-name RectTransform and TMP alignment in Main; `Editor/Classic/BuildClassicUI.cs`, `Runtime/Classic/ClassicLayout.cs` | Awaiting HTML review |
| H2U-007 | Home / skill duration and cooldown rings (second batch, comment 2) | All ready skills have full gold rings. Activation drains the gold ring as a duration clock. When the duration expires, grey the icon and fill a grey radial cooldown; return to full gold and enable the button at readiness. Bulk Order is instant and starts grey cooldown immediately. Use actual remaining mm:ss and independent clocks; locked skills have no gold ready ring. Depends on H2U-005. | `UI Builder/HTML/Classic/classic.js` (`skillClock`, `skillPresentation`, `skillRing`, `tickSkills`, `ActivateSkill`, binding metadata); `parity/native.css` | Editor-authored radial Filled Images around saved Home skill icons in `Editor/Classic/BuildClassicPanels.cs`; live timers from `RedesignState.activeUntil` / `readyAt`, bound by `Runtime/Classic/ClassicUI.Bindings.cs`; reduced-motion handling in Classic motion owners | Awaiting HTML review |
| H2U-008 | Player / Stats and Skills shortcuts (third batch, comment 1) | Remove the painted button frames/backgrounds; retain the icons, text, clickable areas and existing navigation actions. | `UI Builder/HTML/Classic/parity/native.css` (`.classic-toolbar .player-icon`) | Saved Stats/Skills button background Images in `Editor/Classic/BuildClassicPanels.cs`; preserve Button/listener and icon/text objects | Awaiting HTML review |
| H2U-009 | Player / upgrade purchase-button gutter (third batch, comment 2) | Bring upgrade purchase buttons inside their row frames with a consistent 10-logical-pixel right inset. Include padding within the button width and reserve space for the upgrade text. Apply the same rule to the shared catalogue upgrade-row template. | `UI Builder/HTML/Classic/parity/native.css` (`.upgrade-row .row-action`, `.upgrade-row .plain-detail`) | Upgrade-row RectTransforms and right anchors in `Editor/Classic/BuildClassicPanels.cs`; existing shared catalogue row authoring and `Runtime/Classic/ClassicLayout.cs` | Awaiting HTML review |
| H2U-010 | Skills / selected Quick action list popup | Implement selected concept 1: three stacked painted-icon rows, each with name, live ready/active/cooldown/locked status, effect, duration/cooldown, direct Activate and Details. Preserve the shared gold/grey clocks from H2U-007. Replace the oversized guidance banner and Back footer with a subtitle, close X and compact prestige note. Details returns to its invoking row without resetting timers. | `UI Builder/HTML/Classic/index.html`, `classic.js`, new `skills-popup.css`; selected reference `UI Builder/Reference Images/Skills/Popup Variants/2026-10-19/01-quick-action-list.png` | Existing Classic Skills popup in `Editor/Classic/BuildClassicDialogs.cs`; `Runtime/Classic/ClassicUI.cs` / `ClassicUI.Bindings.cs`; existing core `PreviewSkill`, `ActivateSkill`, `activeUntil` and `readyAt` | Awaiting HTML review |
| H2U-011 | Individual skill / selected Hero card | Apply selected concept 1 to all three skills: large painted art, live ready/active/cooldown/locked ring and status, two skill-specific effect tiles, duration/cooldown and one activation button. Remove View skill upgrades for every skill. Keep close/back-to-list, timing semantics and existing activation guards. | `UI Builder/HTML/Classic/classic.js`, new `skill-hero.css`, `index.html`; selected `UI Builder/Reference Images/SkillDetail/Popup Variants/2026-10-19/01-hero-skill-card.png` | Existing Classic SkillDetail authoring and view binding; reuse `RedesignEngine.PreviewSkill` / `ActivateSkill` / `activeUntil` / `readyAt` | Awaiting HTML review |

| H2U-012 | Shared popup dismissal | Clicking or tapping anywhere outside the top popup card invokes the existing Close path once, restoring its parent popup or invoking screen and focus. Inside clicks keep it open. Consume the outside click to prevent underlying navigation or stall taps. Retain existing mandatory SaveRecovery / RecoveryExit choice guards. | `UI Builder/HTML/Classic/classic.js`, `index.html` navigation/QA metadata | Shared modal backdrop listener and close/history/focus behavior in `Runtime/Classic/ClassicUI.cs`; saved dialog backdrop in `Editor/Classic/BuildClassicDialogs.cs` | Awaiting HTML review |

| H2U-013 | All existing popups / generated-reference refresh | Cover all 29 popup routes with the cream/cobalt/gold Gouache Storybook treatment, readable flowing copy, painted artwork, safe title/close clearance, semantic content and scrolling on short screens. Use 26 new distinct ChatGPT-generated references; Account shares Welcome, and retain the selected Skills list/Hero references. Introduce illustrated confirmations, product/milestone panels, statistics ledger, catalogue/receipt rows and appearance previews. Make Welcome/Account compact dismissible modals, show CardDetail close X, preserve rarity frames and all existing action parameters. Remove the bottom Back button from Settings at the user’s explicit follow-up; retain X and outside close. Native recovery/quote/purchase/reveal/skill policies remain unchanged. | `UI Builder/HTML/Classic/popup-refresh.css`, `popup-refresh.js`, `classic.js`, `index.html`, generated `assets/popup-refresh/`; references/screenshots under `UI Builder/Reference Images/PopupRefresh/2026-10-19/` | Existing saved dialogs and shared backdrop/close in `Editor/Classic/BuildClassicDialogs.cs`, `Runtime/Classic/ClassicUI.cs`, `ClassicUI.Bindings.cs` and layout/motion owners. Reuse existing core/action ownership and adapt generated frame/icon assets in the separately requested native stage. | Awaiting HTML review |

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
