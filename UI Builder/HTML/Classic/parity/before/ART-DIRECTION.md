# Classic UI / current functionality — candidate 1

## Current authoring and UI review — 16 October 2026

The current `index.html`, `classic.css`, `classic.js`, `motion.css`, and `motion.js` are the editable Classic presentation source. Preserve the existing exported `content.js` and `fixtures.js` when polishing the interface. The historical generation steps below describe earlier revisions: **do not run `Tools/ClassicUiPreview/build.cjs` to rebuild this version**. It reads the removed `HTML/Player/player.html` and would replace later HTML changes. No deleted source page needs to be restored.

For this UI goal, the user explicitly delegated wireframe and art-reference confirmation to three Audit/Fix/Review agents, all `gpt-6.1-sol` at high effort. This delegation is not final user visual approval. The HTML remains a candidate; Unity, backend, game rules, content, and economy are outside this presentation revision.

The persistent screen, component, state, finding, and verification ledger is [Classic goal coverage](../../Reviews/ClassicGoal/COVERAGE.md), with screenshots and machine-readable evidence beside it. Existing product/card/icon artwork is retained unless that ledger records a specific replacement.

This revision compacts the painted shop only while a tab is expanded, keeps artwork and live signage in the same width-based coordinate system, adds overflow cues and phone-local feedback, and improves popup dismissal, validation, focus and navigation states. Contextual upgrade lists match their quoted scope; the first-stock prompt leads to the existing Products purchase; stale quotes explain their guard before the list. All shelf hit areas are at least 48 logical pixels without overlapping each other or the stall tap area. Separate shelf visual layers preserve the original artwork coordinates.

All existing raster artwork and exported content/fixture files are retained. The R5 header correction has zero decoded Home pixel changes at all four target sizes. Browser results and screenshots establish this editable HTML candidate only; final user acceptance and any native Unity reconstruction remain separate.

## Home revision — 6 October 2026

At the user's request, removed the Home-only “Skill guide / While away / Shop stats” links and anchored the three skill icons, names and status labels at the bottom of the Home panel, directly above the four navigation tabs. Existing destination screens remain accessible through their other routes and review controls. HTML revision only; visual approval and Unity reconstruction remain pending.

Follow-up: moved the automatic-sales/per-tap panel directly above the bottom skill row, keeping both together above navigation. The next-display panel remains below the stall.

## Player revision — 6 October 2026

Latest product-tile placement correction: product name and income now sit to the right of the framed image; **Lvl : xx** sits immediately left of the upgrade button, vertically centered with it. This supersedes the earlier name-row right-corner placement. Checked all eight tiles at 390×844 and 360×540 for alignment and overflow.

Products follow-up: removed the panel's **Browse product upgrades** button at the user's request. The quantity control and eight product rows remain; product-detail upgrade links are unchanged.

Product tile follow-up: removed “Details & next milestone”, framed each product image with a bottom fill bar, and moved the level to the name row's right corner as “Lvl : xx”. Progress is normalized between the previous and next level milestone: `(level − previous) / (next − previous)`, using 0/25/50/100 and then every 100 levels. For example, level 18 is 72% from 0 to 25; reaching 25 begins the next interval at 0%. The image/income area still opens details. Checked eight frames, label placement, normalized fill and detail navigation at 390×844 and 360×540.

The Player toolbar quantity button now cycles ×1 / ×10 / ×100 / Max, like Products, without opening Salesmanship. The Upgrades section has one direct **Buy all Upgrades** action, a current-run purchased/total count (for example 4/1000), and the next ten unowned upgrades whose prerequisites are met, ordered by price. Available does not imply affordable; unaffordable rows retain their disabled purchase controls.

The direct bulk action demonstrates the exported core All-scope batch, including rows beyond the ten visible upgrades. It updates preview ownership/count/list without opening the catalogue or a confirmation sheet and disables once that frozen batch is exhausted. This is still a disposable frozen-data preview, not a live quote engine; native implementation must obtain and commit a fresh full-scope quote atomically. The existing catalogue review flow remains available elsewhere. Focused browser checks passed at 390×844 and 360×540 for quantity cycling without a popup, ten rows, count, and direct full-scope purchase.

4 October 2026. Editable entry: [index.html](index.html).

The user confirmed the old shared HTML as the visual baseline, requested **Player / Products / Legacy / Shop**, and requested reviewing the complete connected prototype together. This later explicit HTML-first instruction governs this adaptation. Every screen remains **candidate**; there is no visual approval or authorization to bring it into Unity.

## What is reused

The layered original stall, independent products and shopkeeper, painted Ivory Bazaar icon atlas, original cream/blue/raspberry surfaces, ticket buttons, fonts and collapsible lower panels are reused by relative reference. Sources are `../Player/player.html`, `../Home/assets/`, and the existing `../Shared/` styles/icons. Their recorded generation and font provenance remain in the original packages. No new raster artwork was generated, and no complete-screen image is used behind controls.

