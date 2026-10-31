# Native Gouache Bazaar implementation

The production `Main` and `Bootstrap` scenes use native uGUI. The HTML reference and its rubric remain frozen. Unity keeps the existing command, save, service, CanvasScaler and safe-area owners.

## Shared system

- Source tokens: `UI Builder/HTML/Classic/system/tokens.json`.
- Presentation asset: `Assets/Game/ClassicUI/Bazaar/Gouache Bazaar.asset` carries the source hash, fonts, colors, shared surfaces and control artwork.
- Editor application: `ClassicBazaarAuthoring.Apply`, available under Rocket Raja → Classic → Apply qualified Gouache Bazaar UI. It checks the qualified HTML hashes before applying.
- Reusable assets: `Components/Surface.prefab`, `Folk action.prefab`, `Information card.prefab`, `Product row.prefab`, `Progress bar.prefab` and `Settings switch.prefab`. Scene instances preserve their existing gameplay owners. Template controls require Editor binding before use.
- Native artwork and slice metadata: [artwork.json](artwork.json) and [skin-manifest.json](skin-manifest.json). New primitives are derived from inspected SVG sources and shared tokens; painted artwork is reused.
- Complete prior scenes remain under `Assets/Game/ClassicUI/Bazaar/Legacy/`, with their roots inactive. They are excluded from the player build.

Main uses logical 360-unit layout values; Bootstrap's existing 1080-unit canvas uses the equivalent values multiplied by three. Typography roles, spacing, borders and geometry are baked by the Editor tool. Runtime code updates state, progress, animation and responsive constraints on preplaced objects.

## Behavior and coverage

The full stall remains visible on every management tab. Home scrolls on short viewports. Dialog headers and actions stay available while their body scrolls. Settings remains mounted behind child dialogs, retaining focus and scroll position. Primary, secondary, disabled, selected and destructive controls share the imported Folk artwork; informational cards, switches and icon controls retain their documented roles.

The coach grows with its live text. The fingertip pulse, purchase feedback, milestone effects, skill effects, card reveal, prestige bloom and two-source audio system remain connected to the existing successful commands. Rounded progress fills resize their sliced geometry rather than stretching their caps.

Checks use disposable profiles. The reports distinguish functional results from manual visual scores:

- [Scores and deductions](scores.json), [native design gate](quality-gate.json).
- [First complete audit](gate-a/index.html), [final complete audit](gate-b/index.html), including four viewports, scroll endpoints, safe insets, large balance and wide name.
- [Nested layers, focus and artwork](layers/dialog-layer-validation.txt).
- [Full tutorial journey](tutorial/PlayChecks.txt), including real saving failure and retry.
- [Feedback, audio and effect pools](feedback/checks.txt).
- [Startup behavior](startup/PlayChecks.txt), [startup edit checks](startup/EditChecks.txt), [startup visual checks](startup/VisualChecks.txt).
- [Android build report](android-build.txt).

The development APK built successfully with zero errors and 32 dependency/compiler/shader warnings. The artifact is `Builds/Android/RocketRaja-bazaar-development.apk`; its SHA-256 is recorded in [android-apk.json](android-apk.json). [Package inspection](android-package.txt) confirms the expected application ID, SDK levels, debuggable flag and Unity launch activity. The [build-profile restoration check](build-settings-restoration.txt) confirms the active profile was restored byte-for-byte.

## Remaining limits

Keeping the full stall requires more scrolling on the shortest device, especially with safe-area insets. Dense ledgers and catalogues retain this tradeoff. Extremely wide store names show an explicit ellipsis once the minimum size is reached; the full name remains editable in Naming. Original painted illustrations retain small variations in lighting and texture. Unity amounts and progression follow the existing engine, so fixtures can show different values from static HTML examples.

No Android device or emulator was available. APK compilation and package inspection can be recorded here; install-and-launch smoke validation remains pending until a device is available. No physical-device pass is claimed.

## Reproduction

Run `ClassicBazaarValidation.Start("gate-a", true)` and repeat as `gate-b` from compiled Edit Mode. `ClassicBazaarStartupValidation.CaptureAll()` captures loading, protected recovery and older-save review. `RunEditChecks()` and `StartPlayChecks()` validate seven isolated startup cases. `ClassicBazaarTutorialValidation.Start()` and `ClassicBazaarFeedbackValidation.Start()` run the separate player journeys. Each runner restores its profile override.

`Tools/BazaarUi/native-gallery.py` publishes the captures. `freeze-native.py` validates the evidence and records the explicitly manual design assessment. `ClassicBazaarAndroidBuild.Start()` builds the saved production scenes without regenerating the UI.
