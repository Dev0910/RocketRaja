# Shopkeeper tutorial flow draft

29 October 2026. Planning draft; presentation and gameplay implementation have not started.

## Confirmed direction

- The existing shopkeeper introduces actions through a speech bubble anchored to him on Home.
- While the player works toward an action, collapse the speech bubble and keep the objective in the existing Home card currently labeled `NEXT DISPLAY`.
- Highlight relevant controls while keeping navigation usable. The user selected this behavior on 29 October.
- Introduce earning, Salesmanship training, the first product, cash upgrades, achievement claims, prestige and Talent pulls using actual player progress.

## Confirmed tutorial timing and audience

The user confirmed on 29 October 2026: “Introduce pulls after the first prestige using a free pull, and show the tutorial only to new users. Go ahead and create the wireframe.”

- Introduce Talent after the first completed prestige, using one of its three free pulls. Do not prompt a paid pull earlier.
- Automatically start the tutorial only for new users/new saves. Existing saves do not receive an automatic tutorial or catch-up sequence. Future implementation must distinguish a newly created save from an older save missing tutorial fields.
- Wireframe revision 1 was explicitly approved on 29 October 2026: “The wireframe looks good. Approve revision 1 and generate the art reference in our Gouache Storybook style.” Art-reference generation is authorized; art-reference approval is a separate stage.

## Opening flow

| Step | When to introduce it | Shopkeeper's message (draft) | Persistent action and completion |
| --- | --- | --- | --- |
| Earn | First playable Home, after startup/name prompts settle | “Let's get this stall going! Tap the counter to earn your first rupees.” | `Train Salesmanship · ₹<cash> / ₹<actual cost>`. Highlight the tappable counter; show a brief tap gesture. At the first ₹10 threshold, automatically open Player and show the Salesmanship dialog (user revision, 30 October). |
| Train Salesmanship | First single-level training is affordable and tap level is zero | “A better sales pitch earns more from every tap. Let's train your Salesmanship.” | On first affordability, automatically open Player and show the shopkeeper-anchored dialog matching Step 01 (30 October revision 7). The objective remains available to return later. Complete after a successful purchase. |
| Explain training | First training purchase succeeds | “Nice! Your taps now earn ₹<after> each, up from ₹<before>.” | Show a short acknowledgement, then move to the first product. Use the purchase's actual before/after values. |
| Stock a product | First Phuljhari is unowned | “Stock Phuljhari and your stall will earn money automatically.” | `Stock Phuljhari · ₹<cash> / ₹<actual cost>`. At the first ₹30 cash threshold after training, automatically open Products and show the Step 01-style shopkeeper dialog; highlight Phuljhari’s stock control. Present this once and leave the purchase to the player (30 October revision 8). Complete after purchase. |
| Explain production | First product purchase succeeds | “Your Phuljhari now earns ₹<rate> every second. Keep tapping to grow faster!” | Briefly highlight automatic sales and per-tap income so the two sources are clear. |
| Buy a cash upgrade | A useful upgrade for an owned product is available | “Let's improve what you're already selling. This upgrade boosts your Phuljhari sales.” | On the first affordable Phuljhari upgrade (₹20 in the preview), automatically open Player and show the Step 01-style shopkeeper dialog after purchase feedback is dismissed. Highlight the relevant upgrade once; the player buys manually (30 October revision 9). Use the engine’s actual price and effect. Complete after a successful purchase, including a qualifying Buy All purchase. |
| Explain the upgrade | The first useful cash upgrade is purchased | “Your Phuljhari income increased from ₹<before>/s to ₹<after>/s.” | Finish the short guided opening. Resume ordinary growth suggestions in the action card. |

Current source-derived examples on a fresh, unboosted save: zero starting cash, ₹1 per tap, first Salesmanship level ₹10, tap income after that level ₹2.40, first Phuljhari ₹30, its base income ₹0.85/s, and its first product-profit upgrade doubles that contribution to ₹1.70/s. These are examples for reviewing copy; the implemented tutorial must read live prices and purchase projections. Older design documents mention a starting cash grant; the current `RedesignState.Fresh()` starts at zero.

## Milestone coaching after the opening

These triggers run independently. The player can grow their stall while waiting; completing an achievement is not a prerequisite for prestige.