`classic.css` contains scoped responsive/layout additions. `classic.js` contains the connected disposable HTML interactions. The original HTML and Unity files are unchanged by this task.

## Placement and coverage

| Surface | Current functionality represented | Status |
| --- | --- | --- |
| Home | Cash, normal sales and tap payout, eight product details, first stocking, three skill details, pinned goal, offline summary | Candidate |
| Player | Salesmanship quantities, statistics, skill guide, prestige shortcut, complete catalogue entry and category shortcuts | Candidate |
| Products | All eight products, quantities, availability, purchase outcomes, product detail and scoped upgrades | Candidate |
| Legacy | Fame multiplier, prestige, 28-card Talent collection, draw credits/KK and appearance/progression explanation | Candidate |
| Shop | Four authored IAP packs, example prices, purchase review, pending/retry/cancel/failure/delivery and distinct repeat transactions | Candidate |
| Catalogue and detail | All 1,000 authored IDs, category/product/skill/search filters, Available/Upcoming/Owned/Full, paging, retained search/scroll, before/after projections, prerequisite routes, exact prices, pinning | Candidate |
| Buy All | Frozen review with budget/IDs/prices, first purchases, cancel/confirm, stale and refreshed review, save blocking | Candidate |
| Talent | All cards, tiers and exact odds, current/next bonuses, free/100-KK actions, locked/zero-balance states, new/duplicate reveal and resume/skip | Candidate |
| Prestige | Eligible/below-target, Fame/draw rewards, reset/keep, next target, core-derived rebuilt example | Candidate |
| Skills | Current core power/duration/cooldown, normal-sales references, base-unlock routes, ready/active/cooldown states | Candidate |
| Utilities | Statistics, settings, reduced motion, editable naming, privacy, help, welcome, account comparison/promotion/sign-out/handoff, recovery confirmation/exit | Candidate |

Product appearance milestones are ownership plus signature I/IV/X this run. Permanent shop decoration thresholds are 0/100/250/500/750/1,000 lifetime unique cash upgrades. The candidate keeps the original stall composition; the progression sheet explains these milestones. Final reconstruction must fit the existing layered progression artwork to the approved layout. The sheets reuse product/skill icons for cards and pack illustrations; bespoke new card/pack artwork is not claimed.

## Fixture and runtime boundary

`Tools/ClassicUiPreview/Program.cs` compiles the existing core read-only in a separate .NET console project and exports six disposable states: new, first shelf stocked, growing, established, prestige-ready, and rebuilt. It never loads a real player save. New→first-stock and ready→rebuilt use actual `BuyLevels` and `Prestige` commands with an in-memory persistence callback. Product/tap quotes, upgrade prices/projections, skill data, card bonuses and prestige previews come from that core.

The browser is a **presentation prototype, not the live game**. It does not accrue income, run the C# economy, store progress, send network/authentication requests or take payment. Ordinary purchases demonstrate a frozen result and then require resetting the snapshot to repeat that level example. Changing review states is explicit. Deterministic sample draws exercise reveal states; they are not a random draw implementation or evidence of production odds.

Unsearched category and product/skill-target Buy All examples use exported native quotes, including uniform discounts and prerequisites unlocked earlier in the batch. Arbitrary search or previously previewed purchases use a frozen-price interaction example; that filtered total is illustrative, and the external review status says so. Native reconstruction must call `QuoteBuyAll` for every live scope; never port this browser fallback into the game.

The single `unity-metadata` block records actions, binding containers, stable collection identities, navigation, state coverage and native owner paths. Dynamic controls register their exact DOM IDs into that block. Presentation and account/recovery entry-point reconciliation remains candidate work for the separately authorized native stage; nothing is labeled Unity-integrated. New four-tab routing requires adaptation of the current native screen router.

## Review controls

Desktop controls sit beside the phone. On narrow screens, **Preview** opens the review controls. Choose a screen, game snapshot and interaction condition. `?screen=Shop`, `?state=new`, and `?condition=pending` support direct links; `?capture=1` hides review tooling. A selected bottom tab or the collapse arrow returns to Home. Escape closes one sheet; blocking recovery remains blocking.

Conditions cover normal, insufficient funds, skill active/cooldown, stale quote, save failure, store loading/unavailable/pending/cancelled/failed/delivered and empty catalogue. IAP outcome buttons are explicitly labeled preview actions. Prices are authored INR examples; the eventual store is the localized-price authority.

## Validation and reproduction

From the repository root:

