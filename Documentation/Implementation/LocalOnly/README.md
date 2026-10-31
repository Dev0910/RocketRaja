# Local-only startup — 28 October 2026

Google login is removed from the playable game. Every launch shows the approved festival-stall loading art and enters the local game automatically. Settings has no Account section; account dialogs and cloud-login recovery choices are removed. Loading completes only when Main has a playable local session; failed local loading reveals existing protected recovery without falsely completing the bar.

## Save and dependency boundaries

- Production startup ignores stored account-mode preferences and never constructs Firebase authentication or Firestore account sessions. Both session hosts reject live account entry.
- Existing local save paths, backups, version checks, offline earnings and storage-failure protections are retained. Account checkpoints/cloud records are not read, migrated, merged or deleted. Known economy-transition rules in the existing save store are unchanged.
- Native Google sign-in bridges, FirebaseAccountAuth, the provider-specific bridge interface, linker entry and GoogleIdentityDependencies declarations are removed. EDM4U regenerated the resolved Gradle declarations without the game's direct Credential Manager 1.6.0 / Google ID 1.2.0 dependencies. Firebase Auth 24.2.0 depends on Credential Manager 1.2.0-rc01, whose Play services implementation depends on Google ID 1.1.0. Those transitive libraries remain with the explicitly retained SDK; see `AndroidDependencyGraph.txt`. They provide no production login entry point.
- Firebase SDK 13.17.0, real configuration, rewarded ads and shared resolver remain. Firebase-console/OAuth/backend state is unchanged. Cloud contracts, stores, account coordinators, premium authority and isolated core tests remain for future integration. Editor fixture injection cannot be reached in players.
- Store purchases remain unavailable. No replacement provider, SDK upgrade, backend deployment or publication was performed.

## Reviewed artwork and native ownership

The user approved wireframe r1, the generated illustration, and the HTML preview in sequence, then explicitly directed Unity reconstruction. Source: `UI Builder/HTML/Loading/`; approval/provenance notes are beside it. The built-in image_gen prompt and selected reference paths are recorded there. Approved art: `Assets/Game/ClassicUI/Loading/festival-stall-r1.png`.

`LocalOnlyPresentationAuthoring` authors the new saved UI and removes account controls from Main. Text, loading bar, recovery choices, geometry and TMP material slots exist before Play Mode. Runtime code changes data/visibility and progress only; it creates no loading-screen UI. The replaced welcome presentation is retained inactive for Editor reference, and legacy canvas snapshots are stripped from player builds. Build processing enforces account removal.

## Verification

- `CoreChecks.txt`: existing .NET core suite, including save durability, lifecycle/offline settlement, simulated native-ad callbacks, and retained account/premium logic.
- `EditChecks.txt`: saved-scene account cleanup, protected saves, dependency removal and retained service packages.
- `LockedSaveRecoveryChecks.txt`: seven checks using the current driver, including an actual file lock, blocked gameplay, retry, eight-hour offline cap and once-only durable settlement. The older validation fixture was updated to use the driver directly instead of an obsolete UI prefab.
- `PlayChecks.txt`: seven isolated automatic startup scenarios: fresh, existing progress, old account mode with untouched account checkpoint, corrupt save, future version, recovered backup and restart. Editor-only profile override keeps real saves untouched; ad services are excluded from these tests.
- `VisualChecks.txt` and `Renders/`: native loading/recovery/legacy presentation at 1080×2200, 390×844, 360×640 and 360×540; live-text overflow checks.
- `SafeAreaCheck.txt`: loading presentation with simulated notched safe-area insets at 390×844.
- `BrowserChecks.txt`: existing Classic Settings/recovery/privacy routes and JavaScript health after account removal.
- `RewardedAdSmoke.txt`: five checks against saved Main with a simulated rewarded provider: cancellation, exact bonus, duplicate callback rejection and durable local reward. Live ad services are excluded.
- `AndroidBuild.txt`: local-only debug-signed development validation APK, retaining the existing package and restoring the project's signing/bundle settings afterward. The first attempt with configured release signing stopped because its keystore password was unavailable; see `AndroidBuild-SigningUnavailable.txt`. `AndroidPackageChecks.json` records package/dependency inspection. No release-signed build is claimed.

No Android device was attached (`adb devices -l` returned an empty list). Airplane-mode behavior is covered by the network-independent startup design and isolated local tests; physical-device startup and a live rewarded-ad presentation are not claimed. Existing rewarded-ad callback checks use simulated provider results, not production ads.

Completed Android build: `Builds/Android/RocketRaja-local-only-validation.apk`, debug signed, package `com.diwali.fireworkstycoon.dev`, version 0.1.5 (6). Build succeeded with zero errors and 31 warnings. APK inspection verifies the game sign-in bridge is absent, direct login declarations are removed, retained SDK dependencies are explained by the Gradle graph, and rewarded ads plus the arm64 player remain packaged. Project signing settings were restored. No APK was installed or published.

## Restoring login later

See `Documentation/CloudSaves.md`. Retained cloud settings are not an activation switch. Restore a reviewed provider adapter and explicit host construction path, then review save selection/migration, purchase identity, native dependencies and account UI before enabling them.
