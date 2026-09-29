# Unity game versus HTML — UI coverage audit

Implementation follow-up: the custom state gaps below are now covered in [UI-COVERAGE-COMPLETION.md](UI-COVERAGE-COMPLETION.md). This audit preserves the original findings; use the completion report and HTML state gallery for current status.

## Result

**No whole custom Unity panel or dialog is missing from the HTML. Several feedback and progression states are missing or incomplete.**

Unity currently builds four primary views (Home, Player, Products, Legacy) and seven custom dialog families (Statistics, Prestige, Offline, Sponsor, Settings, Privacy fallback, Save recovery). HTML has all eleven of these, plus Skills and Product Detail, for **13 destinations**. Locked and ready prestige are variants of one family, not forgotten separate screens.

Scope: inspected the current `Assets/Game/Runtime/GameView.cs`, `GameController.cs`, `RewardedAds.cs`, `Core/SaveStore.cs`, HTML generator, shared preview logic, and rendered HTML. Opened all 13 HTML destinations in Edge at 450×916; no browser exceptions. Exercised Home new/progressed/active/premium states and tap feedback. Saved evidence in [ui-coverage-audit/browser-evidence.json](ui-coverage-audit/browser-evidence.json), with Home screenshots beside it.

**Limit:** Unity MCP at localhost:28252 refused connection. Unity observations below are source-verified, not a claim that a running build was visually inspected. No Unity assets, saves, HTML implementation, or game behavior were changed during this audit.

## Screen-by-screen comparison

| UI | Unity implementation | HTML coverage | Assessment |
|---|---|---|---|
| Home / persistent shop | Wallet, settings, eight shelves, shopkeeper, income, per-tap income, milestones, onboarding, three skills, sponsor, navigation | All principal components exist in the shared Home route | **Partial states:** sponsor-active presentation, milestone progression, feedback and scene tiers need work; see below |
| Player | Tap upgrade, prestige entry, purchased count, 96 authored unlockables, buy-all, Statistics, lifetime summary | Tap upgrade with quantity menu, prestige entry, Stats/Skills icons, buy-all, next ten unpurchased unlockables | **Present.** Showing only ten and moving statistics into a popup are intended changes |
| Products | Eight families, levels, rates, next-display text, quantity choices, purchase quotes, locked/unaffordable states | Eight framed products, progress bars, top-right levels, Profit, expandable quantity menu, original blue upgrade buttons | **Present.** Numerical fixtures exist; multiple artwork tiers do not |
| Legacy | Reputation balance, 12 perks, rank/cost/benefit, affordability and mastery | All 12 perks and corresponding fixture states | **Present** |
| Statistics | Generic confirmation panel showing current-run and lifetime statistics | Dedicated Statistics modal containing the same six fields | **Present; redesigned** |
| Prestige — locked | Eight-family and eligible-income requirements, current eligible income, ad-bonus exclusion | Locked requirements and disabled confirmation | **Present** |
| Prestige — ready | Reputation reward, reset/keep summary, confirm/cancel | Reward, reset/keep, paused-shop wording, confirmation | **Present** |
| Prestige — stale | Notice when the captured reward no longer matches | Dedicated stale-quote message fixture | **Present.** HTML deliberately keeps the refusal within the review modal |
| Offline earnings | Already-credited amount, optional doubling, continue without video | Same core content, additional cap/rate explanation, unavailable/cancelled/failed/consumed variants | **Present.** Actual resume-triggering and reward processing remain native |
| Festival Sponsor | Optional-video confirmation, ×2 for five minutes, reputation exclusion | Dedicated Sponsor modal, active/unavailable/cancelled/failed variants | **Present.** Home's active banner is incomplete even though the modal variant exists |
| Settings | Music, sound, reduced motion, privacy, conditional ad privacy choices | All controls represented; toggles are preview-only | **Present** |
| Privacy information | Configured external privacy URL; development fallback otherwise | Development fallback and conditional privacy-choices entry | **Present for the custom fallback.** External production policy is not authored by this HTML |
| Save recovery | Blocking corrupted-save confirmation, start fresh or keep files and exit | Blocking SaveRecovery modal, matching choices, no dismiss-to-play | **Present.** This does not cover successful backup recovery or later save failures |
| Skills guide | No dedicated Unity dialog in current `GameView` | Three-skill read-only guide | **HTML addition, not missing HTML** |
| Product Detail | No dedicated Unity dialog in current `GameView` | Per-product modal with level, income and display thresholds | **HTML addition, not missing HTML** |

