# Classic Android playtest

This development APK runs the reviewed Classic UI and its native animations from `Assets/Scenes/HTML Importer.unity`. Its application ID is `com.diwali.rocketrajaclassic.playtest` and its launcher name is **Rocket Raja Classic**.

The package keeps progress separate from the existing game. A new installation starts with the review shop, four free pulls, and 400 KK. Updating or relaunching retains this progress under `Application.persistentDataPath/classic-playtest-v1`. Shop outcomes remain isolated development simulations; there are no real payments or production account connections.

## Building

Open the importer scene in Edit Mode with the Android target selected, wait for compilation, then invoke `RocketRaja.Editor.BuildClassicAndroid.Prepare()` and `BuildClassicAndroid.Start("<unique-request-id>")` through the Unity Editor. The queued builder uses the existing ARM64/IL2CPP configuration, Development mode, and the build-only `CLASSIC_DEVICE_PREVIEW` define. Do not call `BuildGame.Prepare()` for this scene.

Output: `Builds/Android/RocketRaja-classic-playtest.apk`. `Build.txt` records the build result and `BuildMessages.txt` records warnings/errors. Original application identity, signing choice, bundle mode, and scene setup are restored after the build. Production scene lists are not replaced.

## Device checks

Use normal Android USB installation and accept the phone's prompt. Do not uninstall or clear the existing game to make this separate package install. A locked phone or a rejected installation prompt requires the device owner's action.

Exercise Home taps and drag rejection, all four tabs, product and tap upgrades, Buy All, scrolling, product details, owned/disabled cards, outside-card dismissal, free and paid 5x pulls, Continue and early dismissal, reduced motion, and background/relaunch persistence. Capture the shared back, rotating card, and settled reveal. Check runtime errors and unchanged UI object counts alongside screenshots; compilation alone does not verify these interactions.

`ClassicDeviceProbe` is saved in the scene and active only in this dedicated development build. It writes read-only state and visible control coordinates to `Application.persistentDataPath/classic-device-state.json` every 0.75 seconds. It provides no command channel. Coordinates use the Android top-left origin; modal overlays can block underlying controls listed in the diagnostic data. Frame timing includes development telemetry overhead and is not a production performance benchmark.

Device execution results are recorded separately in `DeviceTest.md` when installation has been attempted.
