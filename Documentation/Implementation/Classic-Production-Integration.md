# Classic production startup integration

6 October 2026. Implements the first release workstream (REL-01–05). Store distribution, final Google configuration and real billing acceptance are separate, still-open release gates.

## Saved route and ownership

`Bootstrap.unity → Main.unity → ClassicProductionCanvas` is the normal build route. Main also redirects to Bootstrap if opened directly without a selected session. `EditorSceneManager.playModeStartScene` is cleared. The Android build profile uses the global Bootstrap/Main list.

Main contains the saved approved Classic hierarchy, camera and EventSystem. The previous Game, redesigned Canvas and imported UI roots were removed at the user's subsequent cleanup request; see [asset cleanup](ClassicCleanup/README.md). `ClassicPreviewSession` and `ClassicDeviceProbe` are absent from the production hierarchy. All controls, dialogs, text slots and 64 effect hosts exist before Play; the integration adds no runtime UI construction.

`RedesignSessionHost` retains identity and the selected account. `ClassicApplication` binds the selected guest/account to the single active `RedesignRuntimeDriver`. The driver owns gameplay time, local saving and departure/offline settlement; account coordination owns cloud acknowledgment/ownership. The host ticks the account only outside Main, and does not preempt the driver's departure save on Android pause.

Settings/account/recovery entry points open the actual saved Bootstrap account flow. Existing guest/cloud selection, promotion, reconnect, back-to-shop, handoff and sign-out remain coordinator operations. Preview account dialogs are retained as inactive compatibility views, but production navigation cannot enter them.

## Profile contract

- Normal guest: `Application.persistentDataPath/rocket-raja-v2.save`, with its existing backup and recovery handling.
- On this phone, an older `diwali-v1.save` was present. The user explicitly chose to preserve it and allow a separate fresh Classic profile. Bootstrap now offers a confirmation only for that legacy case. Confirming selects `Application.persistentDataPath/classic-v2/rocket-raja-v2.save`, remembers the selection, and never moves or rewrites the older files. An existing separate Classic save is loaded rather than reseeded. Corrupt current-format saves do not receive this choice.
- Account checkpoints: the coordinator's existing `economy-2` subdirectory under that profile root.
- Identity preferences: `rocket-raja-v2.` prefix, separate from legacy/disposable selections.
- New state comes only from `RedesignState.Fresh`: 30 cash for the first manual Phuljhari purchase; no owned products, Fame, Talent cards, KK or free credits.
- `redesign-v2-disposable`, `ClassicImporter-*` and `classic-playtest-v1` are never automatic sources for normal startup. Existing data there is retained. No legacy or prototype conversion is enabled by this change; REL-12 still governs migration policy.
- A legacy sibling save, unsupported future data, corrupt main/backup, marked development fixture, converted fixture or simulated-shop history cannot silently become a fresh normal game. Their files remain intact and Classic presents recovery.
- New Classic review profiles carry persistent `developmentFixture` provenance. Normal initialization, account promotion and account state acceptance reject it. Preview startup is confined to HTML Importer and refuses an existing session host.

## Recovery and screen behavior

Classic displays its saved recovery dialog even when loading failed before an engine exists. Retry uses the driver's load/save/account recovery operation. All gameplay commands and taps are blocked while the session cannot mutate. Account navigation cannot destroy an unsaved local checkpoint. Leaving recovery backgrounds the Android task while retaining files.

Successful recovery rebinds the current account engine, clears stale purchase/draw/prestige quotes and fences old account bindings. Pending reveals and already-settled offline receipts return through their saved views. Animations never settle time or grant rewards.

| Approved behavior | Connected implementation |
|---|---|
| Four tabs and collapse to Home | Existing Player / Products / Legacy / Shop controls |
| Direct product/tap purchase | Existing engine quote/commit, no upgrade-detail navigation |
| Next ten / Buy All | Eligible catalogue ordering and authoritative batch commit |
| Product milestone bars | Existing per-product progress binding and fixed images |
| Inline four-column Talent | Existing 28 saved cards, disabled undiscovered cards |
| Outside-card dismissal / Continue | Existing card modal and persisted reveal continuation |
| Removed upgrade-detail / collection popups | Remain absent |
| Account and recovery screens | Real Bootstrap/session operations, no preview outcomes |
| Shop | Packs disabled and labeled unavailable; no sample prices or simulated success flow is reachable in production. Real storefront/fulfillment remains REL-17–21. |

`ProductionConfig.releaseAcceptanceComplete` defaults false. Ordinary Build Profiles/BuildPipeline builds, including non-development APKs for manual installation, validate content without requiring this approval. The **Rocket Raja → Build Production Bundle** command retains release acceptance, production identity, privacy URL and signing checks. A scene build guard still checks one active Classic driver, inactive legacy gameplay and no preview/probe in non-development scenes. A manually installed APK does not establish store-release readiness.

## Verification

- [Connected native flow](Classic-Production-Validation.txt): **79 passing assertions, no runtime errors**, in the actual saved Bootstrap/Main scenes, with Editor-only auth/cloud adapters and disposable files. Covers fresh/manual purchase, repeated scene return, real file-lock failure, corrupt load without fallback, retry, account binding, offline reconnect, pause/resume, sign-out, stale-binding rejection and unchanged UI/component identities. Additional checks exercise milestone bars, direct tap/product purchases, next-ten ordering, Buy All, four-column Talent, five-card Continue, outside-card dismissal, and the explicit separate-profile confirmation while retaining byte-identical legacy files. Windows transient replacement locks were recovered through the same Retry action.
- [Profile boundary checks](Classic-Profile-Boundary.txt): five rejected profile classes preserve their files; non-development build remains gated.
- `dotnet run --project Tools/ClassicUiImport/ClassicUiImport.csproj`: 66 atomic draw/persistence checks pass.
- `dotnet run --project Tools/RedesignLab/RedesignLab.csproj`: 655 checks pass, including six additional development/converted fixture promotion checks.
- [Android build record](Classic-Connected-Android-Build.txt): uses the configured development app identity, Bootstrap/Main, no preview define and no boosted seed. See the [physical-device record](ClassicConnected/DeviceTest.md) for observed results and limits.
- Normal Editor Play, without fixture setup, reached persistent fresh Classic with 30 cash and zero products, Fame, KK or cards. On the POCO X2, actual legacy recovery was observed before the explicit separate-profile feature; the rebuilt app subsequently allowed purchases, returned through Account options, and retained purchased Phuljhari and Anar after a cold process restart. The package was updated in place without clearing data.

These checks do not establish live Google authentication, deployed rules, real billing, or a Play-installed release. `RedesignCloudSettings.enabledAfterReview` remains false; configuring the final package/signing/Firebase deployment is REL-14. The user confirmed the package is `com.diwali.fireworkstycoon.dev` and no Google Play test build is available yet. Store-installed validation remains pending that distribution. Do not close the full REL-01–05 workstream on fixture evidence alone.
