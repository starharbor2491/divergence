#!/usr/bin/env bash
# Compiles the project headless. Exits non-zero on compile errors.
set -uo pipefail
source "$(dirname "$0")/unity-env.sh" || exit 1
LOG="$ROOT/Builds/logs/compile.log"
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" -logFile "$LOG" -accept-apiupdate
CODE=$?
if grep -E "error CS[0-9]+" "$LOG" | sort -u; then echo "Compile FAILED (log: $LOG)"; exit 1; fi
[ $CODE -ne 0 ] && { echo "Unity exited with code $CODE (log: $LOG)"; exit $CODE; }
echo "Compile OK"
