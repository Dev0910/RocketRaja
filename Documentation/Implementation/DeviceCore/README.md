# Android diagnostic — 18 on-device checks passed

2 October 2026. The user authorized phone installation and asked for a retry after Android initially rejected it. The rebuilt diagnostic installed successfully on the connected **POCO X2 / Android 11 / Adreno 618 / ARM64**. Its first launch passed **14 checks**, and force-stop/relaunch passed **4 checks**. Raw reports and source hashes are in [2026-10-02](2026-10-02/). The installed game's APK hash was identical before/after; its save data was not accessed.

Full-catalogue Buy All quotation averaged **5.2991 ms**, state validation **0.7296 ms**, JSON encoding **0.6031 ms**, decoding **1.6961 ms**, atomic disk save **15.8761 ms**. The saved full-run JSON was **24,541 bytes**. Twenty measured prestige/three-draw/manual-rebuild cycles averaged **7.4629 ms**, with **0 retained managed-byte delta after GC**. These are isolated development-app measurements, not final-game UI frame-time or scroll results.

The newer build completed with zero errors in 2m57.480s. Unity's reported total build bytes were 141,129,309; the actual compressed APK file was 18,696,723 bytes. Binary SHA256: `6D89ECD75123F7E2E4BF8B454CF0116C8138D56AF4AC0D2BCD742BA67763E360`. This source snapshot predates the 2 October development placeholder-shop changes; those have separate desktop/Unity evidence. Current diagnostic APK: `Temp/RedesignDeviceQA-20261002-005107-7545630d/Build/RocketRaja-core-qa.apk` (temporary ignored output, reproducible with the checked-in script).

The force-stop test preserved the exact pending operation, then applied a synthetic response once, rejected its duplicate and rejected a stale wallet. No real Google Play purchase or remote backend was used. RR-10-06 and RR-11-04 remain open for the integrated native prototype.

## Historical first build

1 October 2026. The first APK could not be installed: `INSTALL_FAILED_USER_RESTRICTED: Install canceled by user`. Preserve these original files as historical evidence. The old temporary APK has since been removed by temporary-folder cleanup.

The isolated Unity 6000.4.9f1 project built successfully with **0 errors** in **3m 30.829s**, producing a **141,145,208-byte ARM64 IL2CPP development APK**. Package `com.diwali.rocketrajaredesign.qa`, app name `Rocket Raja Core QA`, version 0.0.1. No Firebase, billing, ads, account credentials or existing player saves were included.

- `build-result.txt`: actual Unity BuildPipeline outcome.
- `source-manifest.json`: hashes of the exact copied C# sources and catalogue used for this diagnostic.
- `apk-sha256.txt`: binary digest.
- `install-result.txt`: actual Android install rejection.
- `installed-game-before.txt` and `installed-game-after.txt`: identical hashes of the existing game's APK. Its save data was not accessed or changed.

Historical APK path: `Temp/RedesignDeviceQA-20261001-172323/Build/RocketRaja-core-qa.apk`. Reproduce using `Tools/RedesignDeviceQA/build.ps1`; protocol and limitations are documented in `Tools/RedesignDeviceQA/README.md`.

This intermediate diagnostic neither replaces the installed game nor closes final game build/device acceptance.
