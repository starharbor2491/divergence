#!/usr/bin/env bash
# Runs the EditMode tests headless and prints a summary from the XML report.
set -uo pipefail
source "$(dirname "$0")/unity-env.sh" || exit 1
LOG="$ROOT/Builds/logs/test.log"
RESULTS="$ROOT/Builds/test-results.xml"
rm -f "$RESULTS"
"$UNITY" -runTests -batchmode -nographics -projectPath "$PROJECT" -testPlatform EditMode -testResults "$RESULTS" -logFile "$LOG"
if [ ! -f "$RESULTS" ]; then grep -E "error CS[0-9]+" "$LOG" | sort -u; echo "No results file (log: $LOG)"; exit 2; fi
python3 - "$RESULTS" <<'PY'
import sys, xml.etree.ElementTree as ET
r = ET.parse(sys.argv[1]).getroot()
print("tests: total=%s passed=%s failed=%s skipped=%s" % (r.get("total"), r.get("passed"), r.get("failed"), r.get("skipped")))
bad = 0
for c in r.iter("test-case"):
    if c.get("result") == "Failed":
        bad += 1
        print("FAILED", c.get("fullname"))
        m = c.find("failure/message")
        if m is not None and m.text: print("   ", m.text.strip().replace("\n", "\n    ")[:600])
sys.exit(1 if bad else 0)
PY
