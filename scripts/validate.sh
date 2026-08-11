#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
expected_version='0.0.18'
root_executable="$project_root/Project_Oracle_v0_0_18"

fail() { echo "VALIDATION FAIL: $*" >&2; exit 1; }
blocked() { echo "VALIDATION BLOCKED: $*" >&2; exit 2; }

command -v dotnet >/dev/null 2>&1 || blocked 'Project Oracle v0.0.18 requires the .NET 10 SDK. No C# build or acceptance tests were run.'
sdk_version="$(dotnet --version)"
[[ "${sdk_version%%.*}" == '10' ]] || blocked "Expected .NET SDK 10.x but found $sdk_version."
command -v file >/dev/null 2>&1 || blocked "the 'file' command is required for native executable validation."
command -v ldd >/dev/null 2>&1 || blocked "the 'ldd' command is required for Soar native dependency validation."

for path in \
  vendor/soar/9.6.5/linux-x86-64/sml_csharp.dll \
  vendor/soar/9.6.5/linux-x86-64/libCSharp_sml_ClientInterface.so \
  vendor/soar/9.6.5/linux-x86-64/libSoar.so \
  vendor/soar/9.6.5/license.txt \
  src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar; do
  [[ -f "$path" ]] || fail "required Soar file missing: $path"
done

if ldd vendor/soar/9.6.5/linux-x86-64/libCSharp_sml_ClientInterface.so | grep -Fq 'not found'; then
  ldd vendor/soar/9.6.5/linux-x86-64/libCSharp_sml_ClientInterface.so >&2 || true
  fail 'Soar C# native bridge has an unresolved dependency.'
fi

echo 'PHASE START: restore'
dotnet restore ProjectOracle.sln
echo 'PHASE PASS: restore'

echo 'PHASE START: warnings-as-errors build'
dotnet build ProjectOracle.sln --configuration Release --no-restore
echo 'PHASE PASS: warnings-as-errors build'

echo 'PHASE START: native root publish'
publish_dir="$(mktemp -d)"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$publish_dir" "$validation_temp"' EXIT
rm -f -- "$root_executable"
dotnet publish src/ProjectOracle.Console/ProjectOracle.Console.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained false \
  -p:PublishSingleFile=true \
  -p:UseAppHost=true \
  -o "$publish_dir"
[[ -f "$publish_dir/ProjectOracle.Console" ]] || fail 'dotnet publish did not create the expected Linux apphost.'
cp "$publish_dir/ProjectOracle.Console" "$root_executable"
chmod +x "$root_executable"
file "$root_executable" | grep -Fq 'ELF' || fail 'Project_Oracle_v0_0_18 is not an ELF executable.'
echo 'PHASE PASS: native root publish'

echo 'PHASE START: complete acceptance suite'
set +e
test_output="$(PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE=1 dotnet run --project tests/ProjectOracle.AcceptanceTests/ProjectOracle.AcceptanceTests.csproj --configuration Release --no-build 2>&1)"
test_status=$?
set -e
printf '%s\n' "$test_output"
[[ "$test_status" -eq 0 ]] || fail "acceptance suite exited with status $test_status."
run_count="$(grep -c '^[[:space:]]*Run(' tests/ProjectOracle.AcceptanceTests/Program.cs)"
grep -Fq "Acceptance result: ${run_count} passed; 0 failed." <<<"$test_output" || fail "acceptance summary did not report ${run_count} passed; 0 failed."
echo "PHASE PASS: complete acceptance suite (${run_count} passed; 0 failed)"

