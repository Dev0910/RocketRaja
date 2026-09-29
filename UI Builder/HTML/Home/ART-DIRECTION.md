# Home — HTML art direction and review

**Gouache Storybook HTML pass complete; ready for visual review.** The user selected scene-only option 05 on 2026-09-17 for all game UI and artwork. This Home preview now uses matching painted stall, product displays, shopkeeper and icon layers, with parchment surfaces and matte teal controls. Follow [the game art direction](../../ART-DIRECTION.md). Unity visuals have not been changed.

Latest resolution correction: portrait reference is 1080 pixels wide × 2200 pixels high. Desktop preview fits this aspect ratio; smaller phone checks remain responsive. Earlier 1080×1920 captures are superseded by the 1080×2200 captures.

Open [home.html](home.html) in a browser. Add `?review=1` to show the review state selector and action log; use `?state=new` or `?state=progressed` for clean states. Everything loads from this folder without a server or CDN. The optional local review server is merely convenient.

## Reference and scope

Selected style: ../../Reference Images/Home/Style Explorations/2026-09-17-scene-only/05-gouache-storybook.png.

Original layout input: C:/Users/Dev/AppData/Local/Temp/codex-clipboard-cd137a29-f8e8-4966-aa1e-c9ec06da70e6.png. Project reference: ../../Reference Images/Home/01-bold-cartoon-v2-no-rank.png. Earlier grayscale layouts: ../../Wireframes/Home/.

Latest user correction takes precedence: **no shop rank heading or rank progress bar**. Keep the separate product/display milestone.

This deliverable recreates the visual direction using independently generated empty shop, product art and character, plus live HTML typography, separately clipped painted icons, panels and semantic buttons. It is an editable interpretation, not a flattened reference screenshot. Layered sources and asset prompts are documented in [assets/GOUACHE-PROVENANCE.md](assets/GOUACHE-PROVENANCE.md).

Only Home is designed. Player, Products, Legacy, Settings and the video confirmation have verified Unity routes but their new layouts are later steps. Clicking a route shows an explicitly marked HTML review notice; that notice is not a proposed game popup. No account signup, collectible cards or fourth tab.

## Design / Unity / HTML reconciliation

| Design intent | Existing Unity evidence | HTML and later handoff |
| --- | --- | --- |
| Rank-free cash header | GameView.BuildHeader / RefreshNow still use a named stall-stage label and progress bar | Removed visually here. Remove those view nodes later; retain StallTier mechanics. |
| Persistent upper shop, three lower-panel routes | GameView.BuildNavigation / Switch caches and switches panels | Home upper scene is persistent. Browser logs route requests; other panels are not implemented. |
| Home without title or scrolling | GameView.NewPanel adds header; BuildHome creates Scroll | Fixed responsive zones in HTML. Native view layout needs adaptation. |
| Eight independent product slots and names | GameEngine.VisualTier / State.Level; active GameContent.asset IDs; GameView.RefreshNow | Eight keyed art nodes. New fixture hides art. Runtime label visibility differs and needs visual adaptation. |
| Four store stages / four tiers per product | GameEngine.StallTier / VisualTier | Existing mechanics preserved. One decorated shell and one art tier are provided for this style study; full production art remains separate. |
| Gift-box display name | Active content uses Celebration boxes and ID gift-boxes | Reference-facing label Celebration gift boxes; keep stable ID. |
| Income, tap value, normal income and run earnings | GameEngine.LiveIncome / TapIncome / NormalIncome / State.runEarnings | Live DOM bindings with illustrative fixtures. Separate boosted/normal labels need view adaptation; native formulas remain authoritative. |
| Next milestone / temporary feedback in same strip | GameView.NextGoal exists; Notice uses a separate toast | Milestone strip is fixed in HTML; native purchase-feedback routing and expiry need adaptation. No purchase control exists on Home. |
| Stall-only tap | ShopTapTarget handles pointer threshold; GameController.Tap guards input | In-memory tap illustration. Drag, cancellation and outside release rejected. Native pointer threshold remains owned by ShopTapTarget. |
| Three skill states | BuildHome → GameEngine.ActivateSkill; SkillRank and skill timers | Ready, active, cooldown and locked fixtures. Clicking ready changes its display state and records a request; no skill income/payout is simulated. |
| Optional sponsor | BuildHome.Confirm → GameController.ShowAd → GameEngine.RewardAd | Active and unavailable fixtures. No ad loading or callback/reward emulation. |
| Settings / modal blocking | GameView.ShowSettings / Modal / CloseModal; GameController.ModalPaused | Entry mapped. Preview notice blocks background and supports Escape; actual settings form is excluded. |
| Local persistence and reconciliation | GameController.Save, pause/resume and ad lifecycle; GameEngine reward/offline ownership | Metadata documents existing owners. HTML never reads or writes saves or invokes Unity. |
| Safe areas / reduced motion | Native layout and saved reduced-motion setting | CSS env safe insets and browser reduced-motion support; real Android cutouts and touch ergonomics still require native validation. |

