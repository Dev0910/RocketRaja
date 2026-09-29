# Firebase Unity SDK

Local SDK root: `C:\Unity\Assets\firebase_unity_sdk_13.17.0`

Unity packages: `C:\Unity\Assets\firebase_unity_sdk_13.17.0\firebase_unity_sdk`

Selected version: **13.17.0**. Use this location for future imports; keep Firebase modules on the same version.

## Selected modules

| Package | Purpose |
| --- | --- |
| FirebaseAnalytics | Gameplay and retention measurement |
| FirebaseCrashlytics | Crash reporting |
| FirebaseRemoteConfig | Remote balance and feature configuration |
| FirebaseAuth | Player identity for future account/cloud features |
| FirebaseFirestore | Future cloud save storage |
| FirebaseAppCheck | App attestation for supported backend services |

SDK imports alone do not enable services. Google authentication and Firestore cloud-save backend code now exists; see `Documentation/CloudSaves.md` for configuration, test evidence and the completed Bootstrap scene integration and pending device validation. Guest saves remain local. Custom analytics, Remote Config consumption and App Check provider initialization are not implemented by the cloud-login feature.

Messaging, Functions, Storage, Realtime Database, and AI Logic are deferred until a feature needs them. Shared Firebase App dependencies are included with the selected packages.

The project already includes `Assets/google-services.json`; do not replace it with a sample configuration. Android service connectivity and production configuration need a device validation when the services are integrated.

## Dependency manager

External Dependency Manager **1.2.189** is shared with AdMob as an embedded UPM package at `Packages/com.google.external-dependency-manager`. Firebase unitypackages also bundle a copy under `Assets/ExternalDependencyManager`; exclude that copy on future imports to avoid duplicate editor assemblies. Redundant initial imports were moved to `Temp/FirebaseImportBackup*` outside the asset database.

After removing a duplicate installation, refresh the asset database and force a recursive reimport of `Packages/com.google.external-dependency-manager`. Unity can otherwise retain stale GUID-to-path mappings to the removed DLLs, causing AdMob's editor scripts to report CS0246 for `GooglePlayServices` even though the shared resolver DLL exists and is enabled.

## Google login signing certificates

The Android OAuth registration must match the certificate of the APK installed on the device, including release builds. On 29 September 2026, the POCO X2 release APK showed the account chooser but returned to Welcome: Google Play services reported that the Android application was not registered for OAuth 2.0. Only the debug certificate had been registered.

The existing Firebase Android app (`com.diwali.fireworkstycoon.dev`) now also has these release fingerprints, verified from the installed APK with `apksigner verify --print-certs`:

- SHA-1: `BB:42:49:74:4A:4F:FA:4C:2A:C0:81:CA:53:B9:E4:EC:B1:AE:FC:28`
- SHA-256: `5B:13:3F:14:69:43:59:7B:93:68:87:44:CE:C6:49:13:0D:B2:59:9E:C0:9A:99:BE:07:F5:8D:A9:0E:80:32:86`

The existing debug fingerprints remain registered. The web OAuth client ID did not change; the installed APK signed in successfully without rebuilding after this server-side correction. Firebase Auth reported success and Firestore created the account document in `user`, advancing to cloud revision 7. Register the actual Play App Signing certificate separately before distributing through Google Play. Do not replace an installed release APK with a differently signed debug APK or clear its data to work around login failures.

The console configuration download did not complete in the automation browser, so `Assets/google-services.json` was not overwritten. Its existing web client remains valid and is the client consumed by Credential Manager.

## Android repository paths containing spaces (Gradle)

EDM4U generates a `file:///` repository URL containing the Unity project path. Gradle 9 rejects the unescaped space in `Incremental game`. The settings template encodes spaces as `%20`, and `Assets/Editor/FirebaseRepositoryUriFix.cs` reapplies this correction to generated Gradle settings before building if dependency resolution has regenerated the template. Keep this callback while using the current resolver version.
