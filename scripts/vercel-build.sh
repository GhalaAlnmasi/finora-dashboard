#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1

install_sdk() {
  local install_dir="$ROOT/.dotnet"
  echo "Installing .NET 10 SDK into ${install_dir}"
  curl -fsSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --install-dir "$install_dir"
  export DOTNET_ROOT="$install_dir"
  export PATH="$DOTNET_ROOT:$PATH"
}

sdk_is_net10() {
  command -v dotnet >/dev/null 2>&1 || return 1
  local version
  version="$(dotnet --version 2>/dev/null || true)"
  [[ "$version" == 10.* ]]
}

if sdk_is_net10; then
  echo "Using existing .NET SDK $(dotnet --version)"
else
  install_sdk
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet was not found on PATH after SDK install." >&2
  exit 1
fi

echo "dotnet $(dotnet --version)"
dotnet test finora-dashboard.Tests/finora-dashboard.Tests.csproj --configuration Release
dotnet publish finora-dashboard.csproj -c Release -o publish

test -f publish/wwwroot/index.html
test -d publish/wwwroot/_framework
test -f publish/wwwroot/css/app.css
test -f publish/wwwroot/js/finora.js
