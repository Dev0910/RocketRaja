"""Inspect a local-only validation APK without installing it or accessing a device."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import zipfile

parser = argparse.ArgumentParser()
parser.add_argument("apk", type=Path)
parser.add_argument("--aapt", type=Path, required=True)
parser.add_argument("--report", type=Path, required=True)
parser.add_argument("--dependency-report", type=Path, required=True)
args = parser.parse_args()
apk = args.apk.resolve()
with apk.open("rb") as stream:
    digest = hashlib.file_digest(stream, "sha256").hexdigest()

markers = {
    "gameGoogleBridge": b"Lcom/rocketrajagame/identity/GoogleSignInBridge;",
    "googleIdLibrary": b"Lcom/google/android/libraries/identity/googleid/GoogleIdTokenCredential;",
    "credentialManager": b"Landroidx/credentials/CredentialManager;",
    "rewardedAdsSdk": b"Lcom/google/android/gms/ads/rewarded/RewardedAd;",
}
found = {name: False for name in markers}
with zipfile.ZipFile(apk) as archive:
    entries = archive.namelist()
    for entry in entries:
        if entry.endswith(".dex"):
            data = archive.read(entry)
            for name, marker in markers.items():
                found[name] |= marker in data
    native = [entry for entry in entries if entry.startswith("lib/") and entry.endswith(".so")]

badging = subprocess.run([str(args.aapt), "dump", "badging", str(apk)], capture_output=True, text=True, check=True).stdout
root = Path(__file__).resolve().parents[2]
gradle = (root / "Assets/Plugins/Android/mainTemplate.gradle").read_text(encoding="utf-8")
dependencies = args.dependency_report.read_text(encoding="utf-8")
checks = {
    "gameGoogleBridgeAbsent": not found["gameGoogleBridge"],
    "directLoginDependenciesAbsent": "androidx.credentials:" not in gradle and "identity.googleid:" not in gradle,
    "onlyRetainedSdkGoogleIdVersion": "com.google.android.libraries.identity.googleid:googleid:1.1.0" in dependencies and "com.google.android.libraries.identity.googleid:googleid:1.2.0" not in dependencies,
    "rewardedAdsSdkPresent": found["rewardedAdsSdk"],
    "arm64PlayerPresent": "lib/arm64-v8a/libil2cpp.so" in native,
}
result = {
    "apk": str(apk), "bytes": apk.stat().st_size, "sha256": digest,
    "package": next(line for line in badging.splitlines() if line.startswith("package:")),
    "debuggable": "application-debuggable" in badging,
    "dexMarkers": found, "checks": checks,
    "dependencyReport": str(args.dependency_report),
    "retainedSdkDependencyChain": ["com.google.firebase:firebase-auth:24.2.0", "androidx.credentials:credentials-play-services-auth:1.2.0-rc01", "com.google.android.libraries.identity.googleid:googleid:1.1.0"],
    "sdkDependencyNote": "Firebase Auth 24.2.0 depends on Credential Manager; its Play services implementation depends on Google ID 1.1.0. These transitive libraries remain with the explicitly retained SDK. The game's direct Google ID 1.2.0 / Credential Manager 1.6.0 declarations and sign-in bridge are removed; no production sign-in entry point exists.",
    "nativeLibraries": native,
    "passed": all(checks.values()),
}
args.report.parent.mkdir(parents=True, exist_ok=True)
args.report.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"passed": result["passed"], "checks": checks, "bytes": result["bytes"], "sha256": digest}, indent=2))
if not result["passed"]:
    raise SystemExit(1)
