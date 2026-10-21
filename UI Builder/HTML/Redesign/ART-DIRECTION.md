# Redesign HTML handoff — 2 October 2026

The user requested completion of the redesign and explicitly waived UI approval pauses on 2 October. Wireframe → generated art → editable HTML order was retained for all 13 screens. This is permission to continue, not a claim of human visual acceptance. Native reconstruction is authorized by that same request.

## Sources and inspection

- Editable structure: `../../Wireframes/Redesign/*-v1.svg`, with the screen/state register in `NOTES.md`.
- Gouache Storybook style: `../../Reference Images/Home/Style Explorations/2026-09-17-scene-only/05-gouache-storybook.png`.
- Thirteen individual references: `../../Reference Images/Redesign/*-v1.png`; Kaju Katli uses `kajushop-test-v1.png`. `GENERATION.json` records prompts and provenance. Each generated result was inspected before producing the HTML.
- Canonical editable package: `redesign.html`, `redesign.css`, `preview.js`, `catalogue.js`. Reproduce content/metadata with `node Tools/RedesignLab/BuildPreview.cjs` from the project root. The generator requires the preceding reference files.
- Existing painted layers: `../Home/assets/gouache-stall.png`, `gouache-products.png` and the inspected static `gouache-paper.svg`. Existing local Nunito and Fraunces fonts come from `../Shared/fonts/`. These are independently composited art, not screen screenshots.
- Run `node "UI Builder/HTML/Redesign/verify-preview.cjs"`. `renders/verification.json` records 66 checks/captures, including 52 screen/viewport checks and ten extra state captures. All 13 reference-size renders and the compact Home were visually inspected. Layouts were checked at 390×844, 360×640, 360×540 and 450×916; 36 PNGs are retained. `inspection.json` records static package inspection. Executable local fixture scripts were read before running; external HTTP(S) is blocked by the preview verifier.

## Reconciliation with generated art

Keep the cream paper, maroon frame, teal primary action, restrained gold borders, Nunito body and Fraunces headings. The Home keeps eight stable shelf positions and an independent counter tap target. Unowned products are visibly faded. Short screens retain all touch targets and readable labels; the illustration yields space to controls.

Generated spelling, sample names and amounts are not gameplay authority. Authored catalogue IDs, core commands and the specification govern the native binding. The HTML removes the generated KK marketing slogan/oversized hero and uses a prominent **TEST SHOP · No real payment** banner. Pack prices are explicitly design examples and never charged. The old generated shop-name sign is replaced by the current game's heading. The next stage must use the authoritative projection for each selected item rather than copying another item's sample numbers.

## Functionality and fixture boundary

The one `unity-metadata` block maps 31 actions and 22 bindings, navigation, collection policy, persistence, input guards, commerce and accessibility. HTML scripts are offline presentation fixtures: no save storage, network, Firebase, Unity Purchasing, credentials or entitlement authority. Screens show independent examples, not one consistent player save. The test-shop browser balance demonstrates outcomes but is not native delivery evidence. DevelopmentShop.cs supplies the native persisted, isolated purchase commands.

Search exposes authored entries from the complete 1,000-item catalogue. Six browser example rows with inspection paging are not native virtualization. Reconstruct a ScrollRect with an Editor-preplaced reusable row pool, full content extent, retained filters/scroll, and authoritative requirements/affordability. Details, Buy All and reveal numbers in static examples must be replaced with immutable/current core projections. The native batch must contain every quoted entry, never just the three illustrated rows. The 28 card buttons use reusable product icon placeholders; bespoke progression artwork remains RR-08 work.

Verified preview interactions include search/empty/return, quantity selection, UI/modal tap isolation and the three-tap ceiling, test purchase pending/success/duplicate/cancel/failure, paid-draw deduction, and selected product/card/skill identity. Locked/ready skill controls, zero balances and below-target prestige copy are consistent. Additional screenshots cover loading, unavailable, pending, save failure, stale quote and progressed Home. These are not proof of native recovery, lifecycle, account or billing behavior.

## Native reconstruction direction

Replace the Main gameplay presentation in its verified production owner. Preserve the complete legacy hierarchy inactive and its saved references. Use one shared header/footer and safe content area, Editor-authored scroll views and native TMP/Button/Image components. All 13 screen roots, modal states, list rows, card cells and effects must be serialized before Play Mode. Runtime code only binds state, reuses rows and animates existing objects.

Bind gameplay to RedesignRuntimeDriver/RedesignEngine, settle offline credit before input, and keep income/cooldowns active in menus. Only real application background suspends the session. Only the counter accepts gameplay tap gestures. Settings, privacy, naming and recovery remain reachable through adapted existing saved screens. Live Google/Play connection stays unavailable in this explicitly isolated prototype.

Inspect the actual CanvasScaler/safe area before mapping 390-unit reference geometry; use stretch anchors for the body and edge anchors for header/footer. Buttons stay at least 48 logical units. Body content scrolls on short devices. Verify rupee glyphs and long names without changing shared font assets.

Motion: button press 80 ms; screen opacity 0.6→1 over 160 ms; saved card reveal opacity/scale 0/0.93→1/1 over 400 ms, with skip settling the already-committed result. Reduced motion settles immediately. Native interruption/reopen and start/in-flight/end checks remain necessary. No reward is granted by animation completion.

Native scene integration, Unity visual parity, serialized ownership, device interaction/performance, 32 product looks/six shop stages, and player testing remain separately open. This handoff completes the preceding visual artifact sequence; it does not close those implementation tasks.
