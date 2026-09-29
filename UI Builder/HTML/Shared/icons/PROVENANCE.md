# Ivory Bazaar icon collection

Created on 2026-09-18 using the built-in imagegen tool for the user's request to coordinate icons across every existing UI Builder screen.

The generated atlas is original artwork; reference inputs are existing project artwork. This is an HTML icon refresh, not a new screen layout or a Unity import. The prior atlas remains in Home/assets as historical provenance.

Output: `ivory-gouache-atlas.png` (1254 × 1254 RGBA). The tool returned this resolution despite the larger requested canvas. Each source icon has approximately 200–330 pixels of detail, sufficient for the current phone UI and 1080-pixel reference viewport.

Generated source: `C:/Users/Dev/.codex/generated_images/01a0b10b-1d66-7e01-9ced-81c998c5231e/exec-e0cea4aa-e79d-41f1-b40b-9df1ad0b31fe.png`.

Alpha inspection: 808,748 fully transparent pixels; 761,551 partially transparent pixels. The PNG is copied byte-for-byte. No raster editing, background removal, or recoloring was performed. `manifest.json` contains optical crop rectangles rather than assuming the requested regular grid; SVG viewports isolate each icon without changing the image. The same shared symbol library serves standalone Home and every generated screen.

Reference inputs:

- `Reference Images/Home/Style Explorations/2026-09-17-scene-only/05-gouache-storybook.png`: selected painting style.
- `HTML/Player/renders/blue-ticket-navigation-450x916.png`: current Ivory Bazaar UI and blue ticket buttons.
- `HTML/Home/assets/gouache-icons.png`: previous icon subjects.

Three simple utility glyphs (close, collapse and empty-slot plus) are editable SVG paths in `icon-library.cjs`, with the current UI color and consistent rounded strokes. Existing product illustrations and folk floral ornaments already match the scene/interface and remain shared project assets.

## Exact generation prompt

> Use case: stylized-concept. Asset type: one production UI sprite atlas for the Indian festival game Diwali Dukaan.
> Create ONE square 2048 x 2048 RGBA transparent sprite atlas, exactly 4 columns by 4 rows of evenly spaced icons, 512 x 512 pixels per cell. No grid lines, no labels, no words, no text, no background tiles. Genuine transparent empty canvas. Each icon is completely separate, centered within its own cell, contained within the central 400 x 400 area with at least 56 pixels of empty padding around each cell edge. Same perceived size and visual weight for all 16. No overlap and no clipping.
> Style references: Image 1 is the approved Gouache Storybook scene, use its warm hand-painted opaque pigments, irregular brush edges, friendly forms and Indian festival aesthetic. Image 2 is the CURRENT Ivory Bazaar interface and blue ticket buttons; match its cobalt blue, warm ivory, marigold gold and raspberry. Image 3 is the OLD icon atlas, only subject reference: improve its uneven framing, too much bright yellow, excessive tiny detail and bevels. New icons must be visibly hand-painted 2D, simple shapes with cream highlight strokes and restrained dark indigo contours, warm light from upper left, matte gouache texture. Readable as tiny 28-40px UI symbols. No glossy plastic, no 3D bevels, no gradients, no huge badges or background plaques. Keep enough cream and gold around outer silhouettes to be readable on dark cobalt buttons as well as ivory cards.
> EXACT CELL ORDER left to right:
> Row 1: (1) friendly Indian male shopkeeper head-and-shoulders portrait, moustache, dark swept hair, ivory kurta and raspberry scarf, match the scene shopkeeper; (2) small warm wooden two-tier shelf holding three simple cobalt/raspberry/gold fireworks packs, no readable text; (3) one gold five-point reputation star with two raspberry ribbon tails; (4) cream-and-gold eight-tooth settings cog with cobalt round center.
> Row 2: (1) simplified festive firework burst, bold marigold and ivory rays with small raspberry sparks, compact strong silhouette; (2) two helping hands shaking, raspberry sleeve left, cobalt sleeve right, simple anatomically clear fingers; (3) ochre bulk-order parcel wrapped in ivory twine with a small raspberry tag; (4) three overlapping stacks of matte gold coins, clear thick shapes, no currency symbols.
> Row 3: (1) upright index finger tapping, warm skin, raspberry cuff and three gold tap rays; (2) statistics: three rising cobalt-and-gold bars on a small ivory ledger leaf, no text; (3) skills guide: single ivory-and-gold four-point spark over an open cobalt-and-raspberry book, no text; (4) prestige: small raspberry-and-gold diya lamp with one golden flame and two painted upward renewal arrows, strong simple silhouette.
> Row 4: (1) offline capacity: round ivory clock with cobalt hands and gold rim, two hands only and four tick marks; (2) skill cooldown: simple cobalt hourglass with gold sand and one curved ivory arrow; (3) sturdy warm-gold padlock, cobalt keyhole, ivory highlight; (4) optional video: cream triangular play mark inside a small raspberry film ticket with gold edge and four ivory perforations, no text.
> Each symbol should feel like a lovingly hand-painted miniature from the same coherent icon family. Make useful finished artwork, not a design board.

