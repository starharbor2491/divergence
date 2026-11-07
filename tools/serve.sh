#!/usr/bin/env bash
# Serves Builds/WebGL at http://localhost:8000
cd "$(dirname "$0")/../Builds/WebGL" && exec python3 -m http.server "${1:-8000}"
