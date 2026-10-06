# Classic save/recovery candidate

6 October 2026. Target remains `com.diwali.fireworkstycoon.dev` with the existing Diwali Games signing identity. POCO X2 is the only connected physical device. Preserve the user's existing Classic and legacy data during updates.

Native recovery evidence: [26 saved-scene checks](Native.txt), [702 core checks](Core.txt), [28 cloud protocol checks](Cloud-Protocol.txt), [66 draw checks](Draws.txt), [22 rules-emulator checks](Firestore-Rules.txt). Tests identify simulated clock/ENOSPC and disposable profile boundaries explicitly.

The build/install and physical restart results for the updated candidate are appended below after verification. Real Google sign-in is not enabled or claimed: current Firebase console access cannot list project apps, and only one physical device is available.

## Built artifact

- [Build report](Android-Build.txt): succeeded, zero errors, 22 warnings, 2m 44.17s. Development APK using Bootstrap/Main with no preview defines or boosted profile.
- Artifact: `Builds/Android/RocketRaja-classic-connected.apk`, 118,977,756 bytes.
- SHA-256: `5FD3EAB1B80BAA0AAA164B00BA4824CCEE75C0D01C24550E672C6CA7F4908808`.
- [APK signature](Certificate.txt): v2 verified, one signer, same existing release certificate (SHA-1 `BB4249744A4FFA4C2AC081CA53B9E4ECB1AEFC28`).
- [Source/content/config hashes](Source-Hashes.txt), [79 startup/account fixture checks](Startup-Account.txt).
- The phone screenshot before the update was black. No visible gameplay or physical restart success is inferred from that capture; the user was asked to unlock it.

## Installation and pending visible acceptance

`adb install -r` returned **Success**. No data clear, profile conversion or signing change was performed. The subsequent activity launch started process 11829, but Android reported `mWakefulness=Asleep`, with the last sleep caused by the power button. Visible launch and retained-progress acceptance on this exact binary remain pending the user's unlock. The earlier candidate's successful physical restart remains historical evidence in `ClassicPresentation/DeviceTest.md`.

A bounded `Unity`/`AndroidRuntime` log inspection for process 11829 found no `Exception`, `FATAL`, `Error` or `Failed` matches. This does not prove visible startup, real sign-in, or correct rendering while asleep.

REL-13 has completed local/native durability implementation and tests, but its checkbox remains open for the updated candidate's visible phone restart check. REL-12 remains open for remote player/entitlement inventory; REL-14–15 for live console/configuration and real sign-in; REL-16 for physical two-device acceptance. The fixture and emulator reports do not close these external checks.
