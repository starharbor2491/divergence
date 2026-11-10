#!/usr/bin/env bash
# Headless experiment runner (PLAN 17.3): tools/experiment.sh <preset-or-seed> <minutes> [events-file]
# Needs a .NET 8 SDK, from PATH or an installed Unity editor.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
if [ $# -lt 1 ]; then
  echo "Usage: tools/experiment.sh <preset-or-seed> [minutes] [events-file]" >&2
  exit 2
fi
export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
DOTNET="$(command -v dotnet || true)"
has_sdk() {
  [ -n "$1" ] && "$1" --list-sdks 2>/dev/null | grep -Eq '^8\.'
}
if ! has_sdk "$DOTNET"; then
  DOTNET=""
  for candidate in /Applications/Unity/Hub/Editor/*/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet; do
    if [ -x "$candidate" ] && has_sdk "$candidate"; then
      DOTNET="$candidate"
      break
    fi
  done
fi
if [ -z "$DOTNET" ]; then
  echo "A .NET 8 SDK is required. Install it and put dotnet on PATH; Unity's NetCoreRuntime alone is not enough." >&2
  exit 1
fi
"$DOTNET" build -c Release "$ROOT/tools/headless/Headless.csproj" -v q
OUT="$ROOT/Builds/experiments/${1// /-}-$(date +%H%M%S)"
ARGS=(run "$1" "${2:-20}" 120 "out=$OUT")
[ -n "${3:-}" ] && ARGS+=("events=$3")
"$DOTNET" "$ROOT/tools/headless/bin/Release/net8.0/Headless.dll" "${ARGS[@]}"
