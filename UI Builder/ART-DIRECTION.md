# Game art direction — Gouache Storybook

**Status: approved direction.** On 2026-09-17, the user selected **05 — Gouache storybook** from the scene-only exploration and explicitly extended this style to all UI and artwork in the game.

## Authoritative reference

[05 — Gouache storybook](Reference%20Images/Home/Style%20Explorations/2026-09-17-scene-only/05-gouache-storybook.png)

This exact scene-only option is the selected reference, not the earlier full-page storybook variant. Earlier bold-cartoon references remain useful layout history but no longer define the rendering style.

## Visual language

- Hand-painted 2D gouache: visible pigment texture, warm dry-brush edges, layered opaque color and softly irregular painted forms.
- Warm golden light against indigo/purple evening shadows; maroon fabric, ochre wood, cream highlights and teal accents.
- Friendly expressive Indian characters, painted clothing and restrained ink details. Keep silhouettes and expressions readable at phone size.
- Product families stay recognizable through shape and color. Brush texture supports their forms; it must not obscure them.
- Festive marigolds, lanterns and Indian neighborhood details use the same painted treatment. Maintain depth through overlapping layers and lighting.
- Avoid mixing glossy plastic 3D, cut-paper construction, pixel sprites or flat vector scene assets into this direction.

## Apply across the game

| Area | Treatment |
| --- | --- |
| Stall stages and backgrounds | Painted wood, fabric, dusk streets, warm lantern light; distinct progression silhouettes |
| Product displays and tiers | Painted retail packs, coherent highlights and texture; stable two-by-four shelf placement |
| Shopkeeper and customers | Same illustrated character language; separate parts/layers where animation needs them |
| Skill, product and navigation icons | Simplified gouache illustrations with strong silhouettes and consistent light; readable at small sizes |
| Panels, buttons and overlays | Subtle parchment/painted surfaces and restrained hand-painted borders; cream, maroon, teal and gold |
| Typography and numbers | Clean live text above artwork; crisp rupee symbols, strong hierarchy and readable state labels |
| Feedback and decorative effects | Soft painted glows, sparks and simple celebratory shapes consistent with the scene |

The user's earlier “UI is fine” keeps the existing composition and usability. Their latest approval extends the selected artistic finish to UI surfaces and icons as well. It does not require replacing the interface layout with a different design.

## Preserve approved structure

- Portrait reference: **1080 wide × 2200 high**.
- Header cash and settings; no shop-rank heading or rank bar.
- Eight product positions, three skills, three tabs: Player, Products, Legacy.
- Existing screen flow, state distinctions, minimum touch targets and safe-area requirements.
- Editable labels, prices, counters and signage; no changing text baked into production art.
- Existing economy, input guards, save ownership and embedded Unity functionality metadata.

## Production and approval boundary

Use this reference for all subsequent art and UI work. Asset prompts should name Gouache Storybook and include the selected image as the style reference. Keep background/stall, each product display, character parts and icons separate for progression and animation.

The user clarified the mandatory workflow on 2026-09-17: **supplied image as inspiration → wireframe designed for our game → user confirmation → generated reference based on that wireframe → requested revisions and user confirmation → HTML → review → separately directed Unity work.**

Every new screen needs its own approved wireframe and confirmed generated reference. Invoking image-to-html with an image does not bypass these stages. The game-wide Gouache style stays approved, but does not approve a new screen's composition. Record actual confirmation for each artifact/revision; never infer it from an attachment, a file's existence, silence or an assistant-generated draft. End at each review boundary without building the next stage in advance.

Player must resume with a game-specific wireframe informed by the supplied inspiration and current gameplay, followed by generated references and review. The prematurely created Player HTML was not approved and must not serve as the design authority for that wireframe.

The Home HTML received its Gouache Storybook pass on 2026-09-17 and is ready for visual review. See HTML/Home/ART-DIRECTION.md and its generated-asset provenance. Other screens and Unity visuals still require their separately directed conversion steps.

