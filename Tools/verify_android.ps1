param(
    [string]$UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.4.9f1/Editor',
    [string]$ApkPath = (Join-Path $PSScriptRoot '../Builds/Android/RocketRaja-test.apk')
)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$apkFile = (Resolve-Path -LiteralPath $ApkPath).Path
$androidRoot = Join-Path $UnityPath 'Data/PlaybackEngines/AndroidPlayer'
$buildTools = Join-Path $androidRoot 'SDK/build-tools/36.0.0'
$report = [Collections.Generic.List[string]]::new()
$report.Add("APK: $apkFile")
$report.Add("Bytes: $((Get-Item -LiteralPath $apkFile).Length)")
$report.Add("SHA256: $((Get-FileHash -LiteralPath $apkFile -Algorithm SHA256).Hash)")
$alignment = & (Join-Path $buildTools 'zipalign.exe') -c -P 16 -v 4 $apkFile
if ($LASTEXITCODE -ne 0) { throw 'APK alignment validation failed' }
$report.Add('PASS APK ZIP alignment with 16 KB native-library pages')
$signature = & (Join-Path $androidRoot 'OpenJDK/bin/java.exe') -jar (Join-Path $buildTools 'lib/apksigner.jar') verify --verbose $apkFile
if ($LASTEXITCODE -ne 0) { throw 'APK signature validation failed' }
$report.AddRange([string[]]$signature)
$metadata = & (Join-Path $buildTools 'aapt2.exe') dump badging $apkFile
if ($LASTEXITCODE -ne 0) { throw 'APK manifest inspection failed' }
$report.AddRange([string[]]($metadata | Select-String "package:|SdkVersion|sdkVersion|native-code|application-label:" | ForEach-Object { $_.Line }))
$outputRoot = Join-Path $projectRoot 'Builds/Validation/Native'
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
$archive = [IO.Compression.ZipFile]::OpenRead($apkFile)
try {
    foreach ($entry in $archive.Entries) {
        if ($entry.FullName -notlike 'lib/arm64-v8a/*.so') { continue }
        $nativePath = Join-Path $outputRoot ([IO.Path]::GetFileName($entry.FullName))
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $nativePath, $true)
        $headers = & (Join-Path $androidRoot 'NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-readelf.exe') -lW $nativePath
        if ($LASTEXITCODE -ne 0) { throw "ELF inspection failed: $nativePath" }
        $segments = $headers | Where-Object { $_ -match '^\s*LOAD\s' }
        foreach ($segment in $segments) {
            $hex = ($segment.Trim() -split '\s+')[-1]
            if ([Convert]::ToInt32($hex.Replace('0x', ''), 16) -lt 16384) { throw "Native segment not 16 KB compatible: $nativePath" }
        }
        $report.Add("PASS 16 KB ELF load segments: $($entry.FullName)")
    }
} finally { $archive.Dispose() }
$report | Set-Content -LiteralPath (Join-Path $projectRoot 'Documentation/AndroidPackageChecks.txt')
$report
