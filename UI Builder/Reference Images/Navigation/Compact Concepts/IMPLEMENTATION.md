# Floating Tickets — solid ivory

## Latest revision: three blue buttons

The user subsequently requested three separate buttons using the supplied folk-ticket shape (`codex-clipboard-22c982ca-f0fc-4c71-8914-abb0cf2838db.png`), keeping the color blue. This supersedes the continuous outline, flower sprigs and raspberry selection described below. Each icon now sits in its own blue ticket; the active tab uses deeper blue and a gold inner outline. The compact height, solid ivory background and stable stall sizing remain. Checked all tab selections, collapse behavior, minimum touch targets and stall geometry at 360×540, 390×844 and 450×916. Capture: `HTML/Player/renders/blue-ticket-navigation-450x916.png`.

Selected reference: `C-floating-tickets-solid-ivory.png` (also supplied as `codex-clipboard-2e8173f2-9d9f-420e-b537-e16e98598284.png`). The user selected C, requested an opaque background, then explicitly directed implementation of this revision in HTML on 2026-09-18. HTML implementation remains a candidate for visual review.

The shared navigation now has an opaque ivory surface, cobalt ticket outline, small marigold sprigs and a raspberry selection ticket. Player, Products and Legacy keep their existing artwork and order. Visible labels are removed; accessible names, hover titles, keyboard focus and minimum 48 × 48 CSS-pixel targets remain. Home has no selected tab; pressing an active tab still collapses to Home.

Implementation: `HTML/Shared/floating-navigation.css` and `HTML/Shared/ivory/nav-*.svg`, loaded by `build-screens.cjs` for all twelve shared screen entries and by the standalone `HTML/Home/home.html`. The common navigation is 50 scaled design pixels high with a 52 CSS-pixel minimum. Reclaimed height increases lower-panel space and the gap between Home's sponsor and navigation, preserving the accepted stall geometry. Navigation is still part of the persistent shell beneath modal screens.

Verification: compared pre-change and post-change geometry at 360×540, 360×640, 390×844, 450×916 and 1080×2200; checked all three selected states, collapse to Home, accessible names, hidden visual labels and touch targets. At 450×916 the bar decreases from 83.75 to 62.5 pixels; the stall remains 521.5 pixels high. Results are in `Reviews/floating-navigation-verification.json`. Current Home and selected-tab captures are under each screen's `renders/floating-navigation-450x916.png`.

This is HTML presentation only. Existing fixture behavior and embedded Unity functionality contracts are preserved.
