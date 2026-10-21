# Rocket Raja LLM Handoff

## Read this first

This is the working handoff for the **Rocket Raja** Unity project. It is written for a new LLM or engineer joining an active branch, not for a player or store reviewer.

The repository is **not clean**. At the start of this handoff snapshot (22 October 2026, India time), the branch was `codex/redesign-implementation` at commit `ce519aa` and had 44 modified tracked files plus 58 untracked files. The active work is valuable user work: do not reset, clean, reimport wholesale, regenerate the scenes, or discard changes in an attempt to get back to the last commit.

The first task for a successor should normally be:

1. Read `AGENTS.md`, this file, and `git status --short`.
2. Preserve the current working tree and inspect the diff before making related changes.
3. Treat current source code and the newest dated implementation evidence as authoritative over older narrative documents.
4. Use Unity/Editor authoring for scene work. Do not hand-edit serialized `.unity` YAML.

## Project identity and current status

| Item | Current value |
| --- | --- |
| Game | Rocket Raja, portrait Android incremental game about growing an Indian fireworks stall |
| Company | Diwali Games |
| Unity | 6000.4.9f1 |
| Primary platform | Android, IL2CPP, ARM64, minimum API 25 |
| Current Android ID | `com.diwali.fireworkstycoon.dev` |
| Runtime UI | Native UGUI + TextMeshPro serialized in scenes |
| Build scene order | `Assets/Scenes/Bootstrap.unity`, then `Assets/Scenes/Main.unity` |
| Current active presentation | `ClassicProductionCanvas` in `Main.unity` |
| Economy/content version | `3.0.0` / `faster-start-v1` |
| Source control | Git; remote is `https://github.com/Dev0910/RocketRaja.git` |

The internal names need care:

- **Classic** means the current, player-facing production UGUI presentation and its saved scene hierarchy.
- **Redesign** names the active modern economy, save, account, and session code that Classic uses. It is not the old visual UI that was retired.
- **Legacy** means preserved compatibility code and historical game behavior. It is not the current production gameplay path.

The source may therefore use `RedesignEngine`, `RedesignState`, or `RedesignSessionHost` while the live UI is `ClassicUI` and `ClassicApplication`. This is intentional.

## What the current game does

The active game is a portrait incremental game with:

- eight product families;
- manual tapping plus automatic production;
- a generated, validated catalogue of 1,000 cash upgrades;
- three active skills;
- Fame/prestige, free credits, Kaju Katli currency, and 28 Talent cards;
- offline settlement, persistent reveals, milestones, contextual guidance, and save recovery;
- a Classic UI with Home, Player, Products, Legacy, and Shop surfaces plus dialogs;
- optional Google-backed account/cloud progress alongside isolated local guest progress.

The production shop does **not** make real purchases. Client/server premium code and Unity Purchasing infrastructure are present for future work, but real Play billing, store products, backend deployment, and production fulfillment are not enabled.

The current working tree also introduces/extends:

- staged achievements and a persisted reward ledger;
- red-dot notifications for actionable content;
- Classic HTML-to-native scene synchronization and popup refinements;
- current Main/Bootstrap scene updates and account-flow polish.

The in-progress achievement implementation currently contains 23 code definitions in `Assets/Game/Core/RedesignAchievements.cs`. Some 21 October evidence describes an earlier 15-achievement version. Treat the live source as the proposed current behavior and rerun focused tests before calling any achievement count accepted.

## Runtime architecture

```text
Bootstrap.unity
  BootstrapController
  RedesignSessionHost
  ├─ guest local profile / recovery
  └─ optional Google -> Firebase auth -> Firestore account session
            |
            v
Main.unity
  ClassicProductionCanvas
  ClassicApplication
  RedesignRuntimeDriver / RedesignLifecycle
  RedesignEngine + RedesignState + RedesignCatalog
  ClassicUI and saved controls/dialogs
```

Important owners and boundaries:

