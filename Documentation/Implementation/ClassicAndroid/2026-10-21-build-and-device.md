# Classic Android build and POCO X2 test — 21 October 2026

The user requested an Android build and physical-phone test after the popup, Shop and shared-chevron revisions. H2U-015–016 were transferred to saved native Main before building; the explicitly HTML-only Settings switches were not transferred.

## Build and installation

- APK: `Builds/Android/RocketRaja-classic-connected.apk`, 98,933,820 bytes.
- SHA-256: `FF388DEB71120D11EB86F5342217B4B41C726E850AA0265FB9DD07E2A38E53CB`.
- Unity 6000.4.9f1; ARM64 IL2CPP Development build; saved Bootstrap → Main production startup. No preview define, boosted profile, preview driver or device probe.
- Result: **Succeeded, 0 errors, 30 warnings**, 11m27.383s. The build's reported total output size includes other outputs; the APK size is listed above.
- Application: `com.diwali.fireworkstycoon.dev`, version 0.1.3 (2), min SDK 25, target SDK 36.
- APK signature and 16 KB ZIP alignment checks pass. Certificate SHA-256 matches the previously installed app: `5b133f146943597b93688744cec649130db2599ec09a99be07f58da90e803286`.
- `adb install -r` returned **Success** on the connected POCO X2 (Android 11, ARM64, 1080×2400). The existing package was updated without uninstalling or clearing app data.
- Build details: `Documentation/Implementation/Classic-Connected-Android-Build.txt`.

## Verification

The disposable native Editor check at 1080×2400 passed all four collapse callbacks, saved chevron Images, 48×48 touch targets, three Shop removals and stable component count. The QA scene was removed before the production build. Native and browser evidence remain distinct.

The user unlocked the phone and left the app open. Full-resolution 1080×2400 device captures show the live Welcome back popup and existing high-balance progress; its close button is visibly inset from the painted border. The first return reported the already-saved offline earnings from the existing profile. The application continued rendering after restart and resume.

Physical-device checks completed:

- Cold launch after force-stop: **PASS**, a new app process started (15534 → 22424). Android reported `LaunchState: COLD`, `Status: ok`, activity launch time 1,646 ms. This is Android's activity timing, not a benchmark of Unity's fully rendered startup.
- Persisted progress after restart: **PASS for the observed balance/profile**, which remained at its existing large value and continued advancing; it did not fall back to a fresh profile.
- Background/resume: **PASS**. Confirmed the launcher held window focus, then brought the same app process back. Android reported `LaunchState: HOT`, `Status: ok`, activity time 120 ms. The app rendered the same popup after resume.
- Captured app logs before restart, after restart and after resume contained zero matching Unity error, fatal-exception, fatal-signal, NullReferenceException or MissingReferenceException lines. This covers the observed session only.
- Four chevrons, Shop cleanup and 48×48 targets: **PASS in isolated native Editor verification; phone tap-through checks pending**.

One attempted phone tap was rejected with Android's `INJECT_EVENTS` SecurityException; it did not dismiss the popup. The user was offered a manual sequence and replied **“I can’t do them now.”** Phone navigation, chevron actions, Skills/Statistics interaction and popup dismissal therefore remain unverified on the physical device. The existing native Editor evidence is not presented as a substitute for those pending phone interaction checks.

The saved production UI was rechecked after the build, the disposable QA scene is absent, and the Editor was restored to saved Bootstrap. Shared TMP font assets, Bootstrap, ProjectSettings and EditorBuildSettings match this task's baseline hashes.

Evidence and protected baseline: `C:/Users/Dev/.codex/tmp/android-phone-build-20261021/`, including installed-before APK, signing/manifest/alignment checks, installation output, native Editor screenshot, `phone-open.png`, `phone-restarted-ui.png`, `phone-resumed-ui.png`, lifecycle reports and app logcat captures.
