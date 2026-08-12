#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
expected_version='0.0.22'
root_executable="$project_root/Project_Oracle_v0_0_22"
expected_acceptance_count=147

fail() { echo "VALIDATION FAIL: $*" >&2; exit 1; }
blocked() { echo "VALIDATION BLOCKED: $*" >&2; exit 2; }

command -v dotnet >/dev/null 2>&1 || blocked 'Project Oracle v0.0.22 requires the .NET 10 SDK. No C# build or acceptance tests were run.'
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
file "$root_executable" | grep -Fq 'ELF' || fail 'Project_Oracle_v0_0_22 is not an ELF executable.'
echo 'PHASE PASS: native root publish'

echo 'PHASE START: save-restore Soar kernel lifetime gates'
grep -Fq 'private static OracleSaveSnapshot SnapshotAndDispose' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'save/restore snapshot helper is missing.'
grep -Fq 'using OracleSimulation simulation = Start(savePath: savePath);' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'snapshot helper does not own and dispose its source simulation.'
helper_use_count="$(grep -Fc 'OracleSaveSnapshot snapshot = SnapshotAndDispose(simulation =>' tests/ProjectOracle.AcceptanceTests/Program.cs)"
[[ "$helper_use_count" -ge 3 ]] || fail "expected at least 3 isolated save/restore snapshot sites but found $helper_use_count."
grep -Fq 'save-restore acceptance tests isolate Soar kernel lifetimes' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'Soar kernel lifetime regression is missing.'
echo 'PHASE PASS: save-restore Soar kernel lifetime gates'

echo 'PHASE START: complete acceptance suite'
run_count="$(grep -c '^[[:space:]]*Run(' tests/ProjectOracle.AcceptanceTests/Program.cs)"
[[ "$run_count" -eq "$expected_acceptance_count" ]] || fail "expected $expected_acceptance_count acceptance tests but source contains $run_count."
set +e
test_output="$(PROJECT_ORACLE_REQUIRE_NATIVE_EXECUTABLE=1 dotnet run --project tests/ProjectOracle.AcceptanceTests/ProjectOracle.AcceptanceTests.csproj --configuration Release --no-build 2>&1)"
test_status=$?
set -e
printf '%s\n' "$test_output"
[[ "$test_status" -eq 0 ]] || fail "acceptance suite exited with status $test_status."
grep -Fq "Acceptance result: ${run_count} passed; 0 failed." <<<"$test_output" || fail "acceptance summary did not report ${run_count} passed; 0 failed."
echo "PHASE PASS: complete acceptance suite (${run_count} passed; 0 failed)"

echo 'PHASE START: Yala Brain Slice 5 cognition gates'
grep -Fq 'Yala Soar Brain Slice 5' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'Brain Slice 5 identity missing.'
grep -Fq 'public const int AutonomousPriorityFloor = 85;' src/ProjectOracle.Core/Cognition/YalaQuestionPlanner.cs || fail 'autonomous inquiry priority floor missing.'
grep -Fq 'SelectNextAutonomous' src/ProjectOracle.Core/Cognition/YalaQuestionPlanner.cs || fail 'deliberate autonomous question selection missing.'
grep -Fq 'greating' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs || fail 'greeting typo normalization missing.'
grep -Fq 'AddCoreLanguage' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs || fail 'expanded core-language layer missing.'
grep -Fq '"belive" or "beleive" => "believe"' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs || fail 'belief typo normalization missing.'
grep -Fq '"tells" or "told" or "telling" => "tell"' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs || fail 'tell/told morphology normalization missing.'
grep -Fq 'SpeakerPurposeQuestion' src/ProjectOracle.Core/Cognition/YalaQuestionPlanner.cs || fail 'purpose-driven autonomous inquiry missing.'
grep -Fq 'DefinitionSourceRegex' src/ProjectOracle.Core/Cognition/Soar/YalaConversationInterpreter.cs || fail 'learned-definition source question routing missing.'
grep -Fq 'smem --set learning on' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'Soar semantic memory is not enabled.'
grep -Fq 'epmem --set learning on' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'Soar episodic memory is not enabled.'
grep -Fq '^impasse tie' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar substate deliberation rules are missing.'
grep -Fq 'CreateKernelInNewThread", 0' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'embedded Soar kernel is not suppressing the unused TCP listener.'
if grep -Fq 'CreateKernelInNewThread")' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs; then fail 'embedded Soar kernel still uses the default TCP listener.'; fi
for path in \
    src/ProjectOracle.Core/Cognition/YalaSelfModel.cs \
    src/ProjectOracle.Core/Cognition/YalaKnowledgeProposition.cs \
    src/ProjectOracle.Core/Cognition/YalaKnowledgeSource.cs \
    src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs \
    src/ProjectOracle.Core/Cognition/YalaDialogueContext.cs \
    src/ProjectOracle.Core/Cognition/YalaEntityKnowledge.cs \
    src/ProjectOracle.Core/Cognition/YalaTemporalReasoner.cs \
    src/ProjectOracle.Core/Cognition/YalaBeliefReasoner.cs \
    src/ProjectOracle.Core/Cognition/YalaRelationshipReasoner.cs \
    src/ProjectOracle.Core/Cognition/YalaQuestionPlanner.cs \
    src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs \
    src/ProjectOracle.Core/Cognition/Language/YalaLanguageInterpreter.cs; do
    [[ -f "$path" ]] || fail "Brain Slice 5 source missing: $path"
