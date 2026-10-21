# Google login activation — 21 October 2026

The user requested verification of the Firebase setup, activation of Google login, a new APK and testing on the connected POCO X2. Existing local progress, the legacy `user` collection, package and signing identity are retained.

## Firebase verification and deployment

- Firebase Console access works for `rocket-raja-4729c`; earlier permission failures are superseded by this live observation. Firebase CLI has no authorized account; console UI was used for the scoped deployment.
- Google sign-in is already enabled. No provider, credential, OAuth client or certificate registration was changed.
- Registered Android package: `com.diwali.fireworkstycoon.dev`; app ID: `1:259290147663:android:d754f7274e18330880f33a`. Both match the existing `Assets/google-services.json` and generated Android resources.
- Registered release certificate SHA-1: `BB:42:49:74:4A:4F:FA:4C:2A:C0:81:CA:53:B9:E4:EC:B1:AE:FC:28`; SHA-256: `5B:13:3F:14:69:43:59:7B:93:68:87:44:CE:C6:49:13:0D:B2:59:9E:C0:9A:99:BE:07:F5:8D:A9:0E:80:32:86`. These match the installed/previously built APK's signing identity. Existing debug fingerprints remain registered.
- The live Google provider Web client exactly matches the configured and generated `default_web_client_id`. No client secret was read, saved or changed; `google-services.json` was retained.
- The existing default Firestore database is in `asia-south1`. Its previously published rules allowed only `user/{uid}`. The two-line repository diff adds the explicit `user` / `redesignUser` allowlist; the existing UID/provider, payload, timestamp, revision and lease constraints remain identical. All other collections stay denied. No existing player document was modified through the console.
- Published the tested repository rules at **04:33 IST**, then reloaded the console and copied the complete published rule text. It exactly matches `firestore.rules` after line-ending/outer-whitespace normalization. The console reported successful publication.
- Added `redesignUser.save` indexing exemption with collection ascending, descending and array indexes disabled. Existing `user.save` and automatic collection-group defaults were preserved. No composite indexes exist or were added.
- App Check shows its unconfigured Get started page. No App Check setting or protection was disabled for this test.
- Local emulator: **14 tests passed, 0 failed** against `demo-rocket-raja`. Includes separate collection/legacy preservation, another UID and guest denial, denied enumeration/deletion, payload validation, leases and stale ownership. This is emulator evidence, separate from the live configuration verification.

## Unity activation and APK

Enabled `Assets/Resources/RedesignCloudSettings.asset` through an Undo-recorded, saved Editor operation. Verified expected package/project, Android target, existing custom signing, `MatchesApplication == true` and `RedesignSessionHost.AccountFlowAvailable == true`. No runtime code or UI layout changes were needed. The existing local Firebase SDK 13.17.0 was reused.

The connected production builder completed after content/configuration validation. It built saved Bootstrap → Main with ARM64 IL2CPP, Development options, the existing package and signing key, and no preview define or boosted profile.

- Result: **Succeeded, 0 errors, 22 warnings**, 1m27.455s.
- APK: `Builds/Android/RocketRaja-google-login.apk`, **99,524,654 bytes**; version 0.1.3 (2), min SDK 25, target SDK 36, ARM64.
- SHA-256: `E7301303F7F12E049107D9BA4CA7ED8A8B03EA2014EB2730FC8197EEECCB094D`.
- Signature verification and 16 KB ZIP native-library alignment passed. The actual APK certificate matches the live Firebase release fingerprints and existing installed app.
- `adb install -r` returned **Success**; package last update time: 04:36:31 IST. No uninstall or data clearing.
- The current guest save and backup hashes were identical before and immediately after installation, before launching the new APK.
- The Editor was verified compiled/idle and restored to saved Bootstrap. The persisted cloud flag remains enabled. `google-services.json` was not replaced.

## Phone acceptance

POCO X2 remains connected over ADB. The new application started as PID 5311. Captured startup logs contain zero matching Unity errors, fatal errors, permission-denied cloud errors or OAuth registration errors in the observed window. A full-resolution 1080×2400 capture shows existing progressed gameplay and the saved Products chevron. This establishes startup and retained visible progress, not Google authentication.

The previous session established that Android rejects automated taps with `INJECT_EVENTS`; no permission bypass, data clearing or account reset was used. The user completed the normal Settings/Google interaction and confirmed **“it is working.”**

Live verification:

- **Google login: PASS**, user confirmed on the installed POCO X2 APK. Native Firebase authentication state notifications also appeared in the phone log. No credentials or tokens were captured for replay.
- **Classic cloud saving: PASS**, independently observed in the live console. A new `redesignUser` document was created at **04:37:06 IST**, acquired generation 1, and advanced to **revision 13**, updated/heartbeat at **04:37:38 IST**. This verifies authenticated create/acquire/commit against the deployed rules; it is not merely an account-chooser or SDK-initialization result.
- **Legacy cloud preservation: PASS for the observed document metadata**. Rechecked `user` after the test: revision remains **678**, generation 1, and updated/heartbeat remain **6 October 2026, 19:56:38 IST**. No legacy-to-Classic cloud conversion was performed.
- **Process restart: PASS for cold activity startup**, PID 5311 → 8032. Android reported `Status: ok`, `LaunchState: COLD`, total activity time **1,057 ms**. Captured post-restart app logs contain zero matching runtime/fatal/OAuth-registration/cloud-permission errors. This is Android activity timing, not a complete Unity startup benchmark.
- The phone was back in another app by the post-restart capture check. That app was not captured. The restarted Google progress-choice UI and a second account-entry action were therefore not visually verified. No repeated automated taps or further manual sequence was requested after the user's success confirmation.

This delivery verifies one account on one phone. Cancellation, sign-out/account switching and two-device handoff were not retested in this activation session.

Evidence: `C:/Users/Dev/.codex/tmp/google-login-activation-20261021/`, including the previous APK, pre-activation config, published rules before/after, emulator output, console rules screenshot and index screenshot. No credentials or tokens are stored in this report.