echo 'PHASE START: Yala Brain Slice 2 gates'
grep -Fq 'Yala Soar Brain Slice 2' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'Brain Slice 2 identity missing.'
grep -Fq 'smem --set learning on' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'Soar semantic memory is not enabled.'
grep -Fq 'epmem --set learning on' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'Soar episodic memory is not enabled.'
grep -Fq '^impasse tie' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar substate deliberation rules are missing.'
grep -Fq 'drive-curiosity' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Yala drive input is missing.'
grep -Fq 'SessionDecisionCount' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'persistent Soar session counter missing.'
grep -Fq 'YalaConversationInterpreter' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'conversation interpretation boundary missing.'
grep -Fq 'YalaBeliefState' src/ProjectOracle.Core/Domain/WorldEntities.cs || fail 'structured Yala beliefs missing.'
grep -Fq 'YalaEpisodeState' src/ProjectOracle.Core/Domain/WorldEntities.cs || fail 'structured Yala episodes missing.'
grep -Fq 'YalaDriveState' src/ProjectOracle.Core/Domain/WorldEntities.cs || fail 'Yala drives missing.'
echo 'PHASE PASS: Yala Brain Slice 2 gates'

echo 'PHASE START: protected console input gates'
[[ -f src/ProjectOracle.Console/ConsoleInputLine.cs ]] || fail 'ConsoleInputLine is missing.'
[[ -f src/ProjectOracle.Console/LiveConsoleSurface.cs ]] || fail 'LiveConsoleSurface compatibility shell is missing.'
grep -Fq 'public static bool VisibleStatusInBody => false;' src/ProjectOracle.Console/LiveConsoleSurface.cs || fail 'asynchronous terminal-body LIVE status is not hard-disabled.'
grep -Fq 'return false;' src/ProjectOracle.Console/LiveConsoleSurface.cs || fail 'visible LIVE paint permission is not hard-disabled.'
if grep -Eq 'System\.Console\.(Write|WriteLine|SetCursorPosition)|ConsoleTheme\.(Write|WriteLine|WritePrompt|LiveLine|ClearLine)' src/ProjectOracle.Console/LiveConsoleSurface.cs; then
  fail 'LiveConsoleSurface is not terminal-silent.'
fi
if grep -Eq 'LiveConsoleSurface|surface\.Refresh|SetCursorPosition|ConsoleTheme\.LiveLine' src/ProjectOracle.Console/Program.cs; then
  fail 'interactive Program path still contains LIVE-surface or cursor-paint code.'
fi
grep -Fq 'ConsoleTheme.WritePrompt("> ");' src/ProjectOracle.Console/Program.cs || fail 'interactive prompt ownership is missing.'
grep -Fq 'interactive input path contains no LIVE surface refresh' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'no-LIVE interactive path regression test missing.'
grep -Fq 'asynchronous LIVE status is forbidden from the console body' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'console body isolation regression test missing.'
echo 'PHASE PASS: protected console input gates'

echo 'PHASE START: canon and hidden-Oracle truth gates'
grep -Fq 'Monad made Sophia / Wisdom.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad -> Wisdom lore missing.'
grep -Fq 'Wisdom made Yala alone' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Wisdom -> Yala lore missing.'
grep -Fq 'Yala is inherently both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Yala male-and-female nature missing.'
grep -Fq 'Monad rejected Yala because Yala is both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad rejection reason missing.'
grep -Fq 'Gaia creates in-world Time.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Gaia-created Time lore missing.'
grep -Fq 'manifested in the form of a clever serpent' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'serpent manifestation wording missing.'
grep -Fq 'Eve knew only the clever serpent' README.md || fail 'README Eve/serpent knowledge boundary missing.'
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' --exclude-dir=bin --exclude-dir=obj \
  'Creator / Omega|Creator/Omega|autonomous in-world Oracle|Yala is male\.' \
  README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs src >/dev/null; then
  fail 'active source/docs still contain superseded cosmology wording.'
fi
if grep -RIniE --exclude-dir=bin --exclude-dir=obj 'new\("oracle"|new\("Oracle"|\("Oracle", "\(Oracle"' src >/dev/null; then
  fail 'active source appears to recreate Oracle as an in-world target/entity.'
fi
production_text="$(grep -v '^[[:space:]]*#' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || true)"
if grep -qi 'oracle' <<<"$production_text"; then
  fail "Yala's Soar productions expose Oracle knowledge."
fi
echo 'PHASE PASS: canon and hidden-Oracle truth gates'

