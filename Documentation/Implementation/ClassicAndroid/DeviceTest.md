# Android build and installation attempt — 6 October 2026

Status: **APK built, installed and launched; manual device interactions and card animation observed.** Some interaction/lifecycle checks remain unverified, as listed below.

## Build evidence

- Scene: `Assets/Scenes/HTML Importer.unity`, saved Classic UI and animation bindings.
- Unity 6000.4.9f1, Android ARM64, IL2CPP, Development, build-only `CLASSIC_DEVICE_PREVIEW` define.
- Result: succeeded, **0 errors**, 30 warnings, 6 minutes 8 seconds. See `Build.txt` and `BuildMessages.txt`.
- Warnings concern existing package assembly definitions, obsolete Unity API calls/reference comparisons in other runtime code, ProBuilder shaders, and the compressed application icon. They were not suppressed.
- APK: `Builds/Android/RocketRaja-classic-playtest.apk`, **93,892,499 bytes** (89.5 MiB). Unity's larger reported total build size includes other build outputs and is not the APK download size.
- SHA-256: `DC6747C847ACB6D84D13C2B4E4D8C3BA970805266CAA15BEDEE62FB554735681`.
- `aapt dump badging`: `com.diwali.rocketrajaclassic.playtest`, Rocket Raja Classic, version 0.1.1 (2), debuggable, `arm64-v8a`, minimum SDK 25, target SDK 36.
- Launcher: `com.unity3d.player.UnityPlayerGameActivity`.
- `apksigner verify --verbose`: verified successfully with APK Signature Scheme v2, one signer.

## Initial installation attempt

ADB detected a POCO X2 running Android 11, ARM64, 1080 × 2400. The phone was locked with its display off. A normal `adb install -r` attempt returned:

```text
INSTALL_FAILED_USER_RESTRICTED: Install canceled by user
```

The Classic package was absent afterwards. The existing `com.diwali.fireworkstycoon.dev` package remains installed. No uninstall, data clearing, keyguard bypass, or installation-policy change was attempted. The user was asked to unlock the phone and accept USB installation before a retry.

No device execution result was claimed from this initial attempt. The successful retry is recorded below.

## Successful retry and manual device review

The user unlocked the POCO X2 and requested another attempt. Normal `adb install -r` succeeded, followed by a cold launch of `UnityPlayerGameActivity` (Android reported 2,235 ms). Automated taps were rejected with `INJECT_EVENTS` permission errors; after the user selected manual testing, no further injected input was used. The user operated the phone while read-only telemetry and app-only screenshots were collected.

205 observed snapshots cover Home, Player, Products, Legacy and Shop, plus product details, card details, prestige, statistics, purchase preview and draw reveal. Product levels increased, tap training advanced from level 12 to 87, and owned upgrades increased from 3 to 41. These are observed manual purchases, not an assertion that every individual purchase path or Buy All edge case was separately tested.

- Free 5x pull: four free pulls consumed, five card levels granted, 400 KK retained.
- Paid 5x pull: 400 KK consumed, five additional card levels granted. The reveal was subsequently dismissed with no pending result.
- Native reveal rotation observed at approximately -11°, 135°, 181°, 187° and 189°, with Continue disabled while turning. Settled front samples have angle 0°, Continue enabled and 22 active celebration particles.
- All snapshots retained 2,352 UI objects and 8,652 components. The development probe reported zero Unity runtime errors throughout the observed sequence.
- Screenshots confirm the native product layout, collection, full card, reveal, and dialogs rendered on the 1080 × 2400 display with the 95-pixel top inset respected.
- Diagnostic interval FPS median was 59.7, with samples ranging from 34.2 to 60.9. This includes telemetry/screenshot overhead and transitions; it is not a production performance benchmark. Platform logcat contains vendor/resource/Swappy diagnostics and low-memory callbacks, with no observed application crash or managed gameplay exception. It is not an error-free system log.

The user then switched attention to Editor startup. Home counter taps (the count stayed zero), drag rejection, reduced motion on the phone, interruption/early-close behavior, and force-stop/relaunch persistence were not completed in this manual session. Their previous Editor evidence remains separate. No device save was cleared or reset to obtain these results.

Evidence: `C:/Users/Dev/.codex/visualizations/2026/10/04/01a10668-9c90-7f90-b270-d6094bea89ec/Classic-Android-20261006/`, including `manual-observations.jsonl`, `manual-summary.json`, app screenshots and `startup-logcat.txt`.

## Workspace verification

After the build, the importer was restored in Edit Mode with Home selected: 2,352 objects, 499 text labels, 27 dialogs, no missing scripts/fonts/material slots, and a clean scene. The only scene change in this build pass is the saved read-only development probe component and its UI reference.

Original app identity (`com.diwali.fireworkstycoon.dev` / Rocket Raja), custom-signing selection, and APK mode were restored. The active build profile needed an explicit `SaveAssetIfDirty` after restoring its in-memory PlayerSettings; its bytes then matched the pre-build baseline. The resolver's temporary package-ID cache was restored from that same baseline. The builder now performs both restoration steps automatically. The Editor-only restoration change does not alter this APK.

Main, Bootstrap, the shared TMP fallback, scene/meta GUIDs, and production EditorBuildSettings are protected by pre-build hashes. Baseline backups remain outside the project in the task's temporary Android folder.
