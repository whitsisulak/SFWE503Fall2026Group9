#!/usr/bin/env bash
#
# Stands in for a lab analyzer writing a result file.
# Usage:
#   ./scripts/simulate-instrument.sh          # a well-formed file
#   ./scripts/simulate-instrument.sh --bad    # includes malformed lines
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DROP="$ROOT/instrument-drop"
STAMP="$(date +%Y%m%d%H%M%S)"
NOW="$(date +%Y%m%d%H%M)"

mkdir -p "$DROP"

if [[ "${1:-}" == "--bad" ]]; then
  FILE="$DROP/ANALYZER-02_${STAMP}.txt"
  cat > "$FILE" <<EOF
1234|Test^B123^T3 Uptake|31|${NOW}
this line is not a valid message
9999|Test^X999^Unknown Barcode|12|${NOW}
5678|Test^U123^Nitrite|Trace|not-a-timestamp
EOF
  echo "Dropped malformed file: $FILE"
  echo "Expect it in instrument-drop/_failed/ with a .error.txt beside it."
else
  FILE="$DROP/ANALYZER-01_${STAMP}.txt"
  cat > "$FILE" <<EOF
1234|Test^B123^T3 Uptake|28|${NOW}
1234|Test^B124^TSH|2.1|${NOW}
5678|Test^U123^Nitrite|Absent|${NOW}
5678|Test^U124^Leukocytes|Negative|${NOW}
EOF
  echo "Dropped file: $FILE"
  echo "Expect it in instrument-drop/_processed/ within ~5s, and results on /orders."
fi
