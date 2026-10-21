# Achievement reward ledger — 21 October 2026

The user selected **Option 1 — Reward ledger** with: “I like option 1, the reward ledger. Implement this design for the achievements page.” This updates the existing Achievements popup. The selected reference is `UI Builder/Reference Images/Achievements/Popup Variants/2026-10-21/01-reward-ledger.png`.

The selected reference is approved. This session directly implements that requested design in editable HTML and saved native uGUI; it does not claim a subsequent user review of the resulting HTML or Unity pixels.

## Implementation

| Element | Selected intent | HTML | Verified native owner |
| --- | --- | --- | --- |
| Header | Serif title and painted trophy | Shared `Classic/achievements-ledger.js` and CSS | `ClassicAchievementLedgerAuthoring` |
| Overview | Claimed count, ready count and Kaju Katli balance | Live fixture summary | `ClassicUI.RefreshAchievements` and serialized TMP references |
| Achievement rows | Illustrated ledger, progress, reward column, Claim action | 15 stable ID rows from the current achievement definitions | 15 Editor-authored `ClassicAchievementRow` bindings |
| States | READY, in progress, claimed | Disabled action and independent status styling | `CanClaimAchievement` plus existing native data binding |
| Rewards | One saved payout per achievement | Memory-only interaction demonstration | Existing `RedesignEngine.ClaimAchievement` / atomic `Commit` / save store |
| Scrolling | Hero and overview can scroll away on short phones | Bounded scrolling body | Saved ScrollRect; fixed close and footer |
| Motion | Existing game feedback | Existing Classic preview motion | Existing `ClassicMotion` enter/exit, claim feedback and reduced-motion behavior |

The screenshot's sample values are not new balance data. All fifteen definitions, thresholds, reward amounts and persistence rules remain owned by the existing core. The explicit `ledger-reference` browser fixture shows 1/15 claimed, four ready rewards and 410 KK. Other fixtures use the existing core-exported values. Browser changes are disposable memory only; there is no browser persistence, network service or payment integration.

## Artwork

The trophy, Kaju Katli sweets, stall, product assortments, diya, fireworks, earnings pouch and upgrade artwork were generated as one transparent atlas with built-in ChatGPT image generation, using the selected Reward ledger image as reference. See [ASSET-PROMPT.md](ASSET-PROMPT.md). Original generated output is retained in the Codex generated-images directory.

The 1254×1254 atlas has real alpha. Browser copies are at `assets/achievements/ledger-atlas.png` and `../Classic/assets/achievements/ledger-atlas.png`; the native copy is `Assets/Game/ClassicUI/Achievements/ledger-atlas.png`. All three have SHA-256 `6969C52798FFF2E7EEE62BF6D74D9A4CA80990A9F50139592D864ABC2DF57A7C`. Native Sprite rectangles use original source dimensions, preserving correct crops independently of imported NPOT sizes. Art is isolated from all live labels, controls and progress bars.

## Delivery and validation

The READY/Claim follow-up uses four small geometric surfaces authored by `LedgerControlSurface` in the Editor: `ready-badge.png` (notched gold plaque and ornaments), `claim-ready.png` (rounded cobalt and gold), `claim-progress.png` (pale blue) and `claim-claimed.png` (muted tan). These are independent of the painted illustration atlas and all text stays live. Their HTML and native PNG copies are byte-identical, declared in the package metadata and checked by the inspector. Explicit ledger CSS overrides the shared folk button frame, square corners and disabled pseudo-element. Native binding selects the matching serialized state sprite; Claim retains its 48px target.

`index.html` is the focused editable entry; it reuses the same ledger component and styles as the current Classic preview's Achievements route. The focused package passes the project inspector, including resource and native atlas parity checks. The historical `Classic/parity/before` snapshot is preserved unchanged; its obsolete relative links are outside this focused package.

Native replacement is saved in `Assets/Scenes/Main.unity` under the existing `ClassicProductionCanvas`. The original popup remains complete and inactive as `Achievements__Legacy_20261021`; the existing Player shortcut and dialog route open the new page. No UI is constructed in Play Mode.

Browser and Unity checks cover 360×540, 360×640, 390×844 and 450×916. Native checks also cover simulated notch insets, actual Player navigation, claim/save/reload/duplicate guards, unchanged tap/cash behavior, stable preplaced UI identities, close/reopen and interrupted enter/exit feedback. Actual saved Main was exercised with `ClassicApplication` and a separate temporary guest profile. No real player save, account or live service was used.

Evidence and screenshots: `Documentation/AchievementValidation/RewardLedger/`. All 805 core checks pass. Bootstrap was restored and the disposable QA scene removed after verification. An Android binary/device test was not part of this change.