| Area | Main owner(s) | Responsibilities |
| --- | --- | --- |
| Bootstrap/account flow | `Assets/Game/Runtime/Cloud/BootstrapController*.cs`, `RedesignSessionHost.cs` | Guest/account selection, sign-in, recovery, conflict choice, device handoff, entry to Main |
| Active Classic app | `Assets/Game/Runtime/Classic/ClassicApplication.cs`, `ClassicUI*.cs` | Binds a selected session to the saved UI, navigation, dialogs, feedback, and display bindings |
| Lifecycle | `Assets/Game/Runtime/RedesignRuntimeDriver.cs`, `Assets/Game/Core/RedesignLifecycle.cs` | Initialization, game time, save timing, offline settlement, and session changes |
| Economy | `Assets/Game/Core/RedesignEngine.cs`, `ProgressionNumbers.cs`, `EconomyBalance.cs` | Quotes and authoritative in-process command rules for tap/product/upgrade/skill/prestige/draw operations |
| State and persistence | `RedesignState.cs`, `RedesignSaveStore.cs`, `RedesignAccountCoordinator.cs` | Validated state, atomic local saves/backups, account checkpointing, ownership/retry fencing |
| Auth/cloud | `FirebaseAccountAuth.cs`, `GoogleSignInBridge.cs`, `FirestoreSaveStore.cs` | Google credential flow, Firebase identity, server-backed Firestore transactions/listening |
| Content | `GameContent.asset`, `RedesignRuntimeContent.cs`, `RedesignCatalog.cs` | Authored data and strict validation; runtime never falls back to Markdown data |
| Scene/UI authoring | `Assets/Game/Editor/Classic/*`, `BuildGame.cs`, `BuildBootstrap.cs` | Editor-only saved-scene construction, migration, validation, and build commands |

`ClassicApplication` deliberately does not recalculate economy rules. It observes the bound session and opens recovery, draw-reveal, and offline-receipt UI from already persisted state. UI code should ask the engine for quotes and then invoke engine commands; it must not recreate price, payout, prestige, or reward math.

## Scenes and UI authoring

### Bootstrap

`Assets/Scenes/Bootstrap.unity` is first in Build Settings. It contains a serialized native UGUI account/startup surface. It selects a guest or Google account session, handles recovery and conflict choices, then enters Main. The old Bootstrap canvas is retained inactive as `Bootstrap Canvas__LegacyDisabled_20261019`.

### Main

`Assets/Scenes/Main.unity` is the active game scene. Its production root is `ClassicProductionCanvas`; the scene also has the camera and EventSystem. The old Classic canvas is retained inactive as `ClassicProductionCanvas__LegacyDisabled_20261019`. Do not mistake inactive serialized objects for live production UI.

All production UI is saved into these scenes. There are no active `.prefab` files under `Assets/Game`; older README references to thirteen screen prefabs are historical. HTML under `UI Builder/HTML` is a design/parity source, not a runtime WebView dependency.

The UI contains the gameplay surfaces, skills, statistics, settings/privacy, account/recovery, achievement, product, Talent/card, draw/reveal, prestige, and offline dialogs. Keep their serialized references stable. The Editor authoring code explicitly enforces that production scene work is saved, and production guards reject preview/probe components and legacy game roots.

For current HTML-to-Unity ownership and the 29 dialog routes, start with:

- `Documentation/Implementation/Classic-HTML-to-Unity-TODO.md`
- `Documentation/Implementation/ClassicHTMLSync/2026-10-19-native-sync.md`
- `Assets/Game/Runtime/Classic/ClassicUI.HTMLSync.cs`
- `Assets/Game/Editor/Classic/ClassicHTMLSyncAuthoring.cs`

## Economy, data, and save model

`Assets/Resources/GameContent.asset` holds both the preserved legacy `Catalog` and current `RedesignContent`. `RedesignRuntimeContent.Load()` selects and validates the modern content. The current content contract requires exactly eight products, 1,000 uniquely identified cash upgrades, 28 Talent cards, and four authored premium-pack definitions. It validates source/version/hash and economic structure before use.

The current production state is `RedesignState`:

- schema 2 is local-only state; schema 3 is the authoritative-wallet form;
- natural-number counters avoid floating-point loss for counts and currencies where exactness matters;
- `Amount` supports large incremental-economy values;
- commands are quote-bound to owner/session/run/command revisions to reject stale UI actions;
- `RedesignSaveStore` uses checksums, temporary-file writes, replacement, backup, and preserved damaged/interrupted copies;
- changes that grant/consume currency, prestige, draws, offline receipts, and achievement claims should remain transactional and persist before presentation;
- normal/guest and account profiles must remain isolated; do not silently promote prototype, disposable, or legacy saves.

