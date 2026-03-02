#!/bin/bash
# Usage: ./parse_cov.sh <coverage.cobertura.xml> [filter]
# Example: ./parse_cov.sh tests/Fudie.Security.UnitTests/TestResults/*/coverage.cobertura.xml
# Example: ./parse_cov.sh coverage.cobertura.xml JwtValidator

set -e

COVERAGE_FILE="$1"
FILTER="$2"

if [ -z "$COVERAGE_FILE" ]; then
  echo "Usage: ./parse_cov.sh <coverage.cobertura.xml> [filter]"
  echo "  filter: optional class name filter (substring match)"
  exit 1
fi

if [ ! -f "$COVERAGE_FILE" ]; then
  echo "ERROR: File not found: $COVERAGE_FILE"
  exit 1
fi

awk -v filter="$FILTER" '
BEGIN {
  in_class = 0
  in_method = 0
  class_name = ""
  class_line_rate = ""
  class_branch_rate = ""
  method_name = ""
  method_line = ""
  has_issues = 0
  method_output = ""
}

/<class / {
  match($0, /name="([^"]*)"/, cn)
  match($0, /line-rate="([^"]*)"/, lr)
  match($0, /branch-rate="([^"]*)"/, br)

  class_name = cn[1]
  class_line_rate = lr[1]
  class_branch_rate = br[1]

  if (filter != "" && index(class_name, filter) == 0) {
    in_class = 0
    next
  }

  in_class = 1
  class_printed = 0
}

in_class && /<method / {
  match($0, /name="([^"]*)"/, mn)
  match($0, /line="([^"]*)"/, ml)
  method_name = mn[1]
  method_line = ml[1]
  in_method = 1
  has_issues = 0
  method_output = ""
}

in_class && in_method && /<line / {
  hits = ""
  cond = ""
  line_num = ""

  match($0, /number="([^"]*)"/, ln)
  match($0, /hits="([^"]*)"/, h)
  line_num = ln[1]
  hits = h[1]

  if (match($0, /condition-coverage="([^"]*)"/, cc)) {
    cond = cc[1]
  }

  if (hits == "0") {
    has_issues = 1
    method_output = method_output "    UNCOVERED line " line_num "\n"
  } else if (cond != "" && index(cond, "100%") == 0) {
    has_issues = 1
    method_output = method_output "    PARTIAL  line " line_num " branch=" cond "\n"
  }
}

in_class && in_method && /<\/method>/ {
  if (has_issues) {
    if (!class_printed) {
      printf "\n=== %s ===\n", class_name
      printf "Line rate: %.1f%%  Branch rate: %.1f%%\n", class_line_rate * 100, class_branch_rate * 100
      class_printed = 1
    }
    if (method_line != "")
      printf "\n  Method: %s (line %s)\n", method_name, method_line
    else
      printf "\n  Method: %s\n", method_name
    printf "%s", method_output
  }
  in_method = 0
}

/<\/class>/ {
  in_class = 0
}
' "$COVERAGE_FILE"
