#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

expected_version='0.1.7'

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

echo "PHASE START: Company Bible check"
[[ -f docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md ]]
grep -Fq 'No guessing past the Company Bible.' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
grep -Fq 'Physical function keys mean physical function keys.' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
echo "PHASE PASS: Company Bible check"

echo "PHASE START: physical function-key source check"
grep -Fq 'Console.ReadKey(intercept: true)' src/ProjectOracle.Console/Program.cs
if grep -Fq '"f1" or' src/ProjectOracle.Console/Program.cs; then
  echo "VALIDATION FAIL: typed f1 still switches address channels." >&2
  exit 1
fi
grep -Fq 'PhysicalFunctionKeysSelectAddressChannels' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'Version014SaveUpgradesThroughCurrentDefaults' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: physical function-key source check"

echo "PHASE START: deterministic event queue source check"
grep -Fq 'public sealed record ScheduledWorldEvent' src/ProjectOracle.Core/Events/ScheduledWorldEvent.cs
grep -Fq 'public sealed record OfferedChoiceState' src/ProjectOracle.Core/Events/OfferedChoiceState.cs
grep -Fq 'ProcessDueEvents' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'VesselSpeechOffersDeterministicAdamChoices' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'OfferedChoicesSurviveSaveAndRestore' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: deterministic event queue source check"

echo "PHASE START: console smoke test"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$validation_temp"' EXIT
smoke_output="$(dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --seed 104729 --save "$validation_temp/save.json" --once)"
grep -Fq 'Project Oracle v0.1.7' <<<"$smoke_output"
grep -Fq 'The Garden was formed and filled with ancient living kinds.' <<<"$smoke_output"
grep -Fq 'World Seed: 104729' <<<"$smoke_output"
if grep -Fq 'Yala' <<<"$smoke_output"; then
  echo "VALIDATION FAIL: Yala's true name leaked into default world output." >&2
  exit 1
fi
echo "PHASE PASS: console smoke test"

echo "PHASE START: live console launcher checks"
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.1.7 - Live Garden Console'
echo "PHASE PASS: live console launcher checks"

echo "VALIDATION PASS: Project Oracle v${expected_version}"
