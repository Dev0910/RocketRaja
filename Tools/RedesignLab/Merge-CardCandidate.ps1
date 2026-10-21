param(
    [Parameter(Mandatory=$true)][string[]]$FitPaths,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [switch]$AllowPartial
)
$ErrorActionPreference = 'Stop'
$baseline = Get-Content -LiteralPath 'Documentation/TapBalanceMatrix.json' -Raw | ConvertFrom-Json
$income = @(Get-Content -LiteralPath 'Documentation/FasterStartPrestigeFit.json' -Raw | ConvertFrom-Json)
$candidate = [ordered]@{
    description = 'Offline candidate; must pass a fresh full matrix before use in Unity.'
    catalogSourceHash = $baseline.catalogSourceHash
    cardCurve = $null
    handsPayout = $null
    cardGlobalGain = $null
    fameIncomeLogs = @($income)
    tapFameScales = @($baseline.tapFameScales)
    targetOffsetLogs = @($baseline.targetOffsetLogs)
    sources = @($FitPaths)
}
$covered = [Collections.Generic.HashSet[int]]::new()
foreach ($path in $FitPaths) {
    $data = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if ($data.catalogSourceHash -ne $baseline.catalogSourceHash) { throw "Source hash mismatch: $path" }
    $globalGain = if ($null -ne $data.cardGlobalGain) { $data.cardGlobalGain } elseif ($data.cardCurve -eq 'balanced') { 2.0 } elseif ($data.cardCurve -eq 'light') { 0.5 } else { 1.0 }
    if ($null -eq $candidate.cardCurve) {
        $candidate.cardCurve = $data.cardCurve; $candidate.handsPayout = $data.handsPayout; $candidate.cardGlobalGain = $globalGain
    } elseif ($candidate.cardCurve -ne $data.cardCurve -or $candidate.handsPayout -ne $data.handsPayout -or $candidate.cardGlobalGain -ne $globalGain) {
        throw "Cannot merge different card/Hands models: $path"
    }
    foreach ($fit in $data.fits) {
        if ($null -eq $fit.tapScale) { continue }
        $fame = [int]$fit.fame
        if ($fame -lt 0 -or $fame -gt 33) { throw "Unexpected fitted Fame: $fame" }
        $offset = if ($null -eq $fit.incomeOffset) { 0.0 } else { [double]$fit.incomeOffset }
        if ($fame -lt 2 -and $offset -ne 0) { throw 'Fame 0/1 have exact special income values; cannot apply an income-log override.' }
        $candidate.fameIncomeLogs[$fame] = $income[$fame] + $offset
        $candidate.tapFameScales[$fame] = $fit.tapScale
        $candidate.targetOffsetLogs[$fame] = [Math]::Round($fit.targetLog - $fit.current, 8)
        [void]$covered.Add($fame)
    }
}
$missing = @(0..33 | Where-Object { -not $covered.Contains($_) })
if ($missing.Count -gt 0 -and -not $AllowPartial) { throw "Unfitted stages: $($missing -join ', ')" }
$candidate.tapFameScales[34] = $candidate.tapFameScales[33]
$candidate.targetOffsetLogs[34] = $candidate.targetOffsetLogs[33]
$monotonicErrors = @()
for ($fame = 1; $fame -le 34; $fame++) {
    if ($candidate.fameIncomeLogs[$fame] -lt $candidate.fameIncomeLogs[$fame-1]) { $monotonicErrors += "Product Fame $fame" }
    $tap = $candidate.fameIncomeLogs[$fame] + [Math]::Log10($candidate.tapFameScales[$fame])
    $previousTap = $candidate.fameIncomeLogs[$fame-1] + [Math]::Log10($candidate.tapFameScales[$fame-1])
    if ($tap -lt $previousTap) { $monotonicErrors += "Tap Fame $fame" }
    $target = [Math]::Log10(3e23) + 5 * ($fame-1) + $candidate.targetOffsetLogs[$fame]
    $previousTarget = if ($fame -eq 1) { [Math]::Log10(6e21) + $candidate.targetOffsetLogs[0] } else { [Math]::Log10(3e23) + 5 * ($fame-2) + $candidate.targetOffsetLogs[$fame-1] }
    if ($target -le $previousTarget) { $monotonicErrors += "Prestige target $fame" }
}
$candidate.completeFitCoverage = $missing.Count -eq 0
$candidate.missingStages = $missing
$candidate.monotonicErrors = $monotonicErrors
if ($monotonicErrors.Count -gt 0 -and -not $AllowPartial) { throw "Non-monotonic candidate: $($monotonicErrors -join ', ')" }
$candidate | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath
Write-Output "Wrote candidate with $($covered.Count)/34 fitted stages; $($monotonicErrors.Count) monotonicity errors. Fresh replay is still required."
