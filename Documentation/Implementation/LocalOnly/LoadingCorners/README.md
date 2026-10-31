# Loading bar rounded corners — 31 October 2026

Fixed the active Bootstrap loading bar, `Bootstrap Canvas/Local Loading Presentation/Loading Safe Area/Loading Status/Loading Track`. Its shared 32px sprite borders previously used a fixed 0.5 pixels-per-unit multiplier: uGUI compressed the vertical border to fit the thin bar while retaining wide horizontal caps, producing flattened ellipses.

`LoadingBarCorners` keeps the saved track, interior and fill Images sliced with a circular radius of half their shorter side. It responds to RectTransform dimension changes in both Edit and Play modes and creates no UI. A saved Fill Bounds RectTransform owns the existing padding; the same `BootstrapController.loadingFill` Image now scales exactly within those bounds, so very small progress values have positive proportional widths. The existing `SetLoading` logic, progress ownership, artwork, shared import settings and loading flow are unchanged. The Editor authoring helper applies the repair to future authored/rebuilt loading bars.

Fifteen [checks](checks.txt) passed by invoking the actual Bootstrap `SetLoading` method and rendering the saved UI through the Bootstrap camera at 390×844 and 360×540. Covered 0%, 0.5%, 1%, 25%, 58%, 99% and 100%, zero-fill visibility, exact proportional widths, circular corner scale and unchanged component counts. No rendering errors occurred. Captures were produced outside the project before copying the selected result here. No player profile was opened or changed; no device build was performed.

![Corrected loading bar at 58%](loading-390x844-58.png)
