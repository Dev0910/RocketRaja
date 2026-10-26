# Current CTA red-dot states — 25 October 2026

Requested: show the red-dot state of all CTAs. A browser review gallery now presents actual production Unity captures, including on/off appearances, category filters, and full-screen views.

Open `http://127.0.0.1:4310/Classic/reviews/red-dot-states-20261025/`.

- 83 saved bindings inspected through `ClassicRedDots` on `ClassicProductionCanvas`.
- 27 distinct visible CTA appearances captured across 14 screen/scroll positions. Repeated upgrade and achievement rows have one representative per view.
- ON images use the current readiness predicates in `Assets/Game/Runtime/Classic/ClassicRedDots.cs`. OFF images hide only the saved indicators for an appearance comparison; they do not claim a different economy condition.
- Coverage: Player/Legacy navigation and collapse arrows, Skills/Achievements/prestige entry buttons, all three Home skill CTAs, skill-list Activate and Details, each skill-detail Activate, Buy All, individual upgrades in Player/catalogue, reviewed-batch confirmation, 1×/5× draws, prestige confirmation and achievement claims.
- The gallery lists CTAs without current markers: Products/Shop navigation, stocking/levels, tap training, quantity selectors, Stats, Settings, info, ordinary Continue/Close and offline ads.

The capture uses an isolated seeded guest profile under Windows Temp. No real user save, production scene, runtime script, balance or notification rule was changed. Original Bootstrap/Edit Mode context was restored afterwards. The main Classic HTML preview still has no red-dot implementation; this is an inspection gallery of the existing Unity indicators.

Source owners: `ClassicRedDots.cs`, `ClassicRedDotAuthoring.cs`, `ClassicUI.Notifications.cs`. The local review folder contains the native screenshots, exact capture inventory (`states.json`) and capture status. The shared scene's notification rules are unchanged.

## Revision — 26 October 2026

- Removed the 13 saved indicators from Home skills, Player's Skills entry, skill-list Activate/Details controls, skill-detail Activate, and Player/Legacy collapse buttons.
- Skill readiness no longer contributes to the Player navigation indicator. Rewards, upgrades and prestige still contribute.
- Moved 53 blue-button indicators inward by 5 UI pixels on both axes: top-right offsets now (-7, -7), previously (-2, -2).
- Updated the production Main scene and shared authoring/runtime rules so re-authoring and refresh preserve these choices.
- Unity validation passed: 70 retained bindings, excluded controls remain unmarked, blue-button insets, reward claims, affordable upgrades, paid draws, prestige and blocked sessions. Validation used an in-memory engine without writing a player profile.
- The 25 October screenshot gallery above is historical and shows the previous placements.
