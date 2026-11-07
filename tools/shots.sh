#!/usr/bin/env bash
# Builds a macOS dev player and runs the scripted screenshot tour (DevHarness) into Builds/shots.
set -uo pipefail
source "$(dirname "$0")/unity-env.sh" || exit 1
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" -executeMethod Evo.Editor.DevBuild.BuildMac -logFile "$ROOT/Builds/logs/devbuild.log"
if ! grep -q "DevBuild: Succeeded" "$ROOT/Builds/logs/devbuild.log"; then
  grep -E "error CS[0-9]+" "$ROOT/Builds/logs/devbuild.log" | sort -u; echo "Dev build FAILED"; exit 1
fi
rm -rf "$ROOT/Builds/shots"
timeout 240 "$ROOT/Builds/MacDev/Divergence.app/Contents/MacOS/Divergence" -screen-width "${1:-1920}" -screen-height "${2:-1080}" -screen-fullscreen 0 \
  -evoShots "$ROOT/Builds/shots" -logFile "$ROOT/Builds/logs/player.log" > /dev/null 2>&1
echo "Screenshots: $ROOT/Builds/shots"; ls "$ROOT/Builds/shots"
