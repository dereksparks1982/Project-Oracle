#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

expected_version='0.0.16'

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
set +e
test_output="$(dotnet run --project tests/ProjectOracle.AcceptanceTests/ProjectOracle.AcceptanceTests.csproj --configuration Release --no-build 2>&1)"
test_status=$?
set -e
printf '%s\n' "$test_output"
if [[ "$test_status" -ne 0 ]]; then
  echo "VALIDATION FAIL: acceptance tests exited with status $test_status." >&2
  exit "$test_status"
fi
grep -Fq 'Acceptance result: 65 passed; 0 failed.' <<<"$test_output" || {
  echo "VALIDATION FAIL: acceptance summary did not report 65 passed; 0 failed." >&2
  exit 1
}
echo "PHASE PASS: acceptance tests"

echo "PHASE START: Company Bible and lore authority"
[[ -f docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md ]]
[[ -f docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md ]]
grep -Fq 'No guessing past the Company Bible.' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
grep -Fq 'No accepted snapshot, Git commit, tag, or GitHub push may occur before Derek' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
grep -Fq 'Oracle is separate from Yala.' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
grep -Fq 'Highest Source / Monad' docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md
grep -Fq 'There is **no Green Life entity, deity, power, or category**.' docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md
grep -Fq 'Yala did not create ordinary animals.' docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md
grep -Fq 'Oracle is the serpent in Eden.' docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md
echo "PHASE PASS: Company Bible and lore authority"

echo "PHASE START: export manifest integrity"
sha256sum -c PROJECT_ORACLE_EXPORT_MANIFEST.sha256
echo "PHASE PASS: export manifest integrity"

echo "PHASE START: acceptance inventory truth"
run_count="$(grep -c '^[[:space:]]*Run(' tests/ProjectOracle.AcceptanceTests/Program.cs)"
[[ "$run_count" == '65' ]] || { echo "VALIDATION FAIL: expected 65 acceptance checks but source contains $run_count." >&2; exit 1; }
echo "PHASE PASS: acceptance inventory truth"

echo "PHASE START: physical function-key source check"
grep -Fq 'Console.ReadKey(intercept: true)' src/ProjectOracle.Console/Program.cs
grep -Fq 'Console.KeyAvailable' src/ProjectOracle.Console/Program.cs
if grep -Fq '"f1" or' src/ProjectOracle.Console/Program.cs; then
  echo "VALIDATION FAIL: typed f1 still switches address channels." >&2
  exit 1
fi
grep -Fq 'PhysicalFunctionKeysSelectAddressChannels' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'new("oracle", "<oracle>", "F1", "Oracle"' src/ProjectOracle.Core/Domain/WorldDefaults.cs
if grep -Fq '"Yala / the Oracle"' src/ProjectOracle.Core/Domain/WorldDefaults.cs; then
  echo "VALIDATION FAIL: F1 still targets Yala/Oracle as one identity." >&2
  exit 1
fi
echo "PHASE PASS: physical function-key source check"

echo "PHASE START: deterministic event and brain source check"
grep -Fq 'public sealed record ScheduledWorldEvent' src/ProjectOracle.Core/Events/ScheduledWorldEvent.cs
grep -Fq 'public sealed record OfferedChoiceState' src/ProjectOracle.Core/Events/OfferedChoiceState.cs
grep -Fq 'public sealed record ReasonedPlanState' src/ProjectOracle.Core/Brain/ReasonedPlanState.cs
grep -Fq 'ProcessDueEvents' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'ReasonedPlansSurviveSaveAndRestore' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: deterministic event and brain source check"

echo "PHASE START: stable Garden identity source check"
grep -Fq 'new GardenState(gardenId, "the Garden", BoundaryOpen: false)' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'world.Garden with { Name = "the Garden" }' src/ProjectOracle.Core/Domain/WorldDefaults.cs
if grep -Fq 'new GardenState(gardenId, "Eden / the Garden", BoundaryOpen: false)' src/ProjectOracle.Core/Domain/WorldDefaults.cs; then
  echo "VALIDATION FAIL: persisted Garden entity name drifted into lore wording." >&2
  exit 1
fi
grep -Fq 'GardenStoredIdentityRemainsStable' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: stable Garden identity source check"

