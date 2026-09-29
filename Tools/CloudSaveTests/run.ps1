param([string] $JavaHome = '')
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $JavaHome) {
    $cachedJava = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Temp/CloudTestRuntime') -Directory -ErrorAction SilentlyContinue | Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'bin/java.exe') } | Select-Object -First 1
    if ($cachedJava) { $JavaHome = $cachedJava.FullName }
}
if ($JavaHome) { $env:JAVA_HOME = $JavaHome; $env:Path = "$JavaHome/bin;$env:Path" }
Push-Location $projectRoot
try {
    & (Join-Path $PSScriptRoot 'node_modules/.bin/firebase.cmd') emulators:exec --only firestore --project demo-rocket-raja --config firebase.json 'node --test Tools/CloudSaveTests/rules.test.cjs'
    if ($LASTEXITCODE -ne 0) { throw "Firebase rule tests failed with exit code $LASTEXITCODE" }
} finally { Pop-Location }
