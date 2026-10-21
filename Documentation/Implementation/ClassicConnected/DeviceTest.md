# Connected Classic Android checks — 6 October 2026

Device: POCO X2, Android device `31920616`. User-confirmed package: `com.diwali.fireworkstycoon.dev`. The separate `com.diwali.rocketrajaclassic.playtest` installation was retained. No uninstall, data clear, signing workaround, live account mutation, or purchase was performed.

The first connected APK installed after the user unlocked the phone and accepted installation. A force-stop followed by launcher activity start reported `LaunchState: COLD`, `Status: ok`, 1,417 ms activity launch time (not a measurement of game readiness). [Cold-launch capture](AndroidCold.png) shows the actual Bootstrap welcome and normal profile choice.

The phone denies automated taps with `INJECT_EVENTS permission`; UI actions were performed by the user. After Play as guest, the [actual recovery screen](AndroidLegacyRecovery.png) reported legacy progress in the package's persistent profile. It blocked gameplay instead of silently creating a fresh save. Legacy data was present despite the package not appearing in the initial installed-package list; the data restoration mechanism was not independently verified.

The user explicitly requested: “Preserve old save; allow a separate fresh Classic profile.” A new confirmed choice in Bootstrap now selects the normal unboosted `classic-v2` profile while retaining the legacy source. No existing save is moved, converted, deleted or overwritten by this choice. Editor tests verify the confirmation boundary and unchanged legacy bytes.

The first artifact reported a successful build with one exception from an obsolete disposable-profile assertion; [diagnostic record](FirstBuild-Diagnostic.txt). The assertion was updated for the selected persistent profile, and the connected builder now treats any build error as failure. Use the final [build record](../Classic-Connected-Android-Build.txt) for the rebuilt candidate's status.

Store installation is **not verified**. The user confirmed no Google Play test build is available yet for this package. Direct APK installation is not store-install evidence. Live Google configuration remains disabled, and real account/billing/device-handoff acceptance remains open.

## Rebuilt candidate

- Built successfully in 2m 52.8s, **zero errors**, 30 warnings (package/obsolete API, shader and icon warnings retained in the build report).
- APK: `Builds/Android/RocketRaja-classic-connected.apk`.
- SHA-256: `2B2ED0B6A18E49AA1A3A811B27B9445780B58CF82D1A54B1E62CB8FDCE69D887`.
- APK v2 signature verification passed, one signer.
- Updated the existing package successfully with `adb install -r`, without clearing data.
- Rebuilt package cold activity launch: `Status: ok`, `LaunchState: COLD`, 1,288 ms activity launch time.
- Saved-scene reload audit: 2,352 Classic objects, zero missing scripts, 64 fixed particles, one active production binding, no preview component, no Play override. Active roots are Main Camera, Imported Event System and ClassicProductionCanvas; enabled build scenes are Bootstrap and Main.

## Connected gameplay observations

After the rebuilt installation, the user reported a missing option or error. The subsequent device capture showed active Classic gameplay, not a recovery overlay: Phuljhari level 2, income ₹1.7/second and a live cash balance. [Purchase evidence](AndroidPurchasedPhuljhari.png). This establishes that product purchases became available; it does not establish which earlier option or error the user encountered. The separate-profile creation confirmation and preservation of legacy bytes are covered by the native Editor checks above, not direct inspection of the phone's protected save files.

A filtered Unity/AndroidRuntime log read for running PID 32671 contained no matches for Exception, Error, FATAL or Failed. This is a bounded log observation, not proof that no earlier error occurred.

[Normal Editor Play](../Classic-Normal-Editor.txt), without fixture initialization or a Play override, entered Bootstrap and then the persistent guest's Classic Home with cash 30 and zero owned products, Fame, KK or cards. [Editor capture](EditorFreshHome.png).

The user then confirmed Settings → Account options → Play as guest returned with the same progress. [Account-return capture](AndroidAccountReturn.png) shows Phuljhari and Anar on the shelves and a live cash balance. The app was backgrounded to allow its normal departure save, force-stopped, and launched again through its launcher activity. Android reported `Status: ok`, `LaunchState: COLD`, 789 ms activity launch time. [Cold-return capture](AndroidPersistentCold.png) again shows both owned products and continuing cash, confirming persistent returning progress rather than a fresh reset. The post-restart Unity/AndroidRuntime log for PID 1910 had no Exception, Error, FATAL or Failed matches. Exact product counts, offline reward arithmetic and byte preservation on the device were not extracted from protected storage.
