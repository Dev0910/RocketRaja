# Home asset provenance — historical cartoon pass

**Current Home assets:** see [GOUACHE-PROVENANCE.md](GOUACHE-PROVENANCE.md). This document records the superseded cartoon pass. Its asset and icon descriptions below are historical.

Generated for the approved rank-free cartoon reference on 2026-09-14 using the built-in image generation tool. No third-party game illustrations are embedded in this HTML.

| Local asset | Origin | Use |
| --- | --- | --- |
| stall.png | Generated exec-01c47235-367e-4f36-a4fb-42392b4f14ed.png | Empty illustrated shop/background, with separate live HTML text |
| products-alpha.png | Generated exec-6a2d9add-2d6e-456c-90dd-b194168e2778.png | Eight independently displayed atlas regions, confirmed RGBA with real transparency |
| shopkeeper.png | Generated exec-1d044820-f1bc-4ca8-9c1f-b4a103cb4a5f.png | Separate character layer and navigation portrait |
| Outfit.ttf | Existing project SourceArt/Fonts/Outfit-variable.ttf | Local variable font |
| Outfit-OFL.txt | Google Fonts official Outfit OFL, https://raw.githubusercontent.com/google/fonts/main/ofl/outfit/OFL.txt | Font license |

Generated originals remain in C:/Users/Dev/.codex/generated_images/01a09bfc-44e2-7380-ad4b-3263ea4aaa87/. Products were generated as an atlas, then edited with imagegen to remove the generated checkerboard background. All slicing is presentation-only SVG viewBoxes; no image pixels were manipulated programmatically. This package is not a native Unity import manifest.

Icons are original inline SVG shapes in home.html. All cash, names, status text, sign lettering and navigation labels are HTML, not baked into illustrations.

## Generation prompts

### html-stall

Use case: stylized-concept. Create a single SQUARE background illustration asset for an editable HTML game screen, using attached image only as style and shop design reference. Recreate its rich bold outlined 2D cartoon Indian Diwali shop at night, deep indigo sky, festive strings of golden lights, marigold garlands, maroon and cream striped awning, wooden retail shelving and counter. Front facing, centered full shop, no perspective tilt. Critical geometry: top 0–20% is dark evening sky and distant neighborhood silhouettes, reserved empty for HTML cash overlay. Striped awning from y20% to29%. Wide BLANK maroon shop-sign plaque with gold trim y29–38%. Eight completely EMPTY rectangular shelf compartments in two rows of four, x8–92%, y40–72%, with slim wooden dividers. All compartment backs dark warm wood. Bottom 72–100% wooden counter front with maroon decorative inset and subtle golden diya ornament. Blank cream label strips below each compartment, no text. No shopkeeper, no people, no products, no fireworks packs, no coins, no gear, no buttons, no progress bar, no UI, no lettering or numbers anywhere. All products, characters and all text added as separate HTML layers. Full bleed square background with beautiful polished rich cartoon wood details and warm lights matching reference.

### html-products

Use case: stylized-concept. Create a transparent PNG game sprite atlas, square canvas, EXACTLY EIGHT isolated cartoon fireworks product display clusters in a perfectly regular FOUR COLUMNS BY TWO ROWS grid. Every tile occupies one quarter of canvas width and one half of height. Each cluster is centered horizontally and vertically WITHIN ITS OWN TILE, with generous transparent margins, no overlaps or crossing tiles. True transparent background, no backdrop, no floor, no cast shadows beyond object silhouettes, no labels or grid lines. Match the attached game's bold richly saturated cartoon retail packaging, crisp colored dark outlines and warm cel shading. Order: TOP LEFT long blue gold-patterned sparkler cartons; TOP SECOND red green gold conical Anar flower pots; TOP THIRD circular colorful Chakri spinner packs; TOP FOURTH red-tipped striped rocket fireworks on sticks; BOTTOM LEFT stacked small red firecracker packets; BOTTOM SECOND bundled red cracker strings with one red gold carton; BOTTOM THIRD blue gold-patterned cylindrical aerial-shot packs; BOTTOM FOURTH stacked colorful burgundy teal gold gift boxes with bows. Each distinct cluster of 3–5 unlit retail items should fill about 75% of tile width and 60% of tile height, stand upright in front view and have a coherent baseline. Only these 8 clusters, no extra icons or duplicated rows. Illustrative package motifs instead of writing: NO readable text, NO names, NO numbers, NO interface. This atlas will be displayed as eight independent product sprites over empty shelves.

### html-keeper

Use case: stylized-concept. Create an isolated transparent PNG character sprite matching the friendly adult Indian shopkeeper from the attached reference: warm medium brown skin, rounded friendly face, thick dark wavy hair, expressive eyebrows, neat curved black moustache, smiling, cream kurta with subtle gold trim and maroon scarf. Bold polished 2D cartoon with colored outlines and simple rich cel shading, not photorealism. Front view, waist-up including both forearms and hands gently resting at a horizontal implied counter, no actual counter. Centered, entire hair, shoulders and hands uncut with comfortable transparent margin. Actual transparent background, no scenery, no money, no UI, no text, no props, no shadow rectangle. Single character only. Intended as separate HTML artwork layer at bottom right of stall, can also be cropped via CSS for navigation portrait.

### Products transparency edit

Use case: background-extraction. The attached product sprite atlas incorrectly has an opaque gray checkerboard painted behind it. Remove EVERY gray and white checkerboard square from the background and replace the entire background with REAL alpha transparency in the output PNG. Do NOT draw a checkerboard, do not replace with solid white or black. Preserve all eight original colorful fireworks clusters, their exact 4-column 2-row positions, scale, clean dark outlines and colors. Keep the same square canvas and empty margins. Retain transparency around every product and within spaces between rocket sticks. This is a usable sprite sheet; an opaque checkerboard is a failure. No new objects, no text, no changes to composition. Actual transparent background.

## Art scope

One display style per family and one decorated shell are supplied for this Home reference. New-shop review hides all product art and changes numbers/states; it does not deliver the four final production store stages or the 32 progression-tier sprites.

