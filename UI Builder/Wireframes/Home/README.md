# Home wireframes · review 01

Start with **home-review-board.png** for the side-by-side comparison and interaction notes. The matching SVG is editable. Each standalone screen also has an editable SVG and a PNG preview.

## Deliverables

| File stem | Purpose |
|---|---|
| `home-new-shop` | 1080 × 1920: empty shelves, ₹0 cash, ₹1/tap, first product at ₹120, locked skills |
| `home-progressed-shop` | 1080 × 1920: populated shelves, illustrative income, next display milestone, ready/active/cooldown skills |
| `home-review-board` | Both reference screens with design and interaction annotations |
| `home-ratio-checks` | Short, reference, and tall screens compared at the same width |
| `*-1620`, `*-2340` | Both states at 1080 × 1620 and 1080 × 2340 |
| `layout-checks.json` | Reference-coordinate touch rectangles for all six layouts |

SVG text remains live text in Segoe UI with Arial/sans-serif fallbacks. All illustrations, icons, boxes and lines are vector placeholders. Product names and numbered shelf labels identify positions for this review; they are not a requirement to place these labels over the final product artwork. The progressed screen's figures and rank are illustrative, not a simulated save state. The new-shop cash, tap value and first product price follow the existing economy.

## Layout contract

Reference coordinates use 1080 × 1920. Dashed lines at y=24 and y=1896 indicate example safe-area bounds. Actual device safe insets must be respected during the later Unity stage.

| Region | Reference bounds (y) |
|---|---:|
| Safe header / cash | 24–284 |
| Persistent stall | 284–1054 |
| Milestone and purchase feedback | 1054–1172 |
| Income summary | 1172–1392 |
| Home-only skill controls | 1392–1608 |
| Optional sponsor | 1608–1752 |
| Navigation touch region | 1752–1896 |

Header plus stall occupy approximately 55% of the reference canvas. The visible navigation content occupies roughly 6%, with padding increasing its touch region to 144 reference pixels (7.5%). At a logical width of 360, 144 reference pixels correspond to 48 logical pixels. Settings, sponsor, skills and navigation all meet or exceed 48 × 48 at that mapping. Smaller logical widths will require maintaining a 48-pixel minimum during actual responsive implementation rather than uniformly shrinking every control.

Shorter/taller variants retain the lower controls' sizes and adjust the scene's height. The short variant compacts shelving and counter spacing; the tall variant adds scene space. Neither layout scrolls Home or hides a control. These are structural checks, not phone usability testing or final art scaling.

## Interaction annotations

- Only pointer taps on the stall scene earn income. Shelf placeholders are scene content, not purchase buttons. Drag gestures, navigation, settings, sponsor and skill controls do not produce scene taps.
- Settings opens a modal overlay. The modal consumes input so tapping through it cannot earn income. Its content will be wireframed separately.
- Player, Products and Legacy replace only the content below the stall. The header and stall remain visible. Home selects no navigation tab.
- Tapping the selected tab again, or the panel's collapse arrow, restores Home. Other panels retain their scroll position.
- Skills appear on Home only. Locked controls are disabled and belong to Player's unlock progression; ready controls activate their existing skills. Active/cooldown controls remain unavailable until their existing cooldown permits activation. Text communicates every state as well as the grayscale treatment.
- Festival Sponsor opens the existing optional-video confirmation when available. The progressed example shows its active countdown and unavailable state. A displayed reward is not granted by merely tapping or closing the overlay.
- The milestone strip displays the next purchase or appearance change. A brief purchase-feedback message replaces its text in the same rectangle; no content shifts.
- Automatic sales are most prominent. The boosted example separately names normal sales and the sponsor multiplier. Helping Hands is represented as automatic taps, not an additional product-sales multiplier.
- The sample 24-pixel safe bands and the `ILLUSTRATIVE VALUES` note are review annotations, not proposed player-facing UI elements.

## Verification and boundary

Reviewed both reference states and the adaptive portrait comparison. Corrected shelf-label/pack crowding and skill-circle/label spacing during visual review. Generation verifies minimum touch rectangles and text-anchor containment; output checks verify image dimensions, valid SVG XML, and grayscale pixels. These checks do not substitute for final device testing.

Only this wireframe directory is changed. No HTML, generated reference artwork, Unity scene, gameplay, economy, or save changes are part of this step. Home review comes next; later screens and each subsequent production step remain user-directed.

To regenerate these artifacts on this machine: `node "UI Builder/Wireframes/Home/build-wireframes.mjs"`. The script uses the installed Codex workspace Sharp dependency; update that dependency path if moving the generator to another machine.
