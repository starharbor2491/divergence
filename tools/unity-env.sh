#!/usr/bin/env bash
# Finds the pinned Unity editor and exports UNITY, PROJECT and ROOT. Source this file.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/EvoSim"
VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt" | head -1)"
UNITY="/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity"
if [ ! -x "$UNITY" ]; then
  echo "Unity $VERSION is not installed at $UNITY" >&2
  exit 1
fi
mkdir -p "$ROOT/Builds/logs"
export ROOT PROJECT VERSION UNITY