The legacy `GameEngine`, `GameState`, `Catalog`, and `SaveStore` code remains for compatibility fixtures and historical behavior. Do not use it as the implementation target without first confirming the request is explicitly about compatibility work.

## Accounts, Firebase, and premium services

### Google and cloud saves

Guests are local. The account path is optional and starts only through Bootstrap. `RedesignSessionHost` creates the active cloud adapter with `FirestoreSaveStore.RedesignCollection`, so **current Classic production uses the `redesignUser` collection**. The older `user` collection/default adapter remains a compatibility path; Firestore rules intentionally recognize both. Older documents that say current production uses only `user` are historical.

Account operations are designed around server-backed reads/transactions, revision/commit receipts, single-device ownership leases, handoff fencing, and local checkpoints. The game is not server-authoritative for the actual economy; account/cloud protection prevents common cross-account/stale-write issues, not client-side cheating.

`Assets/Resources/RedesignCloudSettings.asset` currently enables the reviewed cloud configuration. Recent 21 October evidence records a successful one-device Google sign-in and cloud commit; two-device handoff acceptance remains open.

### Firebase handling

Read `AGENTS.md` and `Documentation/FirebaseSDK.md` before touching Firebase.

- Reuse Firebase Unity SDK **13.17.0** from `C:\Unity\Assets\firebase_unity_sdk_13.17.0`.
- Importable packages live in `C:\Unity\Assets\firebase_unity_sdk_13.17.0\firebase_unity_sdk`.
- Selected modules: Analytics, Crashlytics, Remote Config, Auth, Firestore, and App Check.
- EDM4U 1.2.189 is embedded at `Packages/com.google.external-dependency-manager`.
- Do not import a second EDM4U copy from a Firebase package.
- Preserve the existing `Assets/google-services.json`; never replace it with a sample.
- The project path has a space. `Assets/Editor/FirebaseRepositoryUriFix.cs` patches generated Gradle repository URLs so they encode it correctly. Keep that callback while using the current resolver.

Never put values from `.env`, service credentials, keystores, ignored `ProductionConfig.asset`, or account tokens into source, reports, or prompts.

### Premium authority

`Backend/Premium` is a Node 22+ server-only implementation. It has testable receipt/wallet/draw/refund logic but is explicitly **undeployed**. It requires an explicit endpoint, sandbox/Play configuration, App Check provider configuration, service identity, runtime scene binding, reviewed shop, live monitoring/refund handling, and real store/device acceptance before it can handle real money. Do not deploy it or create products merely because its tests pass.

See `Backend/Premium/README.md`, `Documentation/Implementation/PremiumAuthority.md`, and `Documentation/Implementation/PremiumDeploymentReview.md` before changing that area.

## Repository map

```text
Assets/
  Game/
    Core/                 Pure-ish economy, state, save, account, premium contracts
    Runtime/              Unity runtime integration, views, motion, audio, smoke harness
      Classic/            Active production UGUI shell and presentation logic
      Cloud/              Bootstrap, Google/Firebase, session and Firestore integration
    Editor/               Editor-only authoring, migration, validation, build commands
  Resources/              GameContent and runtime configuration assets
  Scenes/                 Bootstrap.unity and Main.unity
  Game/ClassicUI/         Saved UI art/support assets, including current HTML-sync work
  Plugins/Android/        Android bridges and Firebase/Gradle integration
  Editor/                 Cross-cutting editor callbacks such as FirebaseRepositoryUriFix
Backend/Premium/          Undeployed Node/Firebase premium authority
Tools/                    .NET and Node validation/authoring/preview tools
UI Builder/HTML/          Editable browser reference/parity implementation and art references
Documentation/            Evidence, runbooks, architecture, implementation records
Game Design/              Product/economy decisions, research, roadmap, TODOs
Packages/                 Unity package manifest and embedded EDM4U
ProjectSettings/          Unity/Android configuration and Build Settings
```

Useful tool directories:

