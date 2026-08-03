#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

expected_version='0.1.10'

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
grep -Fq 'Console.KeyAvailable' src/ProjectOracle.Console/Program.cs
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
grep -Fq 'public sealed record ReasonedPlanState' src/ProjectOracle.Core/Brain/ReasonedPlanState.cs
grep -Fq 'Oracle HTN Brain v0.1' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'ProcessDueEvents' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'VesselSpeechOffersDeterministicAdamChoices' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'OfferedChoicesSurviveSaveAndRestore' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'ReasonedPlansSurviveSaveAndRestore' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: deterministic event queue source check"

echo "PHASE START: first knowing source check"
grep -Fq 'OracleQuestionInterpreter' src/ProjectOracle.Console/OracleQuestionInterpreter.cs
grep -Fq 'Adam knows that he is.' src/ProjectOracle.Console/OracleQuestionInterpreter.cs
grep -Fq 'Mortality becomes real only when days are numbered' src/ProjectOracle.Console/OracleQuestionInterpreter.cs
if grep -Fq 'suitable mate found: {' src/ProjectOracle.Console/Program.cs; then
  echo "VALIDATION FAIL: raw suitable-mate boolean still leaks into status output." >&2
  exit 1
fi
echo "PHASE PASS: first knowing source check"

echo "PHASE START: creation powers source check"
grep -Fq 'public sealed record CreationPowerState' src/ProjectOracle.Core/Domain/WorldEntities.cs
grep -Fq 'new(0, new EntityId("condition:void:0001"), "Void"' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'The Creators threw Yala into the void' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'Sol' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'Thalassa' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'Created just before Adam' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'protected Creator Record outranks her claim' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'OracleAnswersCreationOrderQuestions' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'Version017SaveUpgradesThroughCurrentCreationPowers' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'Version018SaveUpgradesThroughCorrectedCreationPowers' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: creation powers source check"

echo "PHASE START: first brain planner source check"
grep -Fq 'PlanAdamDirectAddress' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'PlanAdamVesselSpeech' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'PlanAdamNaming' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'Adam reasons before response' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'Adam reasons before naming' src/ProjectOracle.Core/Brain/OracleBrainPlanner.cs
grep -Fq 'ReasonedPlans => _reasonedPlans.AsReadOnly()' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'plans / brain' src/ProjectOracle.Console/Program.cs
grep -Fq 'Adam naming creates a reasoned brain plan before the name record' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'direct address to Adam creates a reasoned brain plan before the choice' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: first brain planner source check"

echo "PHASE START: retro console theme source check"
grep -Fq 'internal static class ConsoleTheme' src/ProjectOracle.Console/ConsoleTheme.cs
grep -Fq 'NO_COLOR' src/ProjectOracle.Console/ConsoleTheme.cs
grep -Fq '38;5;46m' src/ProjectOracle.Console/ConsoleTheme.cs
grep -Fq 'WritePrompt' src/ProjectOracle.Console/ConsoleTheme.cs
grep -Fq 'LiveLine' src/ProjectOracle.Console/ConsoleTheme.cs
grep -Fq 'ConsoleTheme.ApplyBase()' src/ProjectOracle.Console/Program.cs
grep -Fq 'ConsoleTheme.WritePrompt' src/ProjectOracle.Console/Program.cs
grep -Fq 'ConsoleTheme.LiveLine(line)' src/ProjectOracle.Console/Program.cs
echo "PHASE PASS: retro console theme source check"

echo "PHASE START: console smoke test"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$validation_temp"' EXIT
smoke_output="$(dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --seed 104729 --save "$validation_temp/save.json" --once)"
grep -Fq 'Project Oracle v0.1.10' <<<"$smoke_output"
grep -Fq 'Live world time appears on the LIVE line below.' <<<"$smoke_output"
grep -Fq 'The Creators threw Yala into the void to see what she would do with her prison.' <<<"$smoke_output"
grep -Fq 'The Garden was created just before Adam as a closed preserve.' <<<"$smoke_output"
grep -Fq 'World Seed: 104729' <<<"$smoke_output"
if grep -Fq 'World time:' <<<"$smoke_output"; then
  echo "VALIDATION FAIL: static startup World time line still appears above the live display." >&2
  exit 1
fi
echo "PHASE PASS: console smoke test"

echo "PHASE START: live console launcher checks"
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.1.10 - Live Garden Console'
echo "PHASE PASS: live console launcher checks"

echo "VALIDATION PASS: Project Oracle v${expected_version}"
