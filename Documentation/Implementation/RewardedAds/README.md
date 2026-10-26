# Rewarded ads restored — 26 October 2026

This supersedes the historical ad-retirement notes for the current Classic production game. The legacy `GameController` stays retired; Classic's Offline **Watch ad for 2× rewards** now uses a Google Mobile Ads provider.

## Configuration

- Google Mobile Ads Unity **11.5.0**, embedded at `Packages/com.google.ads.mobile`. Version 11.6 introduces Unity's separate dependency-manager package, so 11.5 retains this project's existing embedded EDM4U **1.2.189** and Firebase **13.17.0**. Registry downloads hit Windows PackageCache rename errors; the unchanged 11.5 package archive was embedded instead. Unity performed its normal binary API updating on import.
- Android App ID: `ca-app-pub-3595621361914402~9466422249`.
- Android rewarded unit: `ca-app-pub-3595621361914402/1364570948`.
- Runtime resource: `Assets/Resources/RewardedAdsSettings.asset`.
- SDK manifest settings: `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset`.
- `useTestAds` is false for release. Editor and development builds always select Google's platform-specific test rewarded unit. No live ads were clicked during validation.
- iOS production IDs have not been supplied. Release builds with live ads require valid IDs for the selected platform.

The existing Android Firebase configuration, application identity, signing settings and automatic Analytics/Crashlytics collection settings are retained. AdMob's UMP privacy messages must be configured in the supplied AdMob app; the SDK cannot publish them from this project.

AdMob's optional Gradle build preprocessor is disabled in its saved settings: it unconditionally force-resolves dependencies inside BuildPlayer and reproduced EDM4U's previously documented Windows error 1224 while writing a generated Firebase POM. The project already has the required Gradle templates and min API 25. The build wrapper resolves before BuildPlayer, invokes the project's existing generated-POM repair, verifies generated Firebase library hashes/packaging, and requires zero build errors. The SDK manifest processor still writes the supplied App ID normally. The intermediate APK with a resolver error was not accepted; its report is retained as `AndroidBuild-ForcedResolverFailure.txt`.

## Runtime behavior

`AdMobRewardedAds` refreshes consent information and displays UMP forms if required before initializing/loading ads. It preloads one rewarded ad, destroys consumed/expired ads, retries failed loads with a bounded 5–120 second delay and marshals SDK callbacks to Unity's main thread. Settings exposes **Ad privacy choices** when UMP requires it; changing choices discards the old loaded ad before reloading.

The Watch action is enabled only when a loaded ad and mutable session are available. The Google SDK's earned-reward callback, followed by ad close, authorizes the existing receipt-specific `RewardOfflineAd` command. Duplicate callbacks, skipped/failed ads, changed receipts and changed sessions cannot grant an extra reward. Base offline earnings remain saved independently.

Native ad/consent presentations save before opening, preserve the pending offline receipt over pause/resume, and freeze foreground gameplay ticking. Presentation time settles as ordinary offline income without replacing the frozen reward being doubled. A Google account renews its **same owner/generation lease** before resuming; it cannot take ownership from another device. Lost ownership or failed persistence prevents reward mutation and enters the existing recovery flow.

The SDK can create its own consent/ad surfaces. All game-owned UI, including the Settings entry, remains Editor-authored. Isolated game-validation fixtures bypass automatic SDK startup; the dedicated SDK integration check explicitly attaches the Google adapter.

## Evidence and remaining device check

- Core suite: **864 checks passed**, including receipt preservation over a native presentation and same-lease account renewal/rejection after device takeover.
- `Validation.txt`: **12 Google SDK Editor integration checks passed**, including actual SDK placeholder loading/show/completion, Android-pause simulation, durable bonus, duplicate rejection and privacy-entry visibility/layout. The screenshot named `Google-SDK-editor-test-ad.png` is a Google SDK Editor placeholder, not live network delivery.
- The existing offline regression suite covers cancellation, failure, duplicate/late callbacks, unavailable providers and responsive layouts separately.
- No Android device was connected when integration began. Real Google network loading, consent forms, playback and lifecycle behavior must still be verified on a connected device using the development test APK. Editor success does not establish production ad fill.

Final Android build: **Succeeded, 0 errors, 23 warnings**, 1m09.779s. APK: `Builds/Android/RocketRaja-classic-rewarded-test.apk`, 119,730,250 bytes; package `com.diwali.fireworkstycoon.dev`, version 0.1.5 (5), ARM64, min API 25 / target API 36. `ApkChecks.txt` records the packaged supplied App ID, valid debug signature and SHA-256 `355845E529216E352DE1507DD5886E2B7ADA33FC378F9DB7078C2A0D250037FE`.

The configured release signing passwords were not loaded, so this test APK uses Android debug signing. It cannot update the existing release-signed installation; do not uninstall that app or clear its progress to work around the mismatch. Enter the original keystore/key-alias passwords in Unity before building an update with the original signature. The build wrapper restored the existing custom signing setting after building; no signing key or password was changed. No device was connected in the final ADB check, and this APK was not installed.

Official implementation references: [Google Unity setup](https://developers.google.com/admob/unity/quick-start), [rewarded ads](https://developers.google.com/admob/unity/rewarded), [UMP consent](https://developers.google.com/admob/unity/privacy), and [test units](https://developers.google.com/admob/unity/test-ads).