All runtime classifications and exact action/binding mappings are in the **single nonexecuting unity-metadata JSON block inside home.html**. There is no standalone behavior JSON or native geometry manifest. Source code was inspected read-only; Unity MCP reported an idle editor. No gameplay runtime test or Unity import was performed.

## Review fixtures

- **Progressed:** ₹1.24M cash, ₹12.4K live sales, ₹6.2K normal sales, ₹250/tap, ₹3.8M run earnings, eight populated displays. These values and milestone come from the visual reference, not an observed Unity save.
- **New:** ₹0, ₹1/tap, no automatic income or products, three locked skills. First Phuljhari cost ₹120 was checked against the active GameContent.asset.
- **All skills ready:** demonstration of enabled controls without sponsor.
- **Cooldowns:** demonstration of three disabled skills.
- **Video unavailable:** sponsor attempt shows a review notice and grants nothing.

Waiting skills show a translucent grey radial cover over the illustration inside the gold rim. It starts at 360 degrees and clears clockwise from twelve o'clock as the remaining cooldown fraction falls. The WAIT timer stays readable below; ready, active and locked states have no radial cover. Native handoff must use the effective activation cooldown, including Legacy reductions, without changing gameplay timers.

Timers and radial fractions are frozen to make screenshot comparisons reproducible. Skill clicks record requests and demonstrate control state only, so fixture income does not change with a skill click. Reset the fixture before taking reference captures. There is no passive simulation, offline payout, browser storage, network advertisement or real reward in the HTML.

Both main states use the same decorated background to evaluate the selected style. Empty slots are fully independent, but the new fixture is not the final basic-stall production illustration.

## Presentation and interaction

- Reference coordinates: 1080 × 2200. Logical base width: 360; width scales layout tokens while shorter screens compress the art scene.
- Colors: warm cream panels, deep blue ink, maroon/gold shop and teal sponsor/navigation. Local Outfit font for interface text, a live Georgia serif shop sign, and weight changes for hierarchy. Subtle original SVG grain connects the panels to the painted art.
- Settings, skills, sponsor and tabs use targets at least 48 logical pixels. Shelf labels are noninteractive.
- Text, labels and values stay editable. Product artwork is clipped using SVG viewBoxes and a rocket isolation path in preview.js; icon tiles use an explicit shared SVG clip path in home.html; those crops are presentation details, not Unity metadata.
- New/progressed states share one DOM/layout. Review tools are hidden in clean captures.
- Pointer income comes only from the stall button's short accepted gesture; all controls lie outside or above that surface.
- Selecting tabs in Unity must preserve header/stall and each panel's scroll; selecting the current tab or collapse returns Home. HTML only records those destinations for this stage.
- All decorative illustrations ignore pointer events. Keyboard focus is visible; the review notice traps focus to its close control and Escape returns to Home.
- Browser animation respects prefers-reduced-motion. Native music, sound and motion remain owned by existing settings.

## Verification

Run `node verify-preview.cjs` to repeat browser verification. The script uses the local bundled Playwright and installed Edge; set PLAYWRIGHT_MODULE / BROWSER_EXE if using another workstation.

Clean captures in renders/:
- New and progressed at **1080 × 2200**, **360 × 640**, **390 × 844**, **360 × 540**.
- Ready, cooldown and unavailable-video states at **360 × 640**.

All 11 captures generated; 11 interaction checks passed; zero browser script/console errors. Layout checks verified control visibility, minimum target size, no Home scrolling, eight products, three skills and three tabs. Metadata checks verified one JSON block, unique DOM IDs and existing action/binding targets. Interactions checked single tap, drag rejection, button isolation, modal blocking, locked/repeated skills, active sponsor disabling, no reward on video failure, preview Bulk Order boundary and reduced motion.

Reference renders were visually inspected at full reference size and shorter/taller phone layouts. Small screens keep controls readable by reducing scene height. Check this visual tradeoff during your review. [Machine-readable verification result](renders/verification.json) is a QA report, not an alternative behavior contract.

## Next stage

Review this HTML's composition and styling first. Reference refinements, other screens and native Unity reconstruction remain separate directed steps. This package is not Unity-integrated or release-ready.

## Current style — Ivory Bazaar rollout

The user selected A — Ivory Bazaar, accepted the Player implementation, then explicitly requested a detailed review and rollout across every screen. This supersedes prior per-screen color, serif-heading and card-finish guidance. Original wireframes remain behavior/structure history; no new per-screen generation is claimed.

Shared presentation: ../Shared/ivory-bazaar.css; approved Player specifics: ../Shared/player-folk.css; editable frames and ornaments: ../Shared/ivory/; buttons: ../Shared/folk-buttons.css and ../Shared/folk/. Selected image: ../../Reference Images/Player/Folk Variations/A-ivory-bazaar.png.

This screen was visually reviewed at phone sizes, including long-content and applicable disabled/unavailable states. See ../../Reviews/ivory-bazaar-rollout-review.md for screen-specific findings, fixes and verification evidence. Current captures are in renders/. HTML remains a preview; no Unity runtime files or gameplay rules were changed.