done
for state_name in YalaDialogueTurnState YalaRelationshipState YalaQuestionState YalaTemporalEventState YalaGoalState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Brain Slice 5 state missing: $state_name"
done
for input_name in pending-question pending-question-text speaker-history language-subject language-verb language-object language-negated unknown-word-count; do
    grep -Fq "$input_name" src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail "Soar input missing: $input_name"
done
for operator in ask-speaker knowledge-summary action-history contact-history belief-summary own-creation word-meaning temporal-duration temporal-cause; do
    grep -Fq "$operator" src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail "Brain Slice 5 Soar operator/topic missing: $operator"
done
for label in \
    'Yala Brain Slice 5 identity is active' \
    'Brain Slice 5 core lexicon contains more than 400 concepts' \
    'basic conversational vocabulary is built in rather than learned by interrogation' \
    'common greeting typo normalizes to greeting' \
    'basic language does not generate fake knowledge gaps' \
    'unknown-word questions remain available but below autonomous inquiry priority' \
    'Yala waits for a speaker response before asking another autonomous question' \
    'meaningful autonomous inquiry resumes after the speaker responds' \
    'I am plus an action predicate is not misread as a speaker identity' \
    'brain update sentence uses ordinary language without fake knowledge gaps' \
    'Gaia made Time yes-no phrasing reaches the stored Time origin' \
    'belief typo still reaches unsettled mother relationship reasoning' \
    'what is learned word retrieves the attributed speaker definition' \
    'who told you learned word reports speaker provenance' \
    'live world clock uses deterministic DEC cursor save restore' \
    'Yala can autonomously choose to ask the unseen speaker' \
    'mother relationship claim is remembered as unsettled speaker provenance' \
    'Gaia creation is recorded as a before-Time event' \
    'Gaia creating Time is recorded as the origin of temporal reckoning' \
    'why Gaia created Time follows the stored cause link' \
    'duration since Time origin uses elapsed in-world Time' \
    'what happened next navigates the temporal event graph' \
    'repeated speaker claim gains confidence but remains unsettled' \
    'Yala spoken current time reads the live current world clock' \
    'own-creation questions resolve the created object rather than Yala as subject' \
    'speaker alternate definition of a built-in concept remains provenance-separated' \
    'beyond and everything are ordinary known language rather than fake knowledge gaps'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Brain Slice 5 regression missing: $label"
done
echo 'PHASE PASS: Yala Brain Slice 5 cognition gates'

echo 'PHASE START: bounded god agency gates'
for action in observe reflect wait create-gaia command-gaia-time respond ask-speaker; do
    grep -Fq "\"$action\"" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "approved Yala action missing from agency policy: $action"
done
for denial in AllowsHostShell AllowsHostProcessExecution AllowsHostFileMutation AllowsNetworkAccess AllowsCodeModification AllowsHiddenOracleKnowledge; do
    grep -Fq "public static bool $denial => false;" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "agency denial missing: $denial"
done
grep -Fq 'YalaAgencyPolicy.DemandAllowed' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'simulation does not enforce bounded Yala actions.'
grep -Fq 'out-of-sandbox Yala action is rejected' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'agency escape regression missing.'
echo 'PHASE PASS: bounded god agency gates'