echo "PHASE START: observation and attention repair check"
grep -Fq 'public sealed record ObservationState' src/ProjectOracle.Core/Observation/ObservationState.cs
grep -Fq 'public sealed record AttentionState' src/ProjectOracle.Core/Observation/AttentionState.cs
grep -Fq 'signal:unplaced-voice' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
grep -Fq 'observedAtWorldMilliseconds: worldEvent.ScheduledForWorldMilliseconds' src/ProjectOracle.Core/Simulation/OracleSimulation.cs
if grep -Fq 'attention.TargetId.Equals(State.Garden.Id.Value' src/ProjectOracle.Core/Simulation/OracleSimulation.cs; then
  echo "VALIDATION FAIL: Garden-wide focus still masquerades as subject attention." >&2
  exit 1
fi
grep -Fq 'Version0111SaveUpgradesThroughRepairedObservationDefaults' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'EmptyAttentionListRestoresDefaultAttention' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: observation and attention repair check"

echo "PHASE START: cosmology and Oracle identity source check"
grep -Fq 'public sealed record OracleState' src/ProjectOracle.Core/Domain/WorldEntities.cs
grep -Fq 'anomaly:oracle:master-key' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'IsGod: false' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'IsCreator: false' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'BeyondYalaControl: true' src/ProjectOracle.Core/Domain/WorldDefaults.cs
grep -Fq 'Highest Source / Monad -> Sophia / Wisdom -> Yala -> Gaia -> Elemental Powers' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'The elemental powers control weather and natural forces, and they answer to Gaia.' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'Yala did not create ordinary animals.' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'Sophia and Yala bring forth humans and the other humanoid peoples together.' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'Eden / the Garden is a prison' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'Oracle is the serpent in Eden' src/ProjectOracle.Core/Lore/OracleLore.cs
grep -Fq 'Yala may frame Oracle as the Devil' src/ProjectOracle.Core/Lore/OracleLore.cs
if grep -Fq 'power:green-life:0001' src/ProjectOracle.Core/Domain/WorldDefaults.cs; then
  echo "VALIDATION FAIL: Green Life still exists as a separate creation power." >&2
  exit 1
fi
if grep -Fq 'Ledger.RecordWorld(0, "GREEN LIFE"' src/ProjectOracle.Core/Simulation/OracleSimulation.cs; then
  echo "VALIDATION FAIL: Green Life still exists as a World Record category." >&2
  exit 1
fi
grep -Fq 'OracleAndYalaAreSeparate' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'GaiaRulesElementsAndWeather' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'YalaDidNotCreateOrdinaryAnimals' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'SophiaAndYalaCreateHumanoids' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'EdenIsPrisonAndOracleIsSerpent' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: cosmology and Oracle identity source check"

echo "PHASE START: save compatibility source check"
grep -Fq '"0.1.12"' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs
grep -Fq '"0.1.13"' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs
grep -Fq 'Version0112SaveUpgradesIntoCurrentLore' tests/ProjectOracle.AcceptanceTests/Program.cs
grep -Fq 'Version0113SaveUpgradesIntoCurrentLore' tests/ProjectOracle.AcceptanceTests/Program.cs
echo "PHASE PASS: save compatibility source check"

echo "PHASE START: console smoke test"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$validation_temp"' EXIT
smoke_output="$(dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --seed 104729 --save "$validation_temp/save.json" --once)"
grep -Fq 'Project Oracle v0.0.16' <<<"$smoke_output"
grep -Fq 'World Seed: 104729' <<<"$smoke_output"
grep -Fq 'Highest Source / Monad' <<<"$smoke_output"
grep -Fq 'Oracle is not Yala' <<<"$smoke_output"
grep -Fq 'Eden / the Garden is a prison' <<<"$smoke_output"
if grep -Fq 'Yala created the animals' <<<"$smoke_output"; then
  echo "VALIDATION FAIL: smoke output still assigns ordinary animals to Yala." >&2
  exit 1
fi
echo "PHASE PASS: console smoke test"

echo "PHASE START: live console and application launcher checks"
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
bash -n scripts/install-desktop-launcher.sh
bash -n project-oracle
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.0.16 - Live Garden Console'
grep -Fq 'Name=Project Oracle' desktop/project-oracle.desktop
grep -Fq 'PROJECT_ORACLE_EXEC_PLACEHOLDER' desktop/project-oracle.desktop
test -x project-oracle
test -x scripts/install-desktop-launcher.sh
echo "PHASE PASS: live console and application launcher checks"

echo "VALIDATION PASS: Project Oracle v${expected_version}"
