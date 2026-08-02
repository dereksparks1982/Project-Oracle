#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

expected_version='0.1.1'

if ! command -v dotnet >/dev/null 2>&1; then
  echo "VALIDATION BLOCKED: Project Oracle v${expected_version} needs the .NET 10 SDK." >&2
  echo "The dotnet command was not found. No build or tests were run." >&2
  exit 2
fi

sdk_major="$(dotnet --version | cut -d. -f1)"
if [[ "$sdk_major" != '10' ]]; then
  echo "VALIDATION BLOCKED: Expected .NET SDK 10.x but found $(dotnet --version)." >&2
  exit 2
fi

echo "PHASE START: restore"
dotnet restore ProjectOracle.sln
echo "PHASE PASS: restore"

echo "PHASE START: warnings-as-errors build"
dotnet build ProjectOracle.sln --configuration Release --no-restore
echo "PHASE PASS: warnings-as-errors build"

echo "PHASE START: acceptance tests"
dotnet run --project tests/ProjectOracle.AcceptanceTests/ProjectOracle.AcceptanceTests.csproj --configuration Release --no-build
echo "PHASE PASS: acceptance tests"

echo "PHASE START: console smoke test"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$validation_temp"' EXIT
smoke_output="$(dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --seed 104729 --save "$validation_temp/save.json" --once)"
grep -Fq 'Project Oracle v0.1.1' <<<"$smoke_output"
grep -Fq 'Adam awoke in the Garden.' <<<"$smoke_output"
if grep -Fq 'Yala' <<<"$smoke_output"; then
  echo "VALIDATION FAIL: Yala's true name leaked into default world output." >&2
  exit 1
fi
echo "PHASE PASS: console smoke test"

echo "VALIDATION PASS: Project Oracle v${expected_version}"