1. `dotnet run --project Tools/ClassicUiPreview` exports core-derived fixture/content files.
2. `node Tools/ClassicUiPreview/build.cjs` reconstructs the HTML shell and behavior contract while leaving the shared baseline alone.
3. Serve `UI Builder/HTML` locally (this review uses `http://127.0.0.1:8765/Classic/`).
4. `node Tools/ClassicUiPreview/verify.cjs` checks/captures every review route at 390×844, 360×640, 360×540 and 450×916.
5. `node Tools/ClassicUiPreview/interactions.cjs` exercises connected browser behavior, including every catalogue page and actual controls.
6. `node Tools/ClassicUiPreview/states.cjs` checks new/stocked/rebuilt/active/locked/empty/save-error/store states and captures the desktop review surface.

Results: **136 route/viewport checks**, **63 interaction checks**, and **30 snapshot/state checks**, with no browser errors. Game buttons meet the 48px minimum in the layout checks. Key Home, Player, Products/upgrade, Legacy, Shop, Talent, reveal and purchase captures were visually inspected; short-screen Home clipping and small shelf targets were corrected. The desktop review surface was inspected as well. Reports and clean captures are in `renders/`.

These establish browser presentation/interaction behavior only. They do not establish user approval, real IAP/authentication, Unity parity, save correctness, or device runtime acceptance. Candidate 1 awaits the user's whole-prototype review.

## Product popup reference revision — 2026-10-06
Rebuilt ProductDetail around the supplied Restaurant reference: floating title plaque, outside circular close button, framed art/name/level, centered earnings, milestone progress and collection bonus progress. Uses actual x2 milestones and authored portfolio upgrade requirements/rewards; collection bonuses remain purchases, not invented automatic synergy rewards. Product purchases stay on the Products tiles. Short screens scroll within the popup. Salesmanship remains unchanged. Verified at 390x844 and 360x540, and all 63 interaction checks pass. HTML review only; Unity implementation awaits approval.


## Remove upgrade detail popup — 2026-10-06
Removed UpgradeDetail from review screens and upgrade row navigation. Upgrade effects, requirements and purchase buttons remain inline in the lists. Removed View bonus from the product popup; skill upgrade links open the relevant list. Legacy UpgradeDetail URLs return to Player. Updated interaction checks: 61 pass.


## Inline Legacy collection — 2026-10-06
Legacy now contains all 28 cards grouped by rarity, discovery count, balances, draw controls, and resumable reveal. Removed the separate Talent sheet from the review menu; old Talent routes and Shop collection links lead to Legacy. Individual card details remain. Reveal completion refreshes the inline collection. All 62 interaction checks pass. HTML only.


## Full card Option 1
Applied the selected Classic Card hierarchy to the HTML CardDetail popup: card name/level/rarity, large unique artwork, actual current bonus, short flavor description, permanent-bonus footer, and outside close. Removed next-level, odds, and precise-benefit controls from this view. Rarity controls border/level colors. Checked 390x844 and 360x540 and close return to Legacy. Unity remains outside this HTML revision.


## Full-card proportion and rarity correction
Replaced content-stretched framing with a centered 0.65-ratio portrait card that scales to available viewport height. Content uses fixed safe insets and card-relative typography, with a square artwork frame. Generated dedicated Rare plum/floral and Legendary saffron/raspberry sunburst outer and inner borders. Verified all three at 390x844 and 360x540 with no content overflow; inspected screenshots. Generation prompts are in assets/cards/rarity-border-prompts.json. HTML-only revision.


## Regenerated product merchandise
Eight separate transparent painted product icons now replace the atlas crops throughout the Classic HTML preview. Home shelf slots align with the stall artwork, with bottom-aligned merchandise and labels on the painted nameplates. Verified image loading on Home, Products and ProductDetail, and shelf layout at 390x844 and 360x540. Source prompts are preserved in assets/products/icons/manifest.json. This revision applies to the HTML prototype.


## Festival motion pass

The Classic HTML prototype now has a shared motion layer in motion.js and motion.css. Existing screen composition, artwork and economy fixtures are retained.

- Atmosphere: twelve slow drifting gold motes, warm lantern glows and a subtle shopkeeper idle. Ambient motion pauses behind dialogs and while the document is hidden.
- Interaction: brief button press and hover responses, panel entrances with visible-row staggering, dialog entrances and a 160 ms inert dismissal echo. Navigation and focus complete immediately.
- Feedback: counter coin trails, shelf stock pops, purchase bursts, skill readiness/active glows, animated milestone fills, rarity-colored card reveal celebrations and prestige confetti. Feedback follows successful preview outcomes; it cannot grant rewards or spend resources.
- Motion preferences: the existing Reduced motion switch and OS preference disable both CSS and Web Animations, including pseudo-elements. A preference change during a card flip settles the card and makes Continue available. Tap amounts remain readable without animation.
- Performance: transient particle groups are capped, removed after completion, and cancelled when motion is disabled. Hover effects apply only to mouse-like devices; undiscovered cards stay disabled.

Validation: node Tools/ClassicUiPreview/motion.cjs passed 37 checks. Report: renders/motion-report.json. Checked 390x844 and 360x540 layouts. HTML review only.
