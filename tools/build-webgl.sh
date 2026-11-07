#!/usr/bin/env bash
# Builds the WebGL player to Builds/WebGL.
set -uo pipefail
source "$(dirname "$0")/unity-env.sh" || exit 1
LOG="$ROOT/Builds/logs/build-webgl.log"
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" -buildTarget WebGL -executeMethod Evo.Editor.BuildScript.BuildWebGL -logFile "$LOG"
CODE=$?
if [ $CODE -ne 0 ]; then grep -E "error CS[0-9]+|BuildScript|Build failed" "$LOG" | sort -u | head -20; echo "Build FAILED (log: $LOG)"; exit $CODE; fi
echo "Build OK: $ROOT/Builds/WebGL"
