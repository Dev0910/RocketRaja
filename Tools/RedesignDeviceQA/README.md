# Isolated Android core diagnostic

`build.ps1` copies the current pure C# core and validated catalogue into a new Unity project under ignored `Temp/RedesignDeviceQA-<timestamp>`. It launches the same Unity 6000.4.9f1 editor in hidden batch mode and builds an ARM64 IL2CPP development APK. No Firebase, Play billing, ads, game scenes, account credentials or existing user save are copied.

The fixed package is **com.diwali.rocketrajaredesign.qa**, displayed as **Rocket Raja Core QA**. It uses Android debug signing and a separate application data directory. Never install this as `com.diwali.fireworkstycoon.dev`, uninstall the user's game, clear its data or use this as the final redesigned game build.

After a successful build, verify the connected device/target package, install the diagnostic APK, and launch it. It writes `report-before.json` in its own `Application.persistentDataPath`, then stays alive with a blank diagnostic view. Force-stop **only the QA package**, relaunch, and read `report-after.json`. The second phase resumes the exact locally persisted pending currency operation and applies a synthetic fixed response. No purchase, server acknowledgment, authentication or real premium grant occurs.

The first phase measures full-catalogue Buy All quotes, cached income, validation, JsonUtility encode/decode, real atomic file saves, repeated prestige/free-draw/rebuild cycles and retained managed memory. It checks first purchase, the 1,000-entry catalogue, first-prestige rewards, secure random draw/reveal persistence, offline idempotence and schema-3 pending operation persistence. The second phase tests force-stop recovery, duplicate responses and stale wallet rejection.

Record build result, exact source hashes, hardware/runtime metadata and both reports under `Documentation/Implementation/DeviceCore/`. Timings are from an isolated development IL2CPP app; they do not establish the native game's frame rate, scroll/input quality, integrated lifecycle, Firebase ownership or Google Play acceptance. RR-10-06 and RR-11-04 remain open for those integrated checks. This intermediate diagnostic does not satisfy the user's final commit/build/replacement request.

Build command from the repository root: `Tools/RedesignDeviceQA/build.ps1`. The script prints the temporary project, log, process ID and APK paths. It creates a fresh folder and does not recursively delete or move existing work.