| Path | Purpose |
| --- | --- |
| `Tools/RedesignLab` | Main .NET 10 regression suite for current economy/content/account contracts |
| `Tools/EconomyLab` | Legacy/core compatibility checks and simulations |
| `Tools/CloudSaveLab` | Cloud-save-focused .NET checks |
| `Tools/CloudSaveTests` | Firebase rules/Admin transaction checks against a local demo emulator only |
| `Tools/ClassicUiPreview` / `Tools/ClassicUiImport` | HTML/classic UI preview/import support |
| `Tools/RedesignDeviceQA` | Isolated Android core diagnostic; not the full game |

## How to open, run, test, and build

Open the repository in Unity **6000.4.9f1**. Build Settings should contain Bootstrap before Main. Opening Main directly in Play mode routes through Bootstrap when no session is selected.

Safe first validation commands from the project root:

```powershell
dotnet run --project Tools/RedesignLab
dotnet run --project Tools/EconomyLab
dotnet run --project Tools/EconomyLab -- --simulate
```

Premium/server tests require Node 22+ and their local dependencies:

```powershell
npm ci --prefix Backend/Premium --ignore-scripts
npm --prefix Backend/Premium test
```

Cloud-rule tests must target the local demo emulator, never the live Firebase project:

```powershell
npm ci --prefix Tools/CloudSaveTests --no-audit --no-fund
.\Tools\CloudSaveTests\run.ps1
```

Important Unity menus include:

- `Rocket Raja > Prepare Project`
- `Rocket Raja > Build Windows Preview`
- `Rocket Raja > Build Android Test APK`
- `Rocket Raja > Build Production Bundle`
- `Rocket Raja > Validate Unity Save Codec`
- `Rocket Raja > Validate Cloud Save Codecs`
- `Rocket Raja > Run UI Smoke Checks in Editor`
- the `Rocket Raja > Classic` authoring/validation commands
- the `Rocket Raja > Redesign` content/core/production validation commands

`Prepare Project` and several authoring menus deliberately rewrite generated content, scenes, and settings. On this dirty working tree, do not run them as a casual diagnostic; inspect the authoring code and make sure the requested scope permits its changes first. `Build Production Bundle` also has intentional signing, application-ID, privacy-URL, and release-acceptance gates. A development test APK is not a store-ready artifact.

## Current work in progress

The latest committed change is not the full state of the project. Current uncommitted work spans:

- `Assets/Scenes/Main.unity` and `Assets/Scenes/Bootstrap.unity` (very large serialized diffs);
- core account/session/engine/state changes;
- active Classic controls, guidance, layout, bindings, notifications, and HTML-sync behavior;
- new achievement ledger, staged achievement authoring, red dots, popup clearance, panel controls, and supporting art;
- Bootstrap popup/feedback refinements;
- `UI Builder/HTML/Classic` parity/reference revisions;
- validation/evidence additions under `Documentation/AchievementValidation` and `Documentation/Implementation`.

Treat scene diffs as a reviewable unit. A whole-file format, regeneration, or revert could erase extensive serialized UI work. Keep line-ending-only churn out of any review where possible.

The latest available connected Android evidence says a signed development build was installed over an existing app without clearing data, and current save/cloud behavior was preserved. That is encouraging evidence, not permission to skip regression, physical-device, or release acceptance for subsequent changes.

## Documentation guide and conflicts

This repository intentionally retains historical evidence. Dates and scope matter more than filename prominence.

Use this precedence order:

1. Current source and current serialized scenes/configuration.
2. This handoff and the newest dated reports under `Documentation/Implementation/ClassicAndroid/` and `Documentation/Implementation/ClassicHTMLSync/`.
3. `Documentation/FirebaseSDK.md`, current Classic integration reports, and the current `Game Design/Implementation TODO.md`.
4. Broad architecture/design/evidence documents, but only after checking their date and whether they describe Classic production or historical Redesign/legacy work.

High-value current/historical references:

