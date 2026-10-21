# Achievement and Talent Android update — 21 October 2026

Built and installed the requested update on the connected POCO X2 (ADB serial 31920616) using `adb install -r`. Installation returned Success; no uninstall or app-data clearing was performed.

- APK: `Builds/Android/RocketRaja-classic-connected.apk`, 102,186,782 bytes.
- SHA-256: `CFABA8AA1A2201BE86E38D5BCEDC01CFC303F1E0B527E9737DB2894CAAB4C2D1`.
- Unity build succeeded with 0 errors and 30 warnings, duration 4m40.821s. See `../Classic-Connected-Android-Build.txt`.
- Existing package `com.diwali.fireworkstycoon.dev`, version 0.1.3 (2), SDK 25–36; production Bootstrap → Main startup.
- APK signature verification and 16 KB ZIP alignment passed. Certificate SHA-256 matches the previous installed APK and existing Firebase registration: `5b133f146943597b93688744cec649130db2599ec09a99be07f58da90e803286`.
- Google configuration is enabled and matches the application. `RedesignSessionHost.AccountFlowAvailable` verified true before and after building. Existing Firebase configuration and custom signing were reused.
- Saved Main retains 5 staged achievement rows and 83 notification bindings. Talent draws no longer require prestige; currency and recovery guards remain.
- Core validation: all 822 checks passed before building. Native in-memory checks passed for first-run paid draws, valid card saves, and availability badges.
- Installed app was launched successfully. Google sign-in interaction has not been repeated for this update; the earlier Google-enabled build was verified by the user, as recorded in `2026-10-21-google-login.md`.

Previous installed APK retained at `%TEMP%/incremental-phone-update-20261021/installed-before.apk`.
