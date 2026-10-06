param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.4.9f1/Editor/Unity.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$qaRoot = Join-Path $projectRoot ('Temp/RedesignDeviceQA-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8))
if (-not (Test-Path -LiteralPath $Unity)) { throw 'Configured Unity editor does not exist' }
foreach ($folder in @('Assets/Core', 'Assets/Editor', 'Assets/Resources', 'Packages', 'ProjectSettings', 'Build')) {
    New-Item -ItemType Directory -Path (Join-Path $qaRoot $folder) -Force | Out-Null
}
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets/Game/Core') -Filter '*.cs' | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $qaRoot ('Assets/Core/' + $_.Name))
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DeviceCoreProbe.cs') -Destination (Join-Path $qaRoot 'Assets/DeviceCoreProbe.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DeviceQABuild.cs') -Destination (Join-Path $qaRoot 'Assets/Editor/DeviceQABuild.cs')
Copy-Item -LiteralPath (Join-Path $projectRoot 'Documentation/RedesignContentCatalog.json') -Destination (Join-Path $qaRoot 'Assets/Resources/RedesignContentCatalog.json')
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $qaRoot 'ProjectSettings/ProjectVersion.txt')
[IO.File]::WriteAllText((Join-Path $qaRoot 'Packages/manifest.json'), '{"dependencies":{"com.unity.modules.jsonserialize":"1.0.0","com.unity.modules.androidjni":"1.0.0"}}')
[IO.File]::WriteAllText((Join-Path $qaRoot 'DEVICE_QA_ONLY'), 'Isolated diagnostic; no game, Firebase, store or user data is included.')
$sourceHashes = @(Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Assets') -Recurse -File | ForEach-Object {
    [pscustomobject]@{ Path = $_.FullName.Substring($qaRoot.Length + 1).Replace('\','/'); SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
})
[IO.File]::WriteAllText((Join-Path $qaRoot 'Build/source-manifest.json'), (ConvertTo-Json -InputObject $sourceHashes -Depth 4))
$logPath = Join-Path $qaRoot 'Build/editor.log'
$arguments = @('-batchmode','-nographics','-quit','-buildTarget','Android','-projectPath',('"' + $qaRoot + '"'),'-executeMethod','DeviceQABuild.Android','-logFile',('"' + $logPath + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
[pscustomobject]@{ Project = $qaRoot; ProcessId = $process.Id; Log = $logPath; Apk = (Join-Path $qaRoot 'Build/RocketRaja-core-qa.apk') } | ConvertTo-Json