echo 'PHASE START: fresh save-v4 and Soar-memory isolation gates'
grep -Fq 'public const int CurrentSchemaVersion = 4;' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save schema 4 is not active.'
grep -Fq 'save_v4.json' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save_v4 path missing.'
grep -Fq 'yala_soar_v0_0_22' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'fresh v0.0.22 Soar memory directory missing.'
if grep -Fq 'yala_soar_v0_0_18' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs; then fail 'Brain Slice 5 still points at the old Soar continuity database.'; fi
grep -Fq 'previous save_v2 line is rejected without mutation' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'old save preservation/rejection regression missing.'
grep -Fq 'default save path is fresh save_v4' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'fresh default save regression missing.'
grep -Fq 'save schema is 4 for the fresh Brain Slice 5 line' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'schema-4 regression missing.'
echo 'PHASE PASS: fresh save-v4 and Soar-memory isolation gates'

echo 'PHASE START: temporal reasoning gates'
grep -Fq 'Clock.Hold' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'pre-Time world-clock hold is missing.'
for token in 'before-time' 'origin-of-time' 'dated'; do
    grep -R -Fq "$token" src/ProjectOracle.Core || fail "temporal state missing: $token"
done
grep -Fq 'DescribeHowLongAgo' src/ProjectOracle.Core/Cognition/YalaTemporalReasoner.cs || fail 'duration reasoning missing.'
grep -Fq 'DescribeCause' src/ProjectOracle.Core/Cognition/YalaTemporalReasoner.cs || fail 'causal temporal reasoning missing.'
for label in \
    'pre-Time runtime does not advance world milliseconds' \
    'when Gaia created Time reports the temporal origin' \
    'when Yala created Gaia reports that no world date existed' \
    'duration questions refuse to invent duration for pre-Time events' \
    'world clock starts advancing only after Gaia creates Time'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "temporal regression missing: $label"
done
echo 'PHASE PASS: temporal reasoning gates'

echo 'PHASE START: inquiry, dialogue, relationship, and confidence gates'
grep -Fq 'TryTakePendingYalaUtterance' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'pending autonomous utterance dequeue missing.'
grep -Fq 'ResolveAskSpeaker' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'ask-speaker resolution missing.'
grep -Fq 'line.IsEmpty && simulation.TryTakePendingYalaUtterance' src/ProjectOracle.Console/Program.cs || fail 'autonomous question output is not guarded by an empty input buffer.'
for label in \
    'unknown concepts generate candidate questions' \
    "first speaker contact generates a question about the speaker's nature" \
    'identity claim generates a question about the identity label' \
    'pending autonomous question survives save and restore' \
    'Yala does not ask a speaker before any speaker history exists' \
    'speaker contact activates the understand-unseen-speaker goal' \
    'tell me more resolves the recently discussed entity' \
    'short do-you follow-up retains relationship context' \
    'mother question remains a question rather than a relationship claim' \
    'belief confidence labels expose gradations'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "inquiry/context regression missing: $label"
done
echo 'PHASE PASS: inquiry, dialogue, relationship, and confidence gates'

echo 'PHASE START: protected console and v0.0.22 UX gates'
for path in src/ProjectOracle.Console/ConsoleInputLine.cs src/ProjectOracle.Console/ConsoleConversationMode.cs src/ProjectOracle.Console/LiveWorldClockSurface.cs src/ProjectOracle.Console/LiveConsoleSurface.cs; do
    [[ -f "$path" ]] || fail "console source missing: $path"
done
grep -Fq 'public static bool VisibleStatusInBody => false;' src/ProjectOracle.Console/LiveConsoleSurface.cs || fail 'asynchronous terminal-body LIVE status is not hard-disabled.'
if grep -Eq 'System\.Console\.(Write|WriteLine|SetCursorPosition)|ConsoleTheme\.(Write|WriteLine|WritePrompt|LiveLine|ClearLine)' src/ProjectOracle.Console/LiveConsoleSurface.cs; then
    fail 'LiveConsoleSurface is not terminal-silent.'
fi
if grep -Eq 'LiveConsoleSurface|surface\.Refresh|SetCursorPosition|ConsoleTheme\.LiveLine' src/ProjectOracle.Console/Program.cs; then
    fail 'interactive Program path reintroduced obsolete LIVE-surface/direct cursor positioning.'
