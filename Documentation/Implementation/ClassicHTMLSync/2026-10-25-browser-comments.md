# Classic HTML browser comments — 25 October 2026

The user requested sequential implementation of twelve comments against the Classic dev preview. Each item was changed and checked before moving to the next. These changes are HTML candidates; Unity assets and saved scenes were not edited.

| Comment | Change | Verification |
|---|---|---|
| 1 | Settings uses equal scaled width and height, with a minimum 48 px target. | Live desktop measurement: 58.66 × 58.66 px. |
| 2 | Corrected misplaced disabled upgrade buttons. The final row layout reserves independent icon, text and purchase columns. Purchase functionality retained. | Normal and insufficient-funds states keep buttons inside rows and clear of text. |
| 3 | Removed Player's guidance banner; added a small info button beside the collapse arrow. | Opens the existing Buy affordable upgrades guidance. |
| 4 | Removed parentheses around the Achievements ready count. | Label reads “Achievements 9” in the progressed fixture. |
| 5 | Removed Products' quantity banner; added a header info button. | Opens existing Choose your quantity guidance. |
| 6 | Anchored stock buttons inside the right edge and constrained the product-detail click region. | Stock buttons remain inside all eight rows without overlapping product details. |
| 7 | Removed the Legacy gift banner. | No direct guidance banner in Legacy. |
| 8 | Removed the duplicate-card banner from DrawReveal. | No guidance banner in the reveal popup; reveal actions retained. |
| 9 | Removed the Shop gift banner. | No direct guidance banner in Shop. |
| 10 | Generated twelve transparent painted icons, one per CashEffect, and added them to Player and catalogue rows. | Static coverage check: all 1,000 upgrades map to an existing icon. Live loaded images and separate columns checked. |
| 11 | ProductDetail heading uses cobalt text on parchment. | Computed color #164b80; live screenshot confirms readable title. |
| 12 | Removed ProductDetail's large guide and added a small header info button. | Opens quantity guidance; closing returns to the product popup. |

## Files and future Unity owners

- `UI Builder/HTML/Classic/classic.js`: guidance routing, labels, upgrade icon mapping and runtime binding metadata. Native owners: `ClassicUI`, `ClassicGuidance`, `ClassicUI.Achievements` and related Editor authoring scripts.
- `UI Builder/HTML/Classic/parity/native.css`: square Settings target, header information controls, product-row bounds and upgrade-row columns. Native owners: saved Main Canvas and `ClassicPresentationAuthoring` / `BuildClassicPanels`.
- `UI Builder/HTML/Classic/popup-refresh.css`: catalogue upgrade columns. Native owners: saved upgrade catalogue rows and their Editor authoring.
- `UI Builder/HTML/Classic/popup-clearance.css`: ProductDetail title contrast and header info target. Native owners: saved ProductDetail popup and `ClassicPopupClearanceAuthoring` / `BuildClassicDialogs`.
- `UI Builder/HTML/Classic/index.html`: cache revisions and asset metadata.
- `UI Builder/HTML/Classic/assets/upgrades/`: twelve original PNG icons. Built-in `image_gen.imagegen` was used, with transparency requested. Exact prompts and the effect mapping are in `PROVENANCE.json`. No native import has been performed.

The existing Classic baseline snapshots remain unchanged. No claim is made that these HTML changes are already present in Unity.

## Final checks

All 50 responsive cases passed at 964×884, 360×540, 360×640, 390×844 and 450×916. Cases cover new and established shops, insufficient funds, Player, Products, Legacy, Shop, DrawReveal, ProductDetail and the upgrade catalogue. Assertions cover square Settings geometry, row containment, icon/text/button separation, hidden banners, info-button placement, title contrast, duplicate IDs, image loading and horizontal overflow. Recorded results: `2026-10-25-browser-comments-checks.json`. Reusable browser harness: `Tools/ClassicUiPreview/BrowserCommentsChecks.html` (temporarily serve under Classic, then remove the served copy).

The three information actions were also clicked in the live preview: Player opens Buy affordable upgrades; Products and ProductDetail open Choose your quantity. Product popup back/close behavior was checked. Static validation confirms 1,000 upgrades covered by the twelve existing generated PNGs; all twelve are RGBA PNGs. JavaScript syntax, embedded JSON and Git whitespace checks pass.

Screenshots: `2026-10-25-player-comments.png` and `2026-10-25-product-comments.png`.
