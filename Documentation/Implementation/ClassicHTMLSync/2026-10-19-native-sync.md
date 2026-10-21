# Classic HTML → native Unity transfer

The user authorized replacement of the existing production presentation on 19 October 2026, covering H2U-001–H2U-013. This supersedes the earlier HTML-only stage boundary. The original baseline archive and manifests remain unchanged.

## Owners and saved assets

- `Assets/Scenes/Main.unity`: the active `ClassicProductionCanvas`, five existing panels and 29 saved dialog routes. The complete prior canvas remains inactive as `ClassicProductionCanvas__LegacyDisabled_20261019`.
- `Assets/Scenes/Bootstrap.unity`: the actual startup/account presenter, its responsive popup, outside dismissal and preplaced reopening control. The prior canvas remains inactive as `Bootstrap Canvas__LegacyDisabled_20261019`.
- `Assets/Game/Editor/Classic/ClassicHTMLSyncAuthoring.cs`: scoped Editor migration and refinement. It does not run during player initialization. Scene changes were made through the project's routed Unity Editor integration, never by editing scene YAML.
- `Assets/Game/Runtime/Classic/ClassicUI.HTMLSync.cs`: bindings for accepted-tap earnings, native skill deadlines, state rings, hero/list summaries, statistics columns and focus restoration. `ClassicControl.htmlStyle` preserves the shared popup skin when interactability changes.
- `Assets/Game/Runtime/Cloud/BootstrapController.HTMLSync.cs` and `BootstrapPopupDismiss.cs`: dismissal through the real account presenter. Closing initial Welcome reveals a saved reopening button; it never chooses or creates a profile. Loading remains guarded. Main's mandatory recovery choices remain guarded.
- `Assets/Game/ClassicUI/HTMLSync/`: eight byte-identical generated reference-art PNGs, the annular clock sprite and sliced native UI surfaces. All interactive text and controls remain separate uGUI/TMP objects.

The existing 360×640 width-matched Main scaler and 1080×2200 Bootstrap scaler are preserved. `BootstrapPopupSize` provides preferred-size input to the saved layout group without writing geometry; its painted branding/art and controls remain Editor-authored. Responsive anchors, layout groups, scroll views and the existing `ClassicLayout` remain responsible for sizing. The info-row fitter now considers both columns of the statistics ledger.

## Behavior contracts

Home has no visible stall invitation strip; its tap target remains. Automatic sales and tap payout are stacked at left with `/sec` and `/tap`; a native gold chart accompanies the combined rate at right. The combined rate adds successful manual-tap cash deltas from the preceding engine second to current product income with Festival Rush applied. It follows the HTML definition; other scheduled/instant rewards remain with the engine.

Home skills activate directly. The three list rows and selected hero share seven saved clock presentations. Gold drains during active duration; a grey track and radial fill represent remaining cooldown. Artwork and its disc grey during cooldown. Locked and unavailable states disable activation. These bind `RedesignState.now`, `activeUntil` and `readyAt`; no second gameplay timer was added.

Skills uses the selected stacked list. Individual skill details use the selected hero layout, one activation action and no upgrade-navigation footer. Player shortcuts retain their hit regions with no frames. All 30 Player/catalogue upgrade rows retain a ten-unit right gutter.

All popups use the revised shared presentation with generated artwork, independent text/controls, scrolling and title/close clearance. Settings has no bottom Back button. Statistics uses aligned label/value columns, appearance shows product variants and permanent decoration stages, and exact-price views read the current native quote magnitude. Outside dismissal uses the existing top-dialog close path, restores parent/opener focus and blocks underlying taps. Offline dismissal acknowledges the existing receipt without granting income again. Reveal dismissal retains the persisted result and restores Legacy focus.

Bootstrap continues to own Google sign-in, save selection, promotion, handoff, sign-out and account recovery. The browser's simulated service outcomes are not production actions. Store purchases remain unavailable until the existing store integration is ready.

## Verification

**Passed.** [Machine-readable results](2026-10-19-native-verification.json) record the scope and evidence locations.

- 82 connected production-fixture assertions passed with no runtime errors: real Bootstrap/Main navigation, fresh/guest/account paths, Welcome dismissal/reopening without save selection, real file-lock recovery, corrupted-profile preservation, reconnect, pause/resume, sign-out, inline purchases, persisted draw results and stable component identities.
- The existing atomic draw/persistence harness passed 66 checks.
- 72 base captures passed popup bounds/title and horizontal-overflow checks at 360×540, 390×844, 360×640 and 450×916. Focused rerenders cover the final guidance colors/form layout and all three hero variants; additional checks cover 44 px top / 34 px bottom simulated safe-area insets, long rates, invalid/valid names, active skill command guards and active/grey cooldown visuals.
- Native animated entry/exit settled correctly and restored gameplay input; reduced-motion entry/exit settled immediately. Backdrop raycasts selected the modal target and did not grant a stall tap. Receipt/reveal dismissal did not grant rewards twice.
- Reloaded production assets passed the final audit: 29 dialog routes, seven complete radial clocks, valid Main bindings/scaler, 22 saved Bootstrap text components and their slots, one persistent reopening listener and both inactive legacy roots.
- Editor/Play hierarchy and component signatures match after normalizing TMP's automatic renaming of its **preplaced** material slots. The connected fixture separately verifies actual component identities remain unchanged through recovery/navigation.
- All eight imported generated-art PNGs match their browser source bytes. All shared fonts, core economy, packages and project settings remain unchanged. Eleven existing native files changed; 539 files in the scoped native backup remain byte-identical.

The connected fixture was refreshed to explicitly choose ×1 for single-level assertions and to test the current core's corrupt/known-old economy policies. No gameplay policy was changed to satisfy an obsolete fixture assumption.

Evidence is retained at `C:/Users/Dev/.codex/tmp/classic-native-sync-20261019/`: `captures/`, `capture-results-72.json`, focused `capture-results.json`, `behaviors-final.json`, `inspection-final.json`, hierarchy signatures and `final-audit-result.json`. Connected results are also retained in [Classic-Production-Validation.txt](../Classic-Production-Validation.txt). Live Firebase, billing and player profiles were not used.

Unity is left in Edit Mode with Bootstrap restored. The disposable QA scene is absent and was never added to build settings.

The saved hierarchy has been checked for 29 unique dialog routes, seven radial clocks with tracks/discs, unique non-null text/image bindings, recovery dismissal guards, removed footers and all 30 purchase gutters. Native checks exercise accepted taps and expiry, all three skill phase transitions, locked/save-blocked input, nested focus restoration and direct backdrop events. The existing core harness passes 66 atomic draw/persistence checks.

The isolated UI review uses a saved copy of Main named `HTML Importer`, a temporary profile and the actual native engine. Production startup is removed from that copy before Play Mode. It is excluded from build settings and removed after review. Account-flow verification uses the existing Editor-only fixture adapters, never live Firebase or player profiles.

## Fidelity and scope

The painted game artwork, generated popup frame/icons and game font assets are reused. Native sliced surfaces and radial Filled Images approximate browser gradients, SVG strokes, paper grain and CSS shadows; this is not a claim of pixel-identical rasterization. The new UI is editable in the Editor and does not depend on HTML, a WebView or runtime UI factories.

Pre-mutation backups and source hashes are retained outside the repository at `C:/Users/Dev/.codex/tmp/classic-native-sync-20261019/backup/`, with `backup-manifest.json`. Capture evidence and raw checks are in the sibling task directory. Shared fonts, shared art, core economy, packages and project settings are outside the mutation scope.
