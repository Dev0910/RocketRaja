# Rocket Raja

Portrait incremental game for Android. Tap a growing Indian fireworks stall, fill eight product displays, buy 96 run upgrades, use three skills, and prestige into 12 permanent Legacy perks.

## Open and play

- Open this folder in **Unity 6000.4.9f1**. The requested 6000.3.8f1 directory on this machine did not contain an editor executable; 6000.4.9f1 is the installed working editor used for the project.
- Open `Assets/Scenes/Main.unity` and press Play. The UGUI interface is saved in thirteen screen prefabs under `Assets/Game/ImportedUI`; `GameView` routes navigation and dialogs through their presenters. Economy content is authored in `Assets/Resources/GameContent.asset`.
- `Rocket Raja > Build Windows Preview` creates `Builds/Windows/RocketRaja.exe`.
- `Rocket Raja > Build Android Test APK` creates an ARM64 development APK with Google's sample advertising identifiers. This is a test build, not a store release.
- The lower Player, Products and Legacy tabs open their panels; use the collapse control to return to Home. Tap the shop scene to earn; lower panels never award taps.

## Verify

```powershell
dotnet run --project Tools/EconomyLab
dotnet run --project Tools/EconomyLab -- --simulate
dotnet run --project Tools/EconomyLab -- --catalog Documentation/ContentCatalog.json --simulate
```

The pure C# economy suite requires .NET 10 and does not require Unity. It covers prices, bulk buying, milestones, prestige, ad exclusion, offline caps, idempotency and checksum backup recovery. The simulator models a stated purchasing policy, not observed human retention.

Existing pre-rename delivery: Android test APK in `Builds/Android/Diwali-test.apk` (Android 7.1/API 25 or newer, ARM64), and a portable Windows preview archive in `Builds/Diwali-Windows-preview.zip`. Rebuild to produce Rocket Raja-branded binaries. Full evidence and outstanding release gates are in `Documentation/Validation.md`.

Development Windows builds also accept `--smoke <absolute-output-directory>`. This uses a separate temporary save and a deterministic ad adapter, exercises real Unity controls, and writes screenshots plus `SmokeResults.txt`. Test hooks compile out of production builds.

## Project map

| Path | Purpose |
|---|---|
| `Assets/Game/Core` | Large-number arithmetic, catalog, economic commands, state, save envelope |
| `Assets/Game/Runtime` | UGUI views, lifecycle, consent/reward adapter, animation and audio |
| `Assets/Game/Editor` | Reproducible scene preparation and build commands |
| `Assets/Resources/Art` | Original generated stall stages, 32 product displays and shopkeeper frames |
| `Tools/EconomyLab` | Core checks and deterministic three-cycle simulations |
| `Documentation` | Validation evidence, content export, art provenance and release instructions |

## Local saves

Saves use stable content IDs, schema version 1, a SHA-256 checksum, an atomic main-file replacement and a validated backup. They are stored in Unity's `Application.persistentDataPath`, in `diwali-v1.save`. Prestige, Legacy purchases, skill activation and ad rewards save immediately; normal play saves every ten seconds and on suspension. Offline reconciliation and reward receipts are saved together with their payout. There is no cloud recovery.

Google authentication and Firestore cloud saves are integrated through the new [Bootstrap scene](Assets/Scenes/Bootstrap.unity), first before Main. Guests stay local; Settings opens account controls. See [Cloud saves](Documentation/CloudSaves.md) and [native startup validation](Documentation/BootstrapValidation/README.md). Real Android Google login and two-device acceptance remain deferred.

## Release ownership

See `Documentation/ReleaseRunbook.md`. Public release requires an owned application ID and signing key, verified Play Console/AdMob accounts, a published privacy policy, completed testing and accurate policy declarations. The production build command deliberately rejects sample ad identifiers and missing signing/privacy configuration. Nothing has been uploaded or published by this implementation.

## Project naming

Use **Rocket Raja** for all new game branding and **RocketRaja** for code namespaces and build filenames. The workspace directory remains `Incremental game`. Existing compiled builds and historical artwork/evidence retain their original names until regenerated. The company name and Android application identifiers remain stable for compatibility. Desktop players with existing saves continue using the original product save folder; new players use the Rocket Raja folder. The save filename remains `diwali-v1.save`.
