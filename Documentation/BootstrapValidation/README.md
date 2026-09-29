# Native Bootstrap integration

The user approved HTML v1 and directed Unity reconstruction on 2026-09-29: “yes I like it make it in unity”.

## Saved result

- Scene: `Assets/Scenes/Bootstrap.unity`, enabled first in Build Settings, followed by Main.
- A saved uGUI Canvas, TextMeshPro labels, Images, Buttons, two save-summary cards, loading indicator and EventSystem. All 48 UI objects exist before Play Mode. No runtime UI constructor or HTML renderer is used.
- Artwork, Google icon, sliced surfaces, spinner and local TMP fonts: `Assets/Game/Bootstrap/`. The illustration is the approved text-free image; text, controls and state summaries remain editable.
- Runtime controller: `Assets/Game/Runtime/Cloud/BootstrapController.cs`. It reuses PlayerSessionHost and the existing auth/save interfaces. Button feedback is a small attached component animating an existing label.
- Settings account button/status serialized in `Assets/Game/ImportedUI/Settings Screen.prefab`. Privacy prefab and its authoring source describe optional Google sync. Existing Main scene file and shared fonts remain byte-identical to the pre-task baseline.
- Direct Main entry no longer silently loads a guest: it redirects to Bootstrap until a prepared session exists. Editor/Windows use guest play; Google authentication is Android-only.

## Reconstruction contract

The reference is 1080 × 2200. The CanvasScaler matches width, with unit ancestor scales. Background art envelopes the viewport while keeping its aspect. A safe-area adapter fits the existing content root. Editor-configured vertical layout anchors branding above a flexible scene region and a bottom content-sized ivory panel. Cards use a horizontal layout, and buttons retain 48-pixel targets at a 360-pixel width. State changes toggle existing content and update text; they do not repair geometry or assemble UI.

The source HTML metadata remains embedded in its canonical file. Its code-evidence strings were normalized to separate paths and symbols so the package inspector can validate them. Native bindings supersede the original `existing-needs-adaptation` notes; no second behavior authority was introduced.

| Trigger | Native owner and visible result | Motion / guards | Evidence |
| --- | --- | --- | --- |
| First launch / remembered identity | BootstrapController.Start, Welcome, Guest, Google | Guest initialization is local; Google services lazy | GuestRoute.txt, full smoke |
| Google save conflict | ChooseProgress + two summary cards | Explicit device replacement confirmation; no merging | All-state renders, existing cloud protocol suite |
| Another device | ContinueAccount / Enter(true) | Pending indicator; busy blocks re-entry; final save or lease expiry | Device/handoff renders; cloud protocol suite |
| Failed connection / auth | Error and retry/guest actions | Account remains paused until authoritative restore | Failure renders; protocol timeout/reconnect tests |
| Damaged guest save | Recovery / fresh confirmation | Preserve original files, explicit confirmation | Recovery/fresh renders; Unity codec checks |
| Settings account | Serialized account button/status | Sign-out returns to separate guest; blocked account offers reconnect | IntegrationChecks.txt; native Settings smoke |
| Loading | Existing spinner RectTransform | 300°/second clockwise, unscaled time; reduced-motion state stops it | MotionChecks.txt; spinner source timing from HTML |
| Button press | BootstrapButtonFeedback + Button color transition | One-unit label offset, restored on release/exit/disable | MotionChecks.txt |

## Validation evidence

- `SceneChecks.txt`: all serialized references assigned; 12 states at 1080×2200, 390×844, 360×640 and 360×540, plus a simulated safe area. Visible buttons contained and at least 48px; no overflowing TMP text. States retain the same hierarchy.
- `Renders/`: native captures for all 12 states. Welcome, conflict and compact layouts visually inspected.
- `GuestRoute.txt`: real guest button initialized an isolated local session; no auth or cloud adapters initialized before the choice.
- `Smoke/SmokeResults.txt`: **143 passing checks, SMOKE SUCCEEDED**, launched through Bootstrap into Main using isolated saves and test ads.
- `MotionChecks.txt`: spinner rotation, hiding on state exit, UI object reuse, and press/reset passed in Play Mode.
- `IntegrationChecks.txt`: saved Settings references and optional-cloud privacy copy passed. Unity cloud codec/recovery checks rerun with `ValidateCloudSaves.Run`.
- `CloudProtocol.txt`: **26 cloud protocol checks passed**.
- `Economy.txt`: existing core economy suite passed.
- `DirectMain.txt`: direct Editor play redirected to Bootstrap before initialization without loading player saves. `ReloadChecks.txt`: controls and cards survive scene reload.
- Android integration report: `AndroidBuild.txt`; output is `Builds/Android/RocketRaja-cloud-backend-check.apk`. **Succeeded**, zero errors, 29 warnings, 2m57s.
- `ChangedBaselineFiles.txt`: scoped modifications compared with pre-task hashes. Shared font bytes and Main scene remain unchanged. `PreTaskHashes.json` preserves the baseline. The temporary backup remains outside the project; automatic policy rejected its cleanup command, so it was retained.

## Practical limits

Native TMP metrics, sliced borders and paper fill approximate browser rendering; they are not pixel-identical. Dense native states hide the repeated footer to preserve room, and the short-screen scene is compressed behind the status panel. The decorative stall and shopkeeper are a single static art layer; there are no gameplay interactions on that illustration.

Google login, consent/account-picker behavior and real two-device transfer still need Android devices. The user explicitly deferred that acceptance testing. Editor presentation fixtures never call Google or Firestore and cannot certify live authentication. The unchanged Firestore security rules previously passed the 12-test emulator suite; scene reconstruction did not change those rules.
