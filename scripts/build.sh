#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != MINGW* && "$(uname -s)" != CYGWIN* && "$(uname -s)" != MSYS* ]]; then
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "dotnet SDK is required to build prompts-windows." >&2
    exit 1
  fi
  # WPF targets windows only; CI builds on windows-latest via build.ps1.
  echo "prompts-windows release builds require Windows. Use scripts/build.ps1 on Windows or GitHub Actions." >&2
  exit 1
fi

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$ROOT/scripts/build.ps1"