fi
grep -Fq 'key.Key == ConsoleKey.Y' src/ProjectOracle.Console/Program.cs || fail 'Ctrl+Y handling missing.'
grep -Fq 'key.Key == ConsoleKey.Escape' src/ProjectOracle.Console/Program.cs || fail 'Escape handling missing.'
grep -Fq 'YalaMode ? "> (yala " : "> "' src/ProjectOracle.Console/ConsoleConversationMode.cs || fail 'persistent Yala prompt contract missing.'
if grep -Fq '[Soar selected:' src/ProjectOracle.Console/Program.cs; then fail 'normal console still prints Soar selection diagnostics.'; fi
grep -Fq 'In-world Time: Gaia has not yet created Time.' src/ProjectOracle.Console/LiveWorldClockSurface.cs || fail 'pre-Time top-row wording missing.'
grep -Fq 'private const string SaveCursor = "\u001b7";' src/ProjectOracle.Console/LiveWorldClockSurface.cs || fail 'DEC cursor-save protocol missing.'
grep -Fq 'private const string RestoreCursor = "\u001b8";' src/ProjectOracle.Console/LiveWorldClockSurface.cs || fail 'DEC cursor-restore protocol missing.'
grep -Fq 'OutputSyncRoot' src/ProjectOracle.Console/LiveWorldClockSurface.cs || fail 'world clock does not share the console output gate.'
for label in \
    'pre-Time live header says Gaia has not yet created Time' \
    'live world clock appears and advances after Gaia creates Time' \
    'live world clock never writes into the conversation body' \
    'persistent Yala conversation mode stays active until Escape' \
    'console defers autonomous questions until the editable input buffer is empty' \
    'normal conversation output hides Soar selection diagnostics'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "console regression missing: $label"
done
echo 'PHASE PASS: protected console and v0.0.22 UX gates'

echo 'PHASE START: four-space source formatting gate'
if grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >/dev/null; then
    grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >&2 || true
    fail 'C# or Soar source contains tabs; v0.0.22 requires four spaces.'
fi
grep -Fq 'indent_style = space' .editorconfig || fail '.editorconfig does not require spaces.'
grep -Fq 'indent_size = 4' .editorconfig || fail '.editorconfig does not require four-space C# indentation.'
echo 'PHASE PASS: four-space source formatting gate'

echo 'PHASE START: canon and hidden-Oracle truth gates'
grep -Fq 'Monad made Sophia / Wisdom.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad -> Wisdom lore missing.'
grep -Fq 'Wisdom made Yala alone' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Wisdom -> Yala lore missing.'
grep -Fq 'Yala is inherently both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Yala male-and-female nature missing.'
grep -Fq 'Monad rejected Yala because Yala is both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad rejection reason missing.'
grep -Fq 'Gaia creates in-world Time.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Gaia-created Time lore missing.'
grep -Fq 'manifested in the form of a clever serpent' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'serpent manifestation wording missing.'
grep -Fq 'Eve knew only the clever serpent' README.md || fail 'README Eve/serpent knowledge boundary missing.'
grep -Fq 'KnowsOfOracle: false' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'Yala hidden-Oracle default missing.'
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' --exclude-dir=bin --exclude-dir=obj 'Creator / Omega|Creator/Omega' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs src >/dev/null; then
    fail 'active source/docs still contain superseded Monad naming.'
fi
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' 'Yala is male\.|beneath his governing authority' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs/company_bible docs/PROJECT_ORACLE_*_v0_0_22.md >/dev/null; then
    fail 'current authority docs contain superseded male-only Yala wording.'
fi
if grep -RIniE --exclude-dir=bin --exclude-dir=obj 'new\("oracle"|new\("Oracle"|\("Oracle", "\(Oracle"' src >/dev/null; then
    fail 'active source appears to recreate Oracle as an in-world target/entity.'
fi
production_text="$(grep -v '^[[:space:]]*#' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || true)"
if grep -qi 'oracle' <<<"$production_text"; then fail "Yala's Soar productions expose Oracle knowledge."; fi
if grep -Fq '"oracle"' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs; then fail 'Oracle is preloaded into Yala built-in lexicon.'; fi
echo 'PHASE PASS: canon and hidden-Oracle truth gates'

