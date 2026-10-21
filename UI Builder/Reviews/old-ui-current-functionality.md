# Old UI with current functionality — HTML review brief

## User direction — 4 October 2026

The user rejected the new UI/UX and requested fitting the current functionality into the old UI screens, creating HTML first for preview. All screens must receive user approval before beginning Unity reconstruction. The earlier redesign approval waiver does not apply to this work.

This is a presentation revision, not a rollback of the current economy. Preserve the existing old HTML as reference. New candidates must remain distinct until approved. HTML uses explicit example states; it must not change Unity assets, source, saves, accounts, or purchases.

The current instruction establishes HTML-first review for this adaptation. Reuse the old artwork and screen compositions selected by the user; do not introduce another visual redesign or generate replacement artwork without a demonstrated need.

## Confirmed choices — 4 October 2026

1. Baseline: the user confirmed the original shared HTML (`HTML/Player/player.html?screen=Home`).
2. Navigation: the user requested retaining Player / Products / Legacy and adding a fourth **Shop** tab containing the IAPs.
3. Review sequence: the user requested the **entire connected prototype together**.

No new candidate is visually approved. The confirmed direction authorizes the HTML work; it does not approve its resulting appearance or Unity reconstruction. Candidate: `UI Builder/HTML/Classic/index.html`, revision 1.

## Proposed functionality placement

| Old surface | Current functionality to accommodate |
| --- | --- |
| Home and persistent painted stall | Eight product positions, first manual product purchase, normal/current income, three skills, next goal, current-run product appearances and persistent shop decorations; preserve compact composition. |
| Player panel | Tap training with x1/x10/x100/Max; access to skill and shop upgrades, statistics and skill details. |
| Products panel | Product levels and details; product upgrade catalogue with search, category/product scope, Available/Upcoming/Owned/Full filters. Final split of the 1,000-upgrade catalogue remains a layout decision. |
| Shared upgrade details and purchase confirmation | Before/after effects, prerequisites, exact-value expansion, pinned goals, Buy All review with frozen scope, stale quote and save-failure states. |
| Legacy panel | Current Fame and prestige summary plus Talent collection; replace obsolete permanent-perk purchase content. |
| Prestige sheet | Current reset/keep explanation, eligibility, Fame gain, credits and rebuild; confirmation and failed/stale outcomes. |
| Talent collection and detail sheets | 28 cards, ownership/levels, duplicate effects, odds, free credits and separate 100-KK draws, locked/empty/pending states. |
| Draw result | Saved new/duplicate result, skip/reduced motion, resume a pending result without re-awarding. |
| Shop tab | Four current KK packs and simulated-purchase presentation, pending/cancel/failure/recovered outcomes; a dedicated fourth tab, also reachable from Talent. |
| Offline sheet | Current credited earnings, cap/efficiency explanation and return action; no retired rewarded-ad offer. |
| Existing utility screens | Settings, sound/music/reduced motion, naming, statistics, privacy, Bootstrap/account choices and save recovery; retain their old presentation while reconciling current copy and routes. |

## Functional evidence inspected

- `Documentation/Implementation/Specification.md`: current economy scope, 1,000 upgrades, eight products, Fame, cards, KK, offline rules and retired rewarded income.
- `Assets/Game/Runtime/RedesignScreenRouter.cs`: current 13 gameplay routes, history, retained external routes and modal input guards.
- `Assets/Game/Runtime/RedesignHomeScreen.cs`: products, skill details, first purchase, goal, tap training, wallet and tap target.
- `Assets/Game/Runtime/RedesignTalentScreen.cs`: collection, card details, draw actions, shop, odds, prestige and pending result.
- `Assets/Game/Runtime/RedesignRetainedUi.cs` and `Documentation/Implementation/AccountFlow.md`: existing utilities and account/recovery access. Account availability must be reconciled with current configuration before writing final fixture copy; older implementation notes are not proof of live availability.
- `UI Builder/HTML/Player/ART-DIRECTION.md`: persistent stall, collapsible three-tab panels, retained scroll and nested sheets in the older HTML.

These are code/document findings, not a live Unity visual comparison or device verification.

## Review requirements

Keep controls and text editable and reuse local painted assets. Preserve screen history, list position, explicit disabled/locked states and clear cash/Fame/KK/credit distinctions. Gameplay gestures must not trigger from buttons, sheets or scrolling. Include current functionality metadata and identify any native presentation adaptations.

Inspect phone-size renders and actual browser interactions before presenting each candidate. Record the exact candidate revision and the user's response per screen. Approval of Home does not approve other screens, and completion of HTML does not authorize Unity reconstruction.