Native route evidence: `GameView.cs:134` Home; `:185` Player; `:233` Products; `:268` Legacy; `:198` Statistics; `:377` Prestige; `:383` Offline; `:391` Settings; `:400` Privacy; `:409` Recovery. Sponsor originates at `:163`. HTML structure: `build-screens.cjs:65–107`; state handling: `HTML/Shared/preview.js`.

## Missing or incomplete UI states

### 1. Save warning / recovered-backup notices — high priority

**Game:** `GameController.Start` shows `SaveWarning`; `Save` displays a storage-failure notice. `SaveStore.Load` can recover a backup and display “Your last backup was recovered.” These are nonblocking notices, distinct from the blocking unrecoverable-save dialog.

**HTML:** save recovery exists, and the metadata mentions persistence failures, but no normal-player fixture presents either the save-failure warning or recovered-backup notice. Metadata text alone is not a rendered UI state. The existing toast is used for review-only request messages.

**Needed:** two explicit notice previews using the shared notice component. A new full page is unnecessary. Any future fixture must avoid implying a real save succeeded or failed.

Evidence: `GameController.cs:43–47,65–68`; `SaveStore.cs:35–36`; `preview.js:16,60–62`.

### 2. Home sponsor-active banner — high priority

**Game:** Home replaces the offer with “Sponsor active” plus remaining time and disables the sponsor button while the boost is active (`GameView.cs:170–174`).

**HTML:** the main shared Home button always says “Festival Sponsor / Optional video · ×2 sales for 5 minutes” and remains enabled, including `state=active`. The income note simultaneously says Sponsor ×2. The Sponsor modal does have an active message and disables its Watch button, so this is a missing Home state, not a missing modal.

**Needed:** active countdown, expired/available label, and matching interactability on the shared Home button. The older standalone `Home/home.html` study already has a sponsor-active banner; it is not the Home route used by the gallery or the user's current URL.

Browser evidence: `homeStates.active` and `homeStates.progressed` in the audit JSON.

### 3. Home milestone / onboarding progression — medium priority

**Game:** `NextGoal()` selects the next unowned family, then the nearest display upgrade, then a prestige prompt after all display milestones. Home also has different guidance for the first shelf, first run, and post-prestige play.

**HTML:** shared Home has a first-Phuljhari state and a fixed “Phuljhari · 12 levels to upgrade” state. In the premium fixture every product is level 250, yet Home still shows that same 12-level milestone. There is no representative partial-shop next-family fixture or all-displays-complete prestige milestone. First-time guidance is partly represented by the milestone/tap hint, but not the full native guidance sequence.

**Needed:** representative next-family, next-display, all-displays-complete/prestige, and post-prestige guidance states within Home. Do not restore the removed shop-rank header.

Evidence: `GameView.cs:171–183`; `preview.js:17,35`; browser `homeStates.premium`.

### 4. Gameplay feedback layer — medium priority

**Game:** pooled floating +₹ tap feedback; purchase success; buy-all result count; perk improvement; display-upgrade notice; skill activation; prestige follow-up; rewarded-video success. `Notice` shows a short-lived message; `Celebrate` pulses the shop and triggers a burst.

**HTML:** in the main shared route, tapping logs an action and briefly highlights the invisible tap target; no +₹ feedback appears. Purchase/skill actions show generic request text only in review mode, not their intended player-facing feedback. Ad failure/cancellation and stale prestige messages do exist, but reward-success and purchase-success examples do not. The newly added panel/popup animations do not replace these event-feedback states.

