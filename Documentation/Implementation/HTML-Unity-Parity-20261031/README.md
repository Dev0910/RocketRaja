# Current Unity → HTML parity, 31 October 2026

The user's current saved Unity UI is the visual reference. This revision cleans up the existing editable Classic HTML; it does not redesign the game or modify Unity.

[Open the 1080×2200 preview](http://127.0.0.1:8772/Classic/reviews/device-1080x2200.html). Fit changes only the review iframe's displayed size. Inside it, the game uses the actual 1080×2200 viewport and a uniformly scaled 360-unit canvas.

## Changes

- Replaced partial dimension scaling with one shared canvas scale for text, artwork, spacing, hit regions and effects.
- Matched native stall, header, goal/help controls, income, skill controls, navigation, panel headings and list rows.
- Reused native button faces with the correct inward slice dimensions and copied current note, row, chevron and ProductDetail sprites.
- Matched native popup sizes, title weights, content/footer regions, Settings order, Skills/details, Appearance, guidance and statistics.
- Kept Settings mounted behind child popups with its scroll position and focus preserved.
- Normalized particle, tap, coin and tutorial pointer coordinates. Composed Tutorial shares the cleaned Classic presentation and retains its earning/purchase flow.

The authoritative presentation files are `unity-parity.css`, `unity-reference.js`, `unity-presentation.js` and `viewport.js` under `UI Builder/HTML/Classic/`. The reference file contains exported native presentation measurements, not gameplay logic. Old reference galleries and archives are unchanged.

## Evidence

Native snapshots use a disposable local QA profile with a frozen progressed fixture, never a live save or account. Common fixture: product levels 25/18/12/8/5/3/1/1, Salesmanship 12, cash 1.24M, one prestige and three free credits. Captures compare the same fixture; skill/achievement readiness uses actual exported native values. `native-bounds/` contains independently captured geometry/text/image data from the existing Unity hierarchy at 1080×2200. Screenshots are verification artifacts, not rendered UI components.

| Screen | Saved Unity | Cleaned HTML |
| --- | --- | --- |
| Home | [native](native-Home-1080x2200.png) | [HTML](html-Home-1080x2200.png) |
| Settings | [native](native-Settings-1080x2200.png) | [HTML](html-Settings-1080x2200.png) |
| Product detail | [native](native-ProductDetail-0-1080x2200.png) | [HTML](html-ProductDetail-0-1080x2200.png) |

Additional HTML captures cover Player, Products, Legacy, Skills, SkillDetail, Naming, Statistics, Help and Achievements. Reduced motion is enabled for settled comparison frames. Browser font antialiasing differs from Unity TMP, and built-in beveled circles are approximated with CSS; this is not a pixel-identical raster claim.

## Verification

`checks.json`: **1,564 passing checks, 128 route/viewport states**, no script or HTTP errors. Checks cover 32 routes at 1080×2200, 390×844, 360×640 and 360×540, independently measured native popup bounds, viewport fit, local assets, notification markers, retained Settings/scroll, naming errors/save, catalogue filtering, product scrolling, scaled effects and drag guards.

Run with the existing preview server on port 8772:

```powershell
node Tools/ClassicUiPreview/UnityParityChecks.cjs <path-to-playwright-package>
node 'UI Builder/HTML/Tutorial/verify-classic.cjs' <path-to-playwright-package>
```

The existing composed Tutorial suite passes 103 checks covering nine states, four phone sizes, earning thresholds, explicit training/stock/upgrade purchases, achievement claim guards, prestige confirmation, free draw/reveal, skipping and save-error guards. See `tutorial-checks.txt`.

`device-tutorial-checks.json` additionally verifies the live review iframe and canvas are exactly 1080×2200, and checks nine tutorial fingertip alignments at 1080×2200, 390×844 and 360×540.

Main and Bootstrap scene SHA-256 values are unchanged; see `unity-readonly.txt`. Play Mode was stopped and the previous profile override restored. No Unity build or physical-device run was needed for this HTML change.