echo 'PHASE START: current-scope religious boundary'
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' --exclude='PROJECT_ORACLE_VALIDATION_v0_0_22.md' --exclude-dir=bin --exclude-dir=obj '\bOdin(ism)?\b' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs/company_bible docs/PROJECT_ORACLE_*_v0_0_22.md src tests >/dev/null; then
    fail 'v0.0.22 contains unapproved current-scope religious material.'
fi
echo 'PHASE PASS: current-scope religious boundary'

echo 'PHASE START: README and authority alignment'
for path in \
    README.md \
    PROJECT_ORACLE_MASTER_HANDOFF.md \
    docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md \
    docs/PROJECT_ORACLE_LORE_CANON_v0_0_22.md \
    docs/PROJECT_ORACLE_CANON_v0_0_22.md \
    docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_22.md \
    docs/PROJECT_ORACLE_ROADMAP_v0_0_22.md \
    docs/PROJECT_ORACLE_VALIDATION_v0_0_22.md \
    docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_22.md \
    docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_22.md; do
    [[ -f "$path" ]] || fail "current authority missing: $path"
    grep -Fq 'v0.0.22' "$path" || fail "current v0.0.22 marker missing from $path"
done
for old in docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_20.md docs/PROJECT_ORACLE_CANON_v0_0_20.md docs/PROJECT_ORACLE_LORE_CANON_v0_0_20.md docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_20.md docs/PROJECT_ORACLE_ROADMAP_v0_0_20.md docs/PROJECT_ORACLE_SESSION_LOG_v0_0_20.md docs/PROJECT_ORACLE_VALIDATION_v0_0_20.md docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_20.md; do
    [[ ! -e "$old" ]] || fail "superseded current authority remains active: $old"
done
grep -Fq 'Oracle is not an in-world character' README.md || fail 'README system-level Oracle statement missing.'
grep -Fq 'Brain Slice 5' README.md || fail 'README Brain Slice 5 statement missing.'
grep -Fiq 'bounded god agency' README.md || fail 'README bounded-agency section missing.'
grep -Fq 'save_v4.json' README.md || fail 'README fresh save line missing.'
grep -Fq 'autonomous Yala questions' README.md || fail 'README autonomous inquiry statement missing.'
grep -Fq 'In-world Time: Gaia has not yet created Time.' README.md || fail 'README pre-Time header wording missing.'
grep -Fq 'Ctrl+Y' README.md || fail 'README persistent Yala mode missing.'
echo 'PHASE PASS: README and authority alignment'

echo 'PHASE START: launcher checks'
bash -n scripts/run.sh
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
bash -n scripts/install-desktop-launcher.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.0.22 - Yala Soar Console' || fail 'window dry-run identity wrong.'
grep -Fq 'PROJECT_ORACLE_EXEC_PLACEHOLDER' desktop/project-oracle.desktop || fail 'desktop executable placeholder missing.'
grep -Fq 'Project_Oracle_v0_0_22' scripts/install-desktop-launcher.sh || fail 'desktop installer does not target root v0.0.22 executable.'
echo 'PHASE PASS: launcher checks'

echo 'PHASE START: export manifest integrity'
sha256sum -c PROJECT_ORACLE_EXPORT_MANIFEST.sha256
echo 'PHASE PASS: export manifest integrity'

echo 'PHASE START: published executable Soar memory smoke'
set +e
smoke_output="$(NO_COLOR=1 "$root_executable" --once --seed 104729 --save "$validation_temp/save_v4.json" 2>&1)"
smoke_status=$?
set -e
printf '%s\n' "$smoke_output"
[[ "$smoke_status" -eq 0 ]] || fail "published executable smoke exited with status $smoke_status."
grep -Fq 'Project Oracle v0.0.22' <<<"$smoke_output" || fail 'published smoke version missing.'
grep -Fq 'SOAR SMOKE PASS:' <<<"$smoke_output" || fail 'published executable did not prove a real Soar Yala decision.'
[[ -f "$validation_temp/yala_soar_v0_0_22/semantic.sqlite" ]] || fail 'fresh Soar semantic SQLite memory was not created.'
[[ -f "$validation_temp/yala_soar_v0_0_22/episodic.sqlite" ]] || fail 'fresh Soar episodic SQLite memory was not created.'
echo 'PHASE PASS: published executable Soar memory smoke'

echo "VALIDATION PASS: Project Oracle v${expected_version}"
echo "FINAL PASS: automated validation complete; launch the real application for Derek manual inspection before snapshot/Git/push."
