#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK is required to run SitePolicyChecks." >&2
  exit 1
fi

dotnet run --project tests/SitePolicyChecks/SitePolicyChecks.csproj -c Release
