$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

dotnet publish .\src\PromptsWindows\PromptsWindows.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -o .\dist\Prompts

if (Test-Path .\dist\Prompts-windows.zip) {
  Remove-Item .\dist\Prompts-windows.zip -Force
}

Compress-Archive -Path .\dist\Prompts\* -DestinationPath .\dist\Prompts-windows.zip -Force
Write-Host "Built $($Root)\dist\Prompts-windows.zip"
