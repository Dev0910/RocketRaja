# Ivory Bazaar — detailed screen review and rollout

The user selected **A — Ivory Bazaar**, accepted the Player HTML pass, then explicitly requested a detailed review and rollout across the screen set. This report records local code, pixel and browser evidence. It does not claim Unity runtime verification or new per-screen reference generation.

## Shared findings and fixes

1. Products and Legacy retained teal/brown serif headings while Player used blue Outfit typography. Both now share the ivory header, marigold flowers, double blue rule, compact chevron and lotus scroll footer.
2. Dialogs retained burgundy headers, gold outlines, serif titles and brown copy. All nine dialog routes now use ivory paper, cobalt headings, fine blue cards and consistent footer rules. Long titles wrap without displacing close controls.
3. Usable secondary buttons had the same sand treatment as disabled actions. Enabled secondary actions and OFF switches now use ivory faces with blue outlines/text; unavailable actions remain muted sand. Primary and prestige actions retain the selected cobalt/raspberry Folk print system.
4. Legacy's reputation summary occupied excessive space on short phones. Its compact number, text and sprig ornaments leave more room for the perk list. All twelve perks remain present.
5. Skill readiness and active chips originally shared one appearance. Ready, active and cooldown/locked states now have distinct green, blue and sand treatments; text remains the primary state cue.
6. Standalone Home referenced its paper texture relative to the shared stylesheet, producing missing-resource errors. A shared, valid paper URL fixes the path for both Home and the full screen set.
7. The gallery still advertised the older Gouache finish and cropped its previews. It now presents the 13 screens as Ivory Bazaar, with complete phone thumbnails and links to the shared Home route and existing review controls.
8. The disabled-button drop shadow was also applied to the Home skill labels, creating a second outline under the text. Skill controls now keep the depth on their medallions while labels remain crisp.
9. The reduced-motion switch now suppresses the button transitions introduced with Folk print. Native game settings and persistence are untouched.

## Screen-by-screen review

| Screen | Changes and review focus | Result |
| --- | --- | --- |
| Home | Ivory dashboard cards, blue live text, coordinated skill medallions, retained cooldown mask, readable active sponsor state. Reviewed both integrated Home and the earlier standalone study. | Shared Home retains its exact stall geometry across tab switches; standalone resource and interaction checks pass. |
| Player | Retained approved ivory header, icon toolbar, floral card details, quantity selector and ten upcoming unlockables. Rechecked after applying shared styles. | Selection, quote labels, ten-row filtering, scrolling and compact touch controls preserved. |
| Products | Matching header/footer, blue sales hierarchy and detailed product cards; bulk selector retains all four choices. | All eight families remain reachable; last product and its disabled quote are readable. Detail route and focus restoration pass. |
| Legacy | Smaller reputation card with gold sprigs; blue titles, compact perk rows, readable cost/actions. | All twelve perks, mastered state and unaffordable state preserved; final perk reachable on 360×540. |
| Statistics | Outfit section headings, double rules, lighter data rows and aligned bold values. | All six statistics readable; footer remains accessible on short phones. |
| Skills | Consistent framed rows, smaller illustration area, clear rank/effect/cooldown hierarchy and semantic status chips. | All three skills and final guidance reachable; guide remains read-only. |
| Product Detail | Ivory cards, cobalt progress bar and blue milestone labels. | Product art stays correctly cropped; all display stages and final explanatory card reachable. |
| Prestige | Shared header/cards, reset and keep colors retained for meaning, clear locked versus actionable confirmation. | Locked, ready and stale quote fixtures pass; confirmation remains accessible. |
| Settings | Compact framed setting rows, blue ON and ivory OFF switches, usable privacy controls visibly differentiated from disabled buttons. | Toggles remain independent; conditional consent row reachable. |
| Offline | Framed reward hero, subtle gold sprig, clear already-credited text and optional-video action hierarchy. | Unavailable/cancelled/failed/consumed states retain exit access and grant no preview reward. |
| Sponsor | Matching reward hero and optional-video explanation with cobalt and ivory actions. | Active and unavailable states remain clear; no rewarded callback is simulated. |
| Privacy | Ivory information cards, cobalt titles and clear Settings return. | Conditional advertising choices reachable; nested and direct return checks pass. |
| Save Recovery | Raspberry attention heading and fresh-game action, readable preservation copy and distinct keep-files exit. | Blocking behavior retained; no close-to-play affordance added and no files modified. |
| Gallery | Ivory paper, selected reference typography, floral details, blue framed links and uncropped screen previews. | Thirteen screen cards; all preview images load. |

## Verification evidence

- `HTML/verification.json`: **79 viewport/state checks and 17 interaction checks**, no browser errors. Includes 48 layout/metadata passes at 1080×2200, 360×640, 390×844 and 360×540, plus 31 additional state captures.
- `HTML/ivory-review-verification.json`: **21 additional review checks**. Exact shared stall height across Home/Player/Products/Legacy at five sizes (including 450×916); every scrolling area reaches its last content; no horizontal clipping of headings, paragraphs, buttons or ranks; dialog footers remain on-screen; Player quantities and ten-row limit; conditional consent navigation; ready prestige; gallery image loading.
- `HTML/Home/renders/verification.json`: **11 captures and 11 standalone Home interaction checks**, no browser errors.
- Pixel inspection covered every canonical screen at short-phone size, representative 450×916 views including Home, Player and Product Detail, and targeted bottom-of-list, unavailable-reward and conditional-settings captures.
- Representative current captures: each screen's `renders/ivory-bazaar-450x916.png`; short-screen bottom views in `renders/ivory-bottom-360x540.png`; integrated Home in `Home/renders/shared-home-450x916.png`.

## Current sources and boundaries

- Shared rollout: `HTML/Shared/ivory-bazaar.css`.
- Approved Player-specific detail: `HTML/Shared/player-folk.css`.
- Existing Folk print buttons: `HTML/Shared/folk-buttons.css`; new enabled-secondary face: `HTML/Shared/folk/ivory.svg`.
- Editable ornaments and card frame: `HTML/Shared/ivory/`.
- Gallery generator: `build-gallery.cjs`; current documentation generator: `document-ivory.cjs`.
- The gallery's Home opens `Player/player.html?screen=Home`, using the full common panel/dialog navigation. `Home/home.html` remains the earlier standalone Home study with separate frozen values and route notices; it receives the same style and retains its existing behavior.
- Gameplay values remain review fixtures. Native purchasing, ads, saves and Unity presentation integration are separate work. No Unity source, asset or save files changed.

No material visual or browser-interaction blocker remains from this rollout review.
