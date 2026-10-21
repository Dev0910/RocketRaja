# Ad retirement evidence

The existing Main scene and saved Home, Offline, Settings and Privacy presenters are ad-free. This is removal of retired behavior from existing assets, not approval or implementation of the new redesign screens. Main still uses its earlier economy/navigation pending RR-07 integration.

`GameController` never initializes, polls or invokes an advertising provider. Its retained compatibility method rejects each request synchronously. `GameEngine.RewardAd` rejects every placement/receipt without changing state; a saved `sponsorUntil` does not multiply displayed or accrued income. The old serialized fields remain readable for save compatibility. The new economy has no sponsor or ad receipt fields; its restricted fixture migration drops old sponsor/receipt/offline-offer state without an extra credit. Actual deployed saves remain protected by the separate transition policy.

The Sponsor canvas and prefab, Home offer, offline video/offer/message rows, and advertising consent rows are removed from saved assets. Continue and Close preserve income already settled by the engine. Old event/callback methods remain inert during asset transition. Builders no longer generate ad controls, and the retired Sponsor builder rejects explicit calls. Production validation no longer requires AdMob configuration. The old privacy claim about advertising services was removed; analytics settings were not changed. Full production privacy/disclosure review is still RR-11-05.

Unity UPM removed `com.google.ads.mobile` 11.5.0, including its UMP dependency. The manifest, lockfile, generated Android dependencies and Gradle templates contain no AdMob/UMP requirement. `Assets/GoogleMobileAds` settings/linker declarations are removed. Embedded `com.google.external-dependency-manager` 1.2.189 and all selected Firebase 13.17.0 modules remain. The resolver regenerated its managed dependency artifacts; no Firebase SDK import or version change occurred.

## Checks

- [Focused Play Mode results](SmokeResults.txt): **19 PASS**, no runtime errors recorded by the harness. Fresh disposable guest, live cloud disabled. Covers boot without Sponsor, saved controls, ordinary offline settlement, duplicate/late callbacks, dismissal and released input/modal state.
- [Offline capture](ad-free-offline.png) and [Home capture](ad-free-home.png): actual Unity Game View, 1080×2400. Visually checked that the removed controls are absent and Continue/reward text render.
- [Legacy core after retirement](../Regression-LegacyCore-AdRetirement.txt): **310 PASS**. Updated only ad expectations; retained unrelated old-save, quantity, numeric, settings/name and recovery checks. The earlier 308-check baseline is preserved separately.
- [Full interaction regression](Regression-Final/SmokeResults.txt): **141 PASS**, no runtime errors recorded. The [first attempt](Regression/SmokeResults.txt) exposed a hard-coded modal index in the test after Sponsor removal; it was fixed to identify the recovery screen by type. Failed evidence is retained, and the complete corrected run passes.
- [Saved assets](Unity-Assets.txt): **6 PASS** for no missing scripts in the five edited prefabs and successful compilation after SDK removal.
- Redesigned core, cloud protocol and parity checks remain in the parent evidence index.

Reproduce focused runtime checks by setting Unity `SessionState` boolean `RocketRaja.EditorSmoke` to true, `RocketRaja.HtmlMotionStage` to `ad-retirement`, and `RocketRaja.EditorSmokeDirectory` to an absolute disposable evidence directory, then play the saved Main scene. The harness exits Play Mode and resets its smoke flag; its guest data is written to a unique temporary directory. Run `dotnet run --project Tools/EconomyLab -- --catalog Documentation/ContentCatalog.json` for the legacy compatibility checks. `Rocket Raja / Redesign / Retire Existing Ad Controls` repeats saved-asset cleanup but refuses unsaved scene edits.

For the full existing interaction regression, use the same SessionState setup with an empty `RocketRaja.HtmlMotionStage`. The saved-prefab inspection loads Home, Offline, Settings, Privacy and Prestige with `AssetDatabase.LoadAssetAtPath<GameObject>` and requires `GameObjectUtility.GetMonoBehavioursWithMissingScriptCount` to return zero for every descendant; `EditorUtility.scriptCompilationFailed` must be false.

Scope: no device installation, advertising network request, real user save change or new-screen acceptance. Unity editor logs retain the existing path-space/importer diagnostics. Two TMP `OnValidate` exceptions appeared during editor reload/asset processing; no orphan TMP submesh was found in the subsequent inspection, and the focused runtime checks recorded none. These are not represented as an entirely empty editor console.
