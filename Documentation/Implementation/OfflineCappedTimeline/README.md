# Offline Capped Timeline — 25 October 2026

The user selected **2. Capped timeline** from the fill-meter variants and requested implementation in the game. The production owner is the saved `ClassicProductionCanvas` in `Assets/Scenes/Main.unity`. The current Classic HTML preview also matches the selected layout.

The reference is `UI Builder/Reference Images/Offline/FillMeterVariants/2026-10-25/02-capped-timeline.png`. The existing native coins, fonts, paper colours, frame and folk buttons are reused; this is editable uGUI, not a screenshot of the reference. The small geometric efficiency annulus is authored once in the Editor at `Assets/Game/ClassicUI/Offline/efficiency-ring.png` (128×128, neutral white with transparency).

## Layout and bindings

- Preserve the production 360×640 CanvasScaler and safe-area owner. The popup stretches horizontally inside 12-unit side margins and centres vertically. Its authored preferred height is 719 logical units, clamped to the usable viewport. The header reserves 72 units, fixed footer reserves 132, and the remaining body scrolls on shorter screens.
- The body has a coin/reward hero, stored-time timeline, efficiency ring, normal/ad comparison and status. The header has an always-available information button and X. Continue closes in place. The original complete popup remains saved inactive as `Local dialogs/Offline__LegacyDisabled_20261025`.
- `OfflineTimeline` maps the actual receipt: extent = max(away time, cap), gold = stored time / extent, cap marker = cap / extent, grey overflow = max(0, away − cap). Efficiency is independent of the time fill. New receipts freeze their cap; older receipts use the existing Long Opening Hours rule as a fallback.
- Amounts come from `OfflineReceipt.amount`, already settled by `ResolveOffline`. The normal reward remains paid when the popup is closed. Night Sales and Long Opening Hours retain their existing balance curves.

## Actions and motion

| Trigger | Owner and outcome | Verification |
| --- | --- | --- |
| Return to Main with pending receipt | Existing `ClassicApplication` opens the saved Offline view | Production-route Play Mode test with disposable profile |
| Open popup | Existing `ClassicMotion` popup entry/backdrop; stored time and efficiency settle through its 0.4-second fill animation | Opening/settled captures and target-fill checks |
| Header i | Existing Guidance reader with explicit return-play topic; works after the topic has been read | Open, dismiss, return to Offline |
| Continue / X | Persist `DismissOffline`, invalidate any ad attempt, close without switching tabs | Continue, X and late-callback checks |
| Watch ad | Existing `IRewardedAds` interface; disable repeats while waiting | Explicit fake provider in Editor validation only |
| Completed ad | Persist one extra base amount and `adRewarded` atomically for the same pending receipt | Core save-failure/reload/replay tests; native callback test |
| Cancel/fail/unavailable/late result | Retain normal reward; no bonus | Core and native tests |
| Reduced motion | Existing motion owner uses settled fills and authored pose | Native reduced-motion assertion |

**No live advertising SDK or ad unit is configured.** Production uses `DisabledRewardedAds`: Watch is disabled and the popup explains that normal earnings are already saved. `SetOfflineAdProvider` is the integration point for a configured provider that reports completion on Unity's main thread. The browser's Interaction state selector simulates outcomes only. No fake provider is attached to the shipped scene, no ad SDK was installed, and real ad-network/device delivery was not tested.

## Evidence

- `Core-checks.txt`: all **839** RedesignLab checks pass, including 12 new timeline/reward checks.
- `Validation.txt`: **30** native checks using the real production Main route and a disposable seeded profile. Includes saved component count, no runtime UI construction, 390×844 / 360×640 / 360×540, simulated notch insets, card-improved cap/efficiency, ad cancellation/completion/duplicates/stale callback, durable reward, info and Continue.
- `02-capped-timeline-390x844.png`: native settled screen. The smaller native reward than the browser study is correct for the test profile's actual income; no reference amount is hardcoded into gameplay.
- `03-ad-bonus-collected.png`: explicit fake-provider completion evidence.
- `04-capped-360x540.png`: fixed footer and scrollable short-screen body.
- `05-under-cap.png`, `06-card-improved-cap.png`, `07-notched-safe-area.png`: additional actual Unity states.
- `browser-preview.png`: current HTML preview; ad watching, completion and Continue also checked through the browser controls.
- `HTML-inventory-summary.json`: live Classic package inventory passes, with no missing resources, missing Unity assets or duplicate IDs. The scoped inventory excludes preserved historical `parity/before` snapshots and unselected review studies. Historical snapshots remain untouched.
- `Saved-scene-audit.txt`: saved Main reload verifies all 16 view references, inactive entry/legacy roots and the correct authored Edit Mode height. `Scope-checks.json` records byte parity for the protected font files.

Shared font assets were byte-checked against the pre-task backup and are unchanged. Temporary authoring backups and the live HTML inventory mirror remain under the task's Windows Temp folder because automatic approval review blocked their cleanup. No build settings, real player saves, authentication, Firebase setup, balance defaults or unrelated UI requests were intentionally changed. Unity-authored scene YAML retains Unity's standard whitespace formatting.
