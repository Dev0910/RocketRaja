# All-screen icon refresh — 2026-09-18

The user requested a review and style correction of icons across all existing screens in UI Builder. The current Ivory Bazaar interface and approved Gouache Storybook scene are the visual authorities. This pass updates existing HTML presentation; it does not create new screen compositions or claim user approval of the new artwork.

## Result

Sixteen generated painted icons now share a palette, light direction, framing and silhouette scale. Cobalt, warm ivory, marigold and raspberry connect the icons to the scene and folk ticket buttons. The Player tab uses a purpose-made readable bust instead of shrinking the full shopkeeper illustration. Statistics, Skills, Salesmanship and Prestige no longer mix unrelated flat glyphs with painted navigation. Prestige has a diya/renewal symbol distinct from the reputation medal.

Three utility symbols—close, collapse and empty-slot plus—share rounded editable SVG strokes and inherit their interface color. The eight product-family illustrations and existing floral/lotus/sprig decorations were checked and retained: their painting and folk ornament treatment already match the scene and UI. Historical wireframes, selected references and prior captures are preserved as review history.

## Coverage

| Screen | Icon review and treatment |
| --- | --- |
| Home, shared and standalone | Settings, milestone, coins, tapping, all three skills, padlock state, optional-video ticket and all three navigation icons now share the atlas. |
| Player | Painted statistics ledger, skill book, tapping hand and prestige diya; shared header and navigation icons. |
| Products | Shared navigation and collapse control; all eight painted retail silhouettes retained and checked inside their display frames. |
| Legacy | Shared navigation; eight matching product-family illustrations; painted tapping and all-products symbols; correct clock for offline capacity and hourglass for shorter cooldowns. |
| Statistics | Shared close control and persistent shell icons; the corresponding Player shortcut uses the painted ledger. |
| Skills | Same Festival Rush, Helping Hands and Bulk Order artwork as Home; read-only guide states and cooldown text preserved. |
| ProductDetail | Matching painted product crop; shared close control and persistent shell icons. |
| Prestige | Shared close control; corresponding Player entry uses the distinct renewal diya. |
| Settings | Matching painted gear in the shell, shared close glyph; existing labeled switches preserved. |
| Offline | Painted earnings coins, shared close control and shell icons. |
| Sponsor | Painted Festival Rush burst, video entry ticket, shared close control and shell icons. |
| Privacy | Shared close control and persistent shell icons; existing text-based choices preserved. |
| SaveRecovery | Updated persistent shell icons; blocking recovery choices remain the only actions. |

## Sources and reproducibility

- `HTML/Shared/icons/ivory-gouache-atlas.png`: built-in imagegen output, preserved byte-for-byte with real alpha.
- `HTML/Shared/icons/manifest.json`: per-icon crop rectangles and usage map. The generated atlas is not a perfectly regular grid; each crop is specified explicitly.
- `icon-library.cjs`: single symbol library, shared controls and icon catalogue generator. SVG clipping/transforms isolate artwork without modifying the raster or inheriting generic nested-SVG sizing.
- `HTML/Shared/icons.css`: optical sizing while preserving control hit targets.
- `build-screens.cjs`: uses this library for all twelve shared HTML entries and refreshes standalone Home definitions.
- `HTML/Shared/icon-catalogue.html`: all sixteen icons on ivory, blue and raspberry, plus 24/32/40-pixel samples.
- `HTML/Shared/icons/PROVENANCE.md`: exact generation prompt, input references, generated source path and alpha evidence.

While rebuilding, the existing generator was found to omit current floral header/toolbar decorations and scroll footers. These details are now retained by the source generator and shared stylesheet. The older generic quantity interaction test also attempted to select MAX without opening the current menu; it now opens that menu before selecting, matching the actual user flow.

## Verification

- **56 icon/layout checks**, covering all thirteen screens and standalone Home at 360×540, 390×844, 450×916 and 1080×2200. All fourteen embedded Unity behavior contracts hash-identical to the pre-change baseline; exact stall and navigation geometry preserved. Accessible navigation names and minimum 48-pixel control targets retained.
- **79 standard viewport/state checks and 17 interaction checks**, passing with zero browser errors.
- **21 additional Ivory Bazaar review checks**, including scroll reachability, unclipped text, modal footers, independent quantities, focus/history and gallery image loading.
- **11 standalone Home captures and 11 interaction checks**, passing with zero browser errors.
- **Six targeted locked/ready/cooldown icon-state checks** across both Home implementations and a check of the two Legacy timing-perk icons.
- Visual inspection of every canonical screen at short-phone size, the icon catalogue across three backgrounds, and larger Player/Skills views. Current evidence: `all-screens.png`, `icon-catalogue.png`, and each screen's `renders/icons-*.png`.

Rebuild with `node "UI Builder/build-screens.cjs"` and `node "UI Builder/build-gallery.cjs"`. Verification scripts are under `HTML/Shared/` and `HTML/Home/`. Pre-change source backup and contract/layout signatures are stored in this directory. No Unity code, Unity assets or saved games were changed.