| Trigger | One-time teaching | Persistent action |
| --- | --- | --- |
| First achievement becomes claimable | “You've earned an achievement! Collect your Kaju Katli reward.” Explain that earning and claiming are separate. | `Claim <title> · +<reward> KK`; automatically open Player once and highlight Achievements with the Step 01-style shopkeeper dialog. The player opens the claim and collects manually (30 October revision 10). Complete after a successful claim, including a different qualifying achievement. |
| First skill is owned and ready | Optional short introduction to activating that skill and its cooldown. | Highlight the owned, ready skill. Reuse its existing effect description. Avoid recommending a locked skill with no useful effect. |
| First prestige becomes eligible | Explain the actual reset/keep preview, permanent Fame improvement, and free pull rewards before the player confirms. | `First prestige ready · View rewards`. Automatically open Player once and highlight View prestige with the same shopkeeper dialog. Keep confirmation and deferral explicit (30 October revision 10). |
| First prestige completes | One short congratulations and rebuild reminder, followed by Talent introduction. | Automatically open Legacy and show the shopkeeper dialog for one of the first prestige’s three free pulls. The player presses the highlighted 1× pull; do not spend a credit automatically (30 October revision 10). |
| First Talent pull is introduced | Explain permanent cards and the free credit payment. The player presses the existing pull button. | `Use a free Talent pull`. Require an available free credit; do not substitute a paid pull if none remain. |
| First card reveal | Explain the received card's actual benefit and that it survives prestige. Handle a duplicate as an improvement to an existing card. | Finish the existing card reveal first, then show the shopkeeper dialog with its exported benefit and permanence. After acknowledgement, return to normal growth goals (30 October revision 10). |

The latest achievement revision starts at 500 taps, ₹1 billion lifetime earnings, 25 unique upgrades, four simultaneously stocked products, or three prestiges. Coach the first reward actually earned, including one reached earlier than expected. Do not issue a tutorial-only achievement or lower its target.

## Presentation behavior

- One short message at a time, with a clear dismissal/continue control. Tapping through dialogue never also triggers a gameplay action underneath it.
- Anchor every tutorial introduction near the shopkeeper’s head using the Step 01 dialogue. Automatically open the relevant panel once when its step becomes ready, point at its CTA with the generated white-glove cursor, and keep actions manual (30 October revision 11). Do not add inline hints or cover controls (30 October revisions 7–10).
- Rename the action card's kicker to `NEXT STEP` during tutorial guidance. Show a concrete action, progress or price, and readiness. Its click target follows the current objective instead of always opening Products.
- When saving for a purchase, keep the speech bubble collapsed. The card updates as cash changes. When ready, use a restrained highlight and make the destination easy to reach.
- When an action has already happened, recognize it and move on. Never require rebuying, undoing progress or returning to a specific order to satisfy the tutorial.
- Offer Skip tutorial and a way to reopen the current instruction. Persist skipped/completed guidance across launches and prestige. Automatically enroll only newly created saves.
- Give startup, offline earnings, purchase confirmations, card reveals and save recovery their normal priority. Queue relevant guidance until the current interaction is complete; show no stack of stale messages.
- Coordinate with the current optional guidance so only one system explains a topic. Currently `ClassicGuidance.Refresh()` hides `Next display` for a Home tip; the new tutor must keep its active objective card visible.

## Completion and persistence rules for implementation

- Acknowledging a message and completing a gameplay objective are separate events. Closing a message does not buy anything or mark a purchase objective complete.
- Use actual successful game commands and saved ownership/counters to advance the flow. Saving failures and interrupted purchases do not advance a completed objective.
- Store tutorial completion independently from resettable run purchases. Do not repeat the opening after prestige.
- Honor free navigation after each one-time, user-requested introduction route (30 October 2026). Dismissal and subsequent navigation never repeat the automatic route; the objective remains available to return manually.
- Use live before/after purchase data for effect explanations; do not infer an increase from a hardcoded multiplier when other upgrades or temporary effects are present.
- A delayed long-term milestone yields to immediate actions. When multiple milestones arrive together, present one relevant message at a time, preferring claimable rewards and the current action's outcome over background introductions.

## Next deliverable

Art-reference revision 1 has been generated from the approved wireframes under `UI Builder/Reference Images/Tutorial/`, covering the opening, purchase and milestone states in three sheets. The user explicitly approved these Gouache Storybook references and directed HTML creation: “I aprove it star making the html”. The editable HTML preview is under `UI Builder/HTML/Tutorial/`. HTML approval and native implementation remain later steps.