**Needed:** a reusable in-game notice family, illustrative tap feedback, and representative success/celebration previews. These can be frozen visual demonstrations; they must not implement a competing economy or fake real ad callbacks.

The standalone Home study has tap text; that does not establish coverage in `Player/player.html?screen=Home`. Browser tap evidence records `TapStall`, no feedback node and no visible toast.

Evidence: `GameView.cs:152,191,199,214,253,282,356–375,381`; `GameController.cs:111–112`; `preview.js:59–65`.

### 5. Shop/display artwork progression — medium priority

**Game:** `RefreshNow` selects stall appearance from shop tier and product shelf sprites from `VisualTier`; display thresholds are 1, 25, 100 and 250. Shopkeeper idle movement, visitors and celebration effects are also implemented.

**HTML:** uses one Gouache stall image and one crop per product in every fixture. Ownership can hide products and numerical levels/progress change, but the premium fixture uses exactly the same product artwork and stall as progressed. New-shop styling therefore does not demonstrate all game progression appearances. Idle visitors and shopkeeper motion are not represented by the panel animation system.

**Needed:** explicitly reviewed artwork/state variants for the selected art direction, plus an explicit decision on ambient character/visitor motion. These are scene-state and art coverage gaps, not missing menus. Idle decoration is lower priority than save/sponsor/progression clarity.

Evidence: `GameView.cs:316–327,337–353`; `build-screens.cjs:56,65`; matching `stall` and `productArt` values across browser fixtures.

## External UI: absent by design, not forgotten custom screens

| Surface | Owner / trigger | HTML treatment |
|---|---|---|
| First-run advertising consent | Google UMP, `RewardedAds.Initialize`, `ConsentForm.LoadAndShowConsentFormIfRequired` | No branded custom replacement needed; mark as provider-owned in flow documentation |
| Advertising privacy form | Google UMP, `PrivacyOptions`, conditional Settings/Privacy entry | Entry control exists; form is external |
| Full-screen rewarded video | AdMob, `RewardedAds.Show` | Confirmation exists; video playback remains provider-owned |
| Production privacy policy | Configured URL opened by `Application.OpenURL` | Development fallback exists; policy content is separate |

The native UI source does not define an account/login screen, cloud recovery, premium-currency store, daily-reward screen, or separate loading/title menu. Those should not be added as “missing” solely because other games have them.

## Intentional differences to preserve

- Ivory Bazaar surfaces, folk-print blue buttons, and three compact icon-only navigation tickets are the selected redesign.
- An active navigation ticket and icon grow slightly; the upper scene remains steady.
- Shop-rank header/bar, heading flowers, blue sprigs and the scroll-hint strip were explicitly removed.
- Player uses Stats/Skills icons, a compact quantity selector, and only the next ten unpurchased unlockables.
- Products uses Profit, omits the normal-sales note, uses an expandable quantity selector, framed art/progress, top-right levels and no visible “View details” label or auto-collect bar.
- Player's native lifetime summary is represented in Statistics; duplication at the bottom of Player is not required.
- Skills and Product Detail are new HTML presentations requiring later native implementation.
- HTML fixture numbers, selections and animations are not proof of native economy, persistence or ad integration.

## Documentation drift

`screen-specs.json` still describes all 96 visible unlockables and normal-versus-boosted Products totals. Those descriptions predate the user's next-ten and Profit changes. Current DOM and latest user instructions are the authority for this audit. Update those descriptions when maintaining handoff documentation so old requirements are not accidentally restored.

## Recommended completion order

1. Preview save-failure and backup-recovery notices.
2. Correct Home sponsor-active/expired presentation and milestone variants.
3. Add representative tap, purchase, skill, reward and prestige feedback states.
4. Review and author missing stall/product appearance tiers; decide on optional ambient animation.
5. When Unity is available, perform a live Game View comparison and verify provider-owned transitions on an appropriate build/device.

No additional full-screen HTML destination is currently justified by the native UI inventory. The remaining work is mainly state coverage in existing views and shared feedback components.
