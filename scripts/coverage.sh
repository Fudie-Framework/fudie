#!/bin/bash
# Usage: ./coverage.sh <TestProjectName>
# Example: ./coverage.sh Fudie.Domain.UnitTests

set -e

PROJECT="$1"

if [ -z "$PROJECT" ]; then
  echo "Usage: ./coverage.sh <TestProjectName>"
  echo "Example: ./coverage.sh Fudie.Domain.UnitTests"
  exit 1
fi

# Extract package name: Fudie.Domain.UnitTests -> Fudie.Domain
PACKAGE="${PROJECT%.UnitTests}"

TEST_DIR="tests/$PROJECT"
RESULTS_DIR="$TEST_DIR/TestResults"

# Clean previous results
rm -rf "$RESULTS_DIR"

# Run tests with coverage
echo "Running tests for $PROJECT..."
dotnet test "$TEST_DIR/$PROJECT.csproj" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR" \
  --verbosity minimal \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[$PACKAGE]*"

# Find the coverage file
COVERAGE_FILE=$(find "$RESULTS_DIR" -name "coverage.cobertura.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
  echo "ERROR: No coverage file found"
  exit 1
fi

# Generate HTML report with risk hotspots (CRAP score + complexity)
echo ""
echo "Generating HTML report..."
reportgenerator \
  "-reports:$COVERAGE_FILE" \
  "-targetdir:$RESULTS_DIR/CoverageReport" \
  "-reporttypes:Html;TextSummary" \
  "-assemblyfilters:+$PACKAGE"

# Print text summary
echo ""
cat "$RESULTS_DIR/CoverageReport/Summary.txt"

# Print method-level detail
echo ""
echo "=== Method Detail ==="
grep -E 'package name=|class name=|method name=' "$COVERAGE_FILE" \
  | sed -n "/package name=\"$PACKAGE\"/,/package name=/p" \
  | grep -E 'class name=|method name=' \
  | awk '
/class name=/ {
  match($0, /name="([^"]*)"/, a)
  match($0, /line-rate="([^"]*)"/, lr)
  match($0, /branch-rate="([^"]*)"/, br)
  match($0, /complexity="([^"]*)"/, cx)
  printf "\n  %s  Line:%d%%  Branch:%d%%  Complexity:%s\n", a[1], lr[1]*100, br[1]*100, cx[1]
}
/method name=/ {
  match($0, /name="([^"]*)"/, a)
  match($0, /line-rate="([^"]*)"/, lr)
  match($0, /branch-rate="([^"]*)"/, br)
  match($0, /complexity="([^"]*)"/, cx)
  printf "    %-30s Line:%3d%%  Branch:%3d%%  Complexity:%s\n", a[1], lr[1]*100, br[1]*100, cx[1]
}'

# Open HTML report
echo ""
echo "Opening HTML report..."
REPORT_PATH="$RESULTS_DIR/CoverageReport/index.html"

if command -v xdg-open &>/dev/null; then
  xdg-open "$REPORT_PATH"
elif command -v open &>/dev/null; then
  open "$REPORT_PATH"
else
  start "" "$REPORT_PATH"
fi