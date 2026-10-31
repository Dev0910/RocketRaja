# Shopkeeper tutorial — wireframe revision 1

29 October 2026. **Wireframe revision 1 approved.** The user confirmed: “The wireframe looks good. Approve revision 1 and generate the art reference in our Gouache Storybook style.” This approval covers the nine states and short-screen layouts linked below. Generated art references are a separate review stage.

## Review

- [Complete nine-state review board](tutorial-r1-review-board.png) · [editable SVG](tutorial-r1-review-board.svg)
- [Opening sequence, larger four-state board](tutorial-r1-opening-board.png) · [editable SVG](tutorial-r1-opening-board.svg)
- [Short-screen checks](tutorial-r1-compact-checks.png) · [editable SVG](tutorial-r1-compact-checks.svg)
- [Gameplay and teaching flow](../../../Game%20Design/Shopkeeper%20Tutorial%20Draft.md)
- [Generated Gouache Storybook references, revision 1](../../Reference%20Images/Tutorial/ART-DIRECTION.md) — generated after wireframe approval; art-reference approval pending.

Blue identifies the proposed tutorial components and suggested controls. Gray shows simplified existing game structure. The shopkeeper is a schematic placeholder for the existing character, with the bubble pointing toward him. These are structural wireframes, not proposed final artwork or a replacement art style. Existing Player, Products, Legacy and Shop navigation remains usable.

## States

| File stem | State and behavior |
| --- | --- |
| `01-welcome` | Home introduction. Short bubble, “Let's start”, Skip tutorial, and the persistent next-step card. The counter remains exposed. |
| `02-saving` | Bubble collapsed while earning. Card shows `Train Salesmanship · ₹6 / ₹10`; the counter is suggested. The card routes to training and `?` reopens the instruction. |
| `03-salesmanship` | Player → Salesmanship. Coach highlights the existing training action and displays the ₹1 → ₹2.40 improvement. Other navigation remains available. |
| `04-training-result` | Home acknowledgement after the successful purchase. Explain actual tap improvement and advance the card toward the first product. |
| `05-first-product` | Products. Highlight first Phuljhari stock purchase at ₹30 and explain automatic income. |
| `06-first-upgrade` | Player. Recommend a useful upgrade for an owned product, with ₹0.85 → ₹1.70/sec as an unboosted example. The full runtime title remains “Phuljhari Signature Breakthrough I”; its wireframe row uses the shorter label for space. |
| `07-achievement` | Home ready-reward handoff. Example: four stocked products earns Open for business (+5 KK). Trigger whichever first achievement is actually earned. |
| `08-prestige` | Home handoff to the existing prestige preview. Player reviews reset/keep rules, Fame and three free pulls before confirming. This card opens the preview; it does not prestige automatically. |
| `09-free-pull` | Legacy after first prestige. Highlight one free 1× pull, showing the three available credits. The 5× paid option is disabled in this zero-KK review fixture. It does not become part of the tutorial. |

Standalone screens have editable SVGs at 390 × 844 and PNGs at twice that resolution. Welcome and saving also have 360 × 540 SVGs/PNGs. On the short screen, the speech bubble narrows to leave the shopkeeper visible, with the counter directly below it. All new buttons and the navigation controls retain at least 48 × 48 logical-pixel touch targets.

## Interaction intent

- The speech bubble is temporary; the next-step card stays visible during earning/waiting. Its main area opens the target route, and its separate `?` control reopens help.
- For purchases, collapse the Home bubble and use an anchored hint in the destination panel. Do not introduce a blocking spotlight or lock unrelated navigation.
- Show a purchase result only after the successful game command. Use live costs and before/after values; fixtures do not define new balance.
- Existing users receive no automatic tutorial. Missing tutorial fields on an old save must not enroll it as a new user.
- Tutorial completion/skip persists across launches and prestige. Reopening help does not reset completed objectives.
- Achievement, skill and prestige coaching can trigger independently. The first achievement does not gate prestige; buying ahead or using Buy All advances the relevant teaching objective.
- Introduce Talent only after the first completed prestige. Prefer an available free credit and never substitute a 100 KK spend if the user has already used all their credits.
- Do not show multiple coach messages over startup, save recovery, offline rewards or a card reveal. Reveal explanation follows the actual received card and handles duplicates.
- The 48-pixel controls, text measurements and simplified screen composition are checked as wireframes; no native input, saving or responsive implementation has been performed.

## Sources and scope

The latest user directions control tutorial behavior. Layout context comes from the saved native Classic capture `Documentation/Implementation/ClassicHTMLSync/Unity20261025/01-home-square-settings.png` and the active Classic binding/authoring code. The current four-tab navigation is preserved; older Home wireframes and older skill guardrails describe an earlier three-tab game.

Costs and effects were checked against `RedesignState.Fresh`, `RedesignEngine.LevelQuote`, `TapPayout`, the content catalog and `RedesignAchievements`. Current first training is ₹10, first product is ₹30, and an unboosted first training changes tapping from ₹1 to ₹2.40. The prestige/income figures on state 08 are illustrative existing-screen context; runtime teaching must read the actual prestige preview.

Validation: 11 screen variants passed canvas/48-pixel target checks and text-width checks using Segoe UI. SVGs and PNG dimensions are checked by `validate-wireframes.py`. This is a static editable wireframe, with no interactive HTML prototype, generated illustration or Unity changes.

Rebuild with `node build-wireframes.mjs --sharp <installed-sharp-package-path>`, using the bundled dependency path returned by `load_workspace_dependencies`. Run the Python validator from this directory or the project root using a Python installation with Pillow.

The project’s [image-to-html skill](../../../.agents/skills/image-to-html/SKILL.md) says: **“Save editable wireframes and previews under `UI Builder/Wireframes/<Screen>/`, present them, and wait for confirmation before generating art references.”** The user supplied that confirmation on 29 October 2026 and directed art-reference generation. HTML and native reconstruction remain later stages.
