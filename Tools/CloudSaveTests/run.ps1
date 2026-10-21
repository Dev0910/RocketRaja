param([string] $JavaHome = '')
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $JavaHome) {
    $runtimePointer = Join-Path $projectRoot 'Library/RedesignTestRuntime/java-home.txt'
    if (Test-Path -LiteralPath $runtimePointer) { $JavaHome = (Get-Content -LiteralPath $runtimePointer -Raw).Trim() }
}
if (-not $JavaHome) {
    $cachedJava = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Temp/CloudTestRuntime') -Directory -ErrorAction SilentlyContinue | Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'bin/java.exe') } | Select-Object -First 1
    if ($cachedJava) { $JavaHome = $cachedJava.FullName }
}
if ($JavaHome) {
    if (-not (Test-Path -LiteralPath (Join-Path $JavaHome 'bin/java.exe'))) { throw 'Configured Java runtime is missing; see Documentation/Implementation/JavaTestRuntime.json.' }
    $env:JAVA_HOME = $JavaHome; $env:Path = "$JavaHome/bin;$env:Path"
}
Push-Location $projectRoot
try {
    & (Join-Path $PSScriptRoot 'node_modules/.bin/firebase.cmd') emulators:exec --only firestore --project demo-rocket-raja --config firebase.json 'node --test --test-concurrency=1 Tools/CloudSaveTests/rules.test.cjs Backend/Premium/firestore.integration.test.mjs'
    if ($LASTEXITCODE -ne 0) { throw "Firebase rule tests failed with exit code $LASTEXITCODE" }
} finally { Pop-Location }