echo 'PHASE START: README and authority alignment'
for path in \
  README.md \
  PROJECT_ORACLE_MASTER_HANDOFF.md \
  docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md \
  docs/PROJECT_ORACLE_LORE_CANON_v0_0_18.md \
  docs/PROJECT_ORACLE_CANON_v0_0_18.md \
  docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_18.md \
  docs/PROJECT_ORACLE_ROADMAP_v0_0_18.md \
  docs/PROJECT_ORACLE_VALIDATION_v0_0_18.md \
  docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_18.md \
  docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_18.md; do
  [[ -f "$path" ]] || fail "current authority missing: $path"
  grep -Fq 'v0.0.18' "$path" || fail "current version marker missing from $path"
done
grep -Fq 'Oracle is not an in-world character' README.md || fail 'README system-level Oracle statement missing.'
grep -Fq 'both male and female' README.md || fail 'README Yala nature statement missing.'
grep -Fq 'Monad rejected Yala' README.md || fail 'README Monad rejection statement missing.'
grep -Fq 'Gaia creates in-world Time' README.md || fail 'README Gaia/Time statement missing.'
grep -Fq 'Brain Slice 2' README.md || fail 'README Brain Slice 2 statement missing.'
echo 'PHASE PASS: README and authority alignment'

echo 'PHASE START: save-v2 continuity and pre-Time architecture gates'
grep -Fq 'save_v2.json' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save_v2 path missing.'
grep -Fq '"0.0.17"' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'v0.0.17 save-v2 continuity support missing.'
if grep -Fq '"0.0.16"' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs; then fail 'v0.0.16 must remain rejected by the v0.0.18 save loader.'; fi
grep -Fq 'v0.0.18 continues the v0.0.17 save_v2 world line' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'v0.0.17 save continuity regression missing.'
grep -Fq 'Clock.Hold' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'pre-Time world-clock hold is missing.'
grep -Fq 'KnowsOfOracle: false' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'Yala hidden-Oracle default missing.'
echo 'PHASE PASS: save-v2 continuity and pre-Time architecture gates'

echo 'PHASE START: launcher checks'
bash -n scripts/run.sh
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
bash -n scripts/install-desktop-launcher.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.0.18 - Yala Soar Console' || fail 'window dry-run identity wrong.'
grep -Fq 'PROJECT_ORACLE_EXEC_PLACEHOLDER' desktop/project-oracle.desktop || fail 'desktop executable placeholder missing.'
grep -Fq 'Project_Oracle_v0_0_18' scripts/install-desktop-launcher.sh || fail 'desktop installer does not target root v0.0.18 executable.'
echo 'PHASE PASS: launcher checks'

echo 'PHASE START: export manifest integrity'
sha256sum -c PROJECT_ORACLE_EXPORT_MANIFEST.sha256
echo 'PHASE PASS: export manifest integrity'

echo 'PHASE START: published executable Soar memory smoke'
set +e
smoke_output="$(NO_COLOR=1 "$root_executable" --once --seed 104729 --save "$validation_temp/smoke-save.json" 2>&1)"
smoke_status=$?
set -e
printf '%s\n' "$smoke_output"
[[ "$smoke_status" -eq 0 ]] || fail "published executable smoke exited with status $smoke_status."
grep -Fq 'Project Oracle v0.0.18' <<<"$smoke_output" || fail 'published smoke version missing.'
grep -Fq 'SOAR SMOKE PASS:' <<<"$smoke_output" || fail 'published executable did not prove a real Soar Yala decision.'
[[ -f "$validation_temp/yala_soar_v0_0_18/semantic.sqlite" ]] || fail 'Soar semantic SQLite memory was not created.'
[[ -f "$validation_temp/yala_soar_v0_0_18/episodic.sqlite" ]] || fail 'Soar episodic SQLite memory was not created.'
echo 'PHASE PASS: published executable Soar memory smoke'

echo "VALIDATION PASS: Project Oracle v${expected_version}"
echo "FINAL PASS: automated validation complete; launch the real application for Derek manual inspection before snapshot/Git/push."
