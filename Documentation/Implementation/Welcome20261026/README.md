# Reviewed welcome and tap-banner transfer — 26 October 2026

Implemented the user's reviewed HTML revisions in the existing production scenes. Bootstrap owns the live welcome and account-entry flow; Main owns the stall tap prompt. Existing account comparison, loading, error, legacy-save and recovery behavior is retained.

## Saved presentation

- Full-screen festival-night salesman background, responsive upper illustration region, painted cobalt/gold sign, separate editable gold title, and no tagline.
- Scalloped ivory lower panel with layered cobalt/gold trim and separate floral crest. Welcome, description, buttons, hint and both footer statements remain native editable controls/text.
- Google and guest use their existing BootstrapController callbacks. Close uses the existing guarded dismissal and reopen flow. Real Google availability remains platform dependent.
- Painted TAP TO EARN banner spans the shop/paper seam in Main. The original authoritative tap target includes its lower half; next-display/help rows leave clearance. Existing tap feedback and reduced-motion owner remain in use.
- Complete previous Bootstrap Canvas is saved inactive as `Bootstrap Canvas__LegacyDisabled_20261026`; the old tap invitation is also retained inactive. Original pre-task scenes and relevant source files were backed up under the task's temporary `RRWelcome20261026` directory.

All scene changes were authored through the project-routed AI Game Developer Editor integration. No scene YAML was edited. No runtime UI creation was added. Canvas reference resolutions remain Bootstrap 1080×2200 and Main 360×640.

## Source mapping

| Native art | Reviewed source |
|---|---|
| `festival-salesman.png` | `UI Builder/HTML/Classic/assets/welcome-festival-salesman.png` |
| `brand-sign.png` | `UI Builder/HTML/Classic/assets/welcome-brand-sign.png` |
| `tap-banner.png` | `UI Builder/HTML/Classic/assets/tap-to-earn-banner.png` |
| `panel-frame.png` | Inspected static `welcome-panel-frame.svg`, rasterized at 1320×1500 |
| `floral-accent.png` | Inspected static `welcome-floral-accent.svg`, rasterized at 180×120 |
| `button-primary.png`, `button-secondary.png` | Native-sized rendering of the reviewed HTML cream/cobalt/gold button geometry |
| `festival-upper.asset` | Sprite crop of the same selected background's upper 60%; preserves the salesman's visibility when vertical room is short |

Imported assets are under `Assets/Game/ClassicUI/Welcome20261026/`; hashes are in `AssetHashes.json`. Existing Card Serif, Heading, Body and Bold TMP assets are reused with saved material slots. The screen is not a flattened screenshot. Short-screen sign sizing uses an Editor-configured AspectRatioFitter. The existing BootstrapPopupSize layout input has an opt-in viewport fraction; its previous behavior remains the default for other authored uses.

## Verification

- `Validation.txt`: **22 passing focused checks**. Production Bootstrap/Main reload, absent tagline, dismiss/reopen identity retention, minimum touch heights at 360×540 / 360×640 / 390×844, complete footer copy, guest entry, banner visibility, lower-half tap target, exactly-once tap credit, drag rejection, modal blocking, settled native pulse, unchanged hierarchy/component identities and no runtime errors.
- `ProductionFlowValidation.txt`: **121 passing existing production checks**, covering guest/account/recovery transitions using fixture auth/cloud services and temporary profile roots.
- `EditModeAudit.txt`: saved fonts present, inactive legacy canvas, no active tagline, unchanged Canvas scalers, panel and close within simulated top/bottom safe insets.
- Final captures: `Welcome-360x540.png`, `Welcome-360x640.png`, `Welcome-390x844.png`, `Home-360x640.png`, `Home-390x844.png`.
- JavaScript package inspection passed for the active reviewed package. Historical `parity/before` and review galleries contain stale relative links; those were excluded from the task-owned inspection snapshot and preserved unchanged.

The native layout adapts the selected artwork on short screens to keep the salesman visible; this is a responsive composition adjustment rather than pixel-for-pixel stretching of the full portrait. Existing native press/tap feedback is reused; no timing-perfect browser animation claim is made. No live user profile, Firebase account, advertising delivery or physical-device authentication was exercised by these fixture tests. No Android build was requested or produced.

## Reproduction

The scoped Editor authoring/refinement methods are in `ClassicWelcome20261026Authoring.cs`; they operate only in Edit Mode and preserve the previous hierarchy. Do not rerun the initial migration on an already migrated scene. Focused checks can be started with `ClassicWelcome20261026Validation.Start()` from the Editor integration after saving the selected scenes.