| File or area | Use it for |
| --- | --- |
| `README.md` | Fast orientation and common commands; cross-check details because it mixes eras |
| `Documentation/Architecture.md` | Current Classic production contracts and labelled historical redesign context |
| `Documentation/FirebaseSDK.md` | SDK, dependency manager, Android sign-in, and Gradle caveats |
| `Documentation/CloudSaves.md` | Cloud model/history; cross-check dated device status and collection name with current code |
| `Documentation/Implementation/Classic-Production-Integration.md` | Current startup/UI ownership and profile safety rules |
| `Documentation/Implementation/Classic-Save-Account-Integration.md` | Session, save, and account integration evidence |
| `Documentation/Implementation/ClassicHTMLSync/2026-10-19-native-sync.md` | Current Classic HTML-to-native transfer ownership and visual verification |
| `Documentation/Implementation/ClassicAndroid/` | Latest device, login, and achievement/talent records |
| `Documentation/AchievementValidation/README.md` | Earlier achievement evidence; verify current source changed since it |
| `Documentation/Implementation/PremiumDeploymentReview.md` | Explicit future/deployment gates for premium authority |
| `Documentation/ReleaseRunbook.md` | Release gates and what remains intentionally deferred |
| `Game Design/Implementation TODO.md` | Product/release requirement tracker and deferred scope |

Known stale or mixed-era material:

- `Documentation/Validation.md` is a September, ad-era validation record. Ads/UMP were later retired and its runtime/UI description is not the current production target.
- Parts of `README.md` mention old prefabs, a legacy local-only/cloud status, and historical product behavior. Use it for navigation, not as sole authority.
- Older `Documentation/CloudSaves.md` passages say device validation was deferred; more recent Android evidence supersedes that claim.
- Many documents say `user` for cloud saves. Current active Classic code constructs the `redesignUser` adapter; `user` remains compatibility history.
- Old `Redesign` visual-scene and `GameEngine` narratives are historical if they conflict with the current Classic production route.

When updating documentation, preserve evidence history and add a dated correction rather than rewriting an old report to pretend it described later work.

## Product and release boundaries

Still open before a public release:

- full physical-device acceptance across target devices/aspect ratios, background/resume, performance, and accessibility/readability;
- real two-device Google cloud handoff and recovery acceptance;
- uncoached player tests and evidence-based pacing review;
- approved production identity, signing, privacy/support/disclosure, Play Console/testing, and final artifact verification;
- real billing/store product configuration, backend deployment, App Check configuration, sandbox purchase/recovery/refund tests, and monitoring;
- final review/commit of the full active working tree, followed by a reproducible build from that reviewed commit.

Do not infer release readiness from a passing local test suite, Android installation, emulator run, or browser preview. Similarly, do not turn on analytics, Crashlytics, Remote Config, App Check, premium endpoints, IAP products, or Firebase deployments without an explicit request and corresponding configuration review.

## Operating rules for a successor LLM

- Preserve unrelated changes. This workspace is shared and intentionally dirty.
- Use `rg`/targeted reads first; do not load or rewrite huge scene files blindly.
- Make C# behavior changes in the owning layer: engine/state for rules, runtime driver for lifecycle, Classic presentation for view bindings, and Editor scripts for serialized scene authoring.
- Do not construct permanent UI at runtime when an existing saved control or dialog should be authored into the scene.
- Keep local, guest, account, preview, development-fixture, and legacy profile namespaces separate. Never silently migrate or replace a save.
- Keep command/quote fencing, persistence-before-presentation, and account-ownership checks intact. These are correctness features, not optional polish.
- Never put secrets, configuration values, OAuth tokens, signing material, or `.env` contents in code or documentation.
- Before importing Firebase, read `AGENTS.md` and `Documentation/FirebaseSDK.md`, keep all Firebase modules at 13.17.0, and avoid duplicate EDM4U imports.
- Use the local demo emulator for cloud-rule tests. Do not make a production Firebase deployment as part of testing.
- For UI changes, maintain parity with the HTML/reference intent but regard native scene layout, input, focus restoration, safe areas, and real engine bindings as the production truth.

## Suggested first reading path for a new task

1. `AGENTS.md`
2. This handoff and `git status --short`
3. `README.md`
4. `Documentation/Architecture.md`
5. The newest dated Classic/Android report relevant to the requested area
6. The owning source files listed in the runtime architecture table
7. The related test/lab project before changing behavior

That sequence gives a new LLM enough context to distinguish current Classic production work from preserved legacy code and historical design/evidence, while keeping the active user changes safe.
