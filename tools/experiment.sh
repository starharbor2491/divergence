#!/usr/bin/env bash
# Headless experiment runner (PLAN 17.3): tools/experiment.sh <preset-or-seed> <minutes> [events-file]
# Needs a .NET 8 SDK; the one inside the Unity editor is used when `dotnet` is not on the PATH.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
if ! command -v dotnet >/dev/null; then
  VERSION="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/EvoSim/ProjectSettings/ProjectVersion.txt" | head -1)"
  export PATH="/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/Resources/Scripting/DotNetSdk:$PATH"
fi
export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet build -c Release "$ROOT/tools/headless/Headless.csproj" -v q >/dev/null
OUT="$ROOT/Builds/experiments/${1// /-}-$(date +%H%M%S)"
ARGS=(run "$1" "${2:-20}" 120 "out=$OUT")
[ -n "${3:-}" ] && ARGS+=("events=$3")
dotnet "$ROOT/tools/headless/bin/Release/net8.0/Headless.dll" "${ARGS[@]}"
