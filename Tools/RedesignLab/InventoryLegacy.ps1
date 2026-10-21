param([string]$ProfileRoot = 'C:/Users/Dev/AppData/LocalLow/Diwali Games/Rocket Raja')
$ErrorActionPreference = 'Stop'
$report = [Collections.Generic.List[string]]::new()
$report.Add('# Local legacy inventory (sanitized)')
$report.Add('')
$report.Add('Read-only metadata inspection, 1 October 2026. No user identifiers, names, balances, tokens or raw save contents are exported. Remote production records are not accessed. Synthetic checkpoint fixtures cover cloud transitions.')
foreach ($file in Get-ChildItem -LiteralPath $ProfileRoot -File -Recurse | Where-Object { $_.Name -like '*.save*' }) {
    $raw = [IO.File]::ReadAllText($file.FullName)
    $newline = $raw.IndexOf("`n")
    if ($newline -lt 1) { $report.Add('- Unrecognized save envelope, preserved.'); continue }
    try {
        $value = $raw.Substring($newline + 1) | ConvertFrom-Json
        if ($null -ne $value.products) {
            $report.Add("- Local $($file.Extension) file: schema=$($value.schema), economy=$($value.economy), products=$($value.products.Count), upgrades=$($value.upgrades.Count), Legacy slots=$($value.legacy.Count), skills=$($value.skills.Count), hasName=$(-not [string]::IsNullOrEmpty($value.storeName)), hasSponsor=$($value.sponsorUntil -gt 0), adReceipts=$($value.claimedRewards.Count).")
        } elseif ($null -ne $value.save) {
            $report.Add("- Account checkpoint: envelope=$($value.schema), pending=$($value.pending), hasFollowup=$($null -ne $value.followup).")
        } else { $report.Add('- Unrecognized JSON shape, preserved.'); }
    } catch { $report.Add('- Unreadable JSON, preserved.'); }
}
$report | Set-Content -LiteralPath (Join-Path $PSScriptRoot '../../Documentation/Implementation/Legacy-Inventory.md')
$report
