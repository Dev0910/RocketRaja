# Shopkeeper tutorial — HTML revision 11

**30 October 2026. Candidate for visual review.** Revision 11 replaces tutorial outline highlights with a transparent white-glove pointing cursor generated from the user’s supplied reference. The fingertip follows the active CTA during scrolling and resizing. The overlay never intercepts input; unavailable and offscreen targets, confirmations, reveals, skipped and existing-user modes suppress it. Normal keyboard focus indication remains. Asset: `assets/tutorial-pointing-hand.png`; exact generation prompt: `assets/tutorial-pointing-hand.prompt.txt`.

Earlier revisions: **29 October 2026.** Latest user correction: “remove the 2 buttoons and make the popup smaller”. Both dialogue buttons are removed from the DOM and metadata; the note now measures about 289×74 at 390×844. Tap the note or press Escape while focused to collapse it, or continue using the actual game controls. The persistent next-step card still opens the relevant action. Dismissal never makes a purchase or opens a confirmation. The user accepted the tutorial flow but rejected revision 1's independent UI: “the flow is correct but it is noting like the UI we have in game”. This correction takes precedence over the painted reference's incidental restyling.

Preview: http://127.0.0.1:8772/Tutorial/?revision=5 . Clean capture: `?capture=1&step=welcome`. Select any of `welcome`, `saving`, `training`, `result`, `product`, `upgrade`, `achievement`, `prestige`, `talent` with `step=` or the desktop review selector.

## Selected dialogue finish

The user explicitly selected Ivory & gold and directed HTML implementation. Reference: `UI Builder/Reference Images/Tutorial/Dialogue Variants/2026-10-29/01-ivory-gold.png`. The compact, button-free note now uses a rounded ivory parchment body, double gold rim, two small leaf ornaments, subtle shadow and a continuous outlined tail. `assets/ivory-gold-frame.svg` contains only scalable decorative geometry; text stays live HTML. The title uses the existing local Fraunces face scoped to this selected popup; the rest of the current game typography remains unchanged. Corner ornament clearance is reserved in the heading padding. No screenshot clipping, generated text texture or full-screen raster replacement is used. Native transfer remains a separately directed step.

## Actual game presentation

Revision 2 composes the existing **Classic/index.html** including its full native parity styles, native fonts, exported UI faces, artwork, product icons, navigation, income panel, Player rows, Products page, Talent collection and existing popup routes. It no longer constructs replacement screens with separate typography, simplified icons or a different income layout.

`build-classic-preview.py` reads Classic source without modifying it, adds a relative base URL, replaces the embedded contract with a tutorial-aware version and appends the tutorial stylesheet/controller. The tutorial state projects into the existing Classic fixture presentation; original price curves, UI owners and game persistence remain native authority. The composed HTML still contains live editable text and separate scene/product/character layers. No flattened screen image is used.

| Element | Existing presentation retained | Tutorial addition |
| --- | --- | --- |
| Home scene | Current layered stall, exact shopkeeper, gold wallet, settings face, tap banner | Anchored speech panel and counter highlight |
| Next display | Existing notched row, icon, size and position | Current next-step text plus a separate 48px replay-help hit area |
| Income/skills/navigation | Existing dual cash/tap labels, earning-now panel, skill locks, native fonts and painted buttons | None; values come from the current review fixture |
| Player | Full actual toolbar, Salesmanship row, prestige row, Buy All and upgrade list | Small coach hint immediately before the relevant row and a dashed control outline |
| Products | Actual product cards and stock button | Hint and target control binding |
| Legacy | Actual KK header, draw actions and all 28 card slots | Free-pull hint and outline; no paid fallback |
| Tutor completion dialogs | Current native fonts/colors and button faces | Focused fixture confirmation/result dialogs; full native runtime teaching still pending |

## Approved behavior retained

New saves only; free navigation; collapse speech while working; acknowledge actual first training improvement; first product and useful upgrade; independent achievement/prestige milestones; first free pull after first prestige; skip/replay; save-error guards. All tutorial economic transitions are still explicit in-memory fixtures. No browser storage, Unity bridge or live purchase/save system is added.

Other Classic screens remain the existing preview, rather than newly redesigned destinations. The tutorial adapter intercepts its focused controls before the existing Classic demo handler so one click cannot execute two fixture purchases. Native labels and actual amount formatting own production behavior; the illustrative fixture values are not a new economy.

## Review evidence

`verify-classic.cjs` exercises all nine states at 390×844, 360×540, 360×640 and 450×916, checks the native font/style imports, eight product slots and four tabs, then exercises the counter, actual Salesmanship/Products/Legacy controls, claim duplicate rejection, prestige cancellation, free credits, saving unavailable, old-user exclusion and skip/navigation. Results: **90 checks passed**; no script or missing-asset errors. Captures are under `renders-r2/`. This is browser evidence, not native/device verification.

The original HTML revision is preserved under `revision-1/`; its standalone presentation is superseded, while its flow was accepted. `renders/` and browser-checks.txt belong to that earlier version. Current canonical files are `index.html`, `tutorial.js`, `classic-tutorial.css`, with Classic/Home/Shared relative dependencies.

Native implementation is still tracked as H2U-024. No Unity script, scene or player save was changed. HTML visual approval and any native implementation direction remain separate user decisions.

## Automatic feedback correction

User flagged the old generic “Your stall is earning!” modal. Removed those leftover automatic fixture dialogs: product and cash-upgrade outcomes now use the same compact, button-free ivory/gold shopkeeper note. Note dismissal changes no currency and opens no destination; the next-step card remains the explicit route. Talent results use the actual Classic DrawReveal route with its existing card presentation. Deliberately opened claim/prestige review confirmations retain their current guards. Added browser regression checks for no automatic generic dialog after stock, upgrade and free pull; all 90 checks pass.
