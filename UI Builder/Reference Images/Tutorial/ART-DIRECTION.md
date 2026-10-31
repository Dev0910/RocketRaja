# Shopkeeper tutorial — art-reference revision 1

29 October 2026. **Art-reference revision 1 approved.** The user confirmed: “I aprove it star making the html”. HTML creation is authorized; HTML visual approval remains a separate review. Wireframe revision 1 is approved; the user directed this Gouache Storybook generation explicitly. This folder contains painted visual references for review, not production UI assets or a playable tutorial.

## Selected sheets

| Sheet | Covered approved wireframe states |
| --- | --- |
| [Opening](tutorial-art-r1-01-opening.png) | 01 welcome, 02 collapsed/waiting objective, 03 Salesmanship training |
| [Purchases](tutorial-art-r1-02-purchases.png) | 04 actual training improvement, 05 first Phuljhari stock purchase, 06 useful cash upgrade |
| [Milestones](tutorial-art-r1-03-milestones.png) | 07 first achievement claim, 08 prestige introduction, 09 first free Talent pull |

Generation used the built-in imagegen tool. [Full selected prompts and corrections](PROMPTS.md) are retained. Earlier unselected paints are in `iterations/`; their existence is not approval or a second design direction. `layout-*.png` are deterministic contact sheets rendered from the approved editable SVGs for generation input, not new painted references.

## Confirmed sources

- User approval, 29 October 2026: “The wireframe looks good. Approve revision 1 and generate the art reference in our Gouache Storybook style.”
- Layout and interaction authority: [Tutorial wireframe revision 1](../../Wireframes/Tutorial/README.md), including its standalone SVGs, nine-state board and short-screen cases.
- Style authority: [05 — Gouache Storybook](../Home/Style%20Explorations/2026-09-17-scene-only/05-gouache-storybook.png), approved for the game on 17 September 2026.
- Existing character/UI context: `Documentation/Implementation/ClassicHTMLSync/Unity20261025/01-home-square-settings.png` and the original user-provided shopkeeper/action-card crops in this chat.
- Behavior: [Shopkeeper tutorial flow](../../../Game%20Design/Shopkeeper%20Tutorial%20Draft.md), current Classic runtime bindings, engine purchase projections and achievement definitions.

## Visual treatment

Retain opaque hand-painted gouache, maroon wooden stall surfaces, cream/red awning, warm gold lanterns and marigolds, and an indigo/purple festival evening. The established smiling, moustached shopkeeper wears a cream kurta and red stole, positioned at the right of the counter.

Use cream parchment dialogue with a restrained navy/teal edge and small speech tail. Primary actions are deep navy with thin ochre/gold trim; secondary/skip controls stay cream. The waiting state retains the next-step panel with a separate replay `?` control. Highlights use a fine teal/gold dashed outline. Keep readable labels, clear price/payment states and the current four navigation destinations.

## Reconciliation notes

| Element/state | Approved intent and source | Unity evidence | Painted evidence | Difference / treatment |
| --- | --- | --- | --- | --- |
| Speech and waiting | Wireframe 01/02: temporary bubble from shopkeeper; persistent next-step card | Current guidance is optional and can hide the old goal card; no sequential tutor exists yet | Opening left has dialogue, middle has only objective and target | Future tutorial coordinates with existing guidance; painting does not verify runtime behavior |
| Layout geometry | Approved SVGs place the scene bottom around 49% of a 390×844 canvas | Saved native Home keeps the counter and four tabs visible | Painted sheets retain the broad composition but vary vertical spacing, especially the milestone sheet | SVG geometry and short-screen layouts remain authoritative for any later editable implementation; do not copy raster coordinates blindly |
| Shopkeeper and scenery | Existing character, two rows of four product positions | Active Classic upper scene and saved capture | Same clothing/identity and eight labeled positions; initial/after-reset views are empty | Empty-slot parchment/outline decoration in some paints is incidental; no new inventory/card mechanic |
| Training | ₹10 first level, ₹1 → ₹2.40 on an unboosted fresh save | `LevelQuote`, `PreviewLevels`, `TapPayout` | Opening training and purchase-result sheets | Later implementation reads actual purchase data, not fixed painted numbers |
| Product and upgrade | ₹30 first Phuljhari; useful product-profit improvement ₹0.85 → ₹1.70/sec | Engine/catalog/current binding owners | Purchase sheet | Short upgrade title matches wireframe; live text may use the full current title |
| Achievement | Coach whichever reward first becomes ready | `RedesignAchievements`; first ownership goal now needs four products | +5 KK Open for business example | Example is not a mandatory prerequisite for prestige |
| Prestige and Talent | New-user coaching; first Talent teaching after first prestige using a free credit | First prestige grants three credits; normal draw command selects credit when available | Three free credits and highlighted 1× FREE button; disabled 5× cash option | Never turn the tutor into a paid-pull prompt or automatic purchase |
| Navigation/icons | Four existing destinations; navigation stays usable | Classic UI/routes | Player / Products / Legacy / Shop; small icon rendering varies | Preserve existing approved navigation icons and behavior in implementation; incidental painted icons are not a new navigation contract |
| Labels and values | Crisp editable text over artwork | Existing TMP bindings | Raster references depict review copy | Text stays live/editable at the later HTML/native stages; no whole-image screen implementation |

## Inspection

Inspected all selected sheets for correct state order, shopkeeper identity and placement, dialogue versus collapsed state, four navigation labels, eight product positions, ₹10/₹30/₹20 prices, ₹1 → ₹2.40 training, +5 KK achievement copy, and three free credits with a free 1× pull. Corrected duplicate settings icons on the opening sheet and extra shelf compartments on the Legacy reference. Each selected sheet has one settings control per screen.

These raster references are neither pixel-layout validation nor mobile interaction tests. The milestone sheet's roomier upper scene and small differences in decorations/icons remain explicitly subordinate to the approved wireframes and existing artwork.

## Review boundary

The [image-to-html skill](../../../.agents/skills/image-to-html/SKILL.md) requires generated references to be presented and confirmed before HTML. The requested reference-generation stage is complete. Art-reference revision 1 was explicitly approved with direction to create HTML. No HTML, Unity scene, gameplay, balance or save changes are part of this stage.
