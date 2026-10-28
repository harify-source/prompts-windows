$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

dotnet publish .\src\PromptsWindows\PromptsWindows.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -o .\dist\Prompts
if ($LASTEXITCODE -ne 0) {
  throw "dotnet publish failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path .\dist\Prompts)) {
  throw "Publish output missing: .\dist\Prompts"
}

New-Item -ItemType Directory -Force -Path .\dist | Out-Null
if (Test-Path .\dist\Prompts-windows.zip) {
  Remove-Item .\dist\Prompts-windows.zip -Force
}

Compress-Archive -Path .\dist\Prompts\* -DestinationPath .\dist\Prompts-windows.zip -Force
Write-Host "Built $($Root)\dist\Prompts-windows.zip"
