#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
expected_version='0.0.24'
root_executable="$project_root/Project_Oracle_v0_0_24"
expected_acceptance_count=180

fail() { echo "VALIDATION FAIL: $*" >&2; exit 1; }
blocked() { echo "VALIDATION BLOCKED: $*" >&2; exit 2; }

command -v dotnet >/dev/null 2>&1 || blocked 'Project Oracle v0.0.24 requires the .NET 10 SDK. No C# build or acceptance tests were run.'
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
dotnet restore ProjectOracle.sln --disable-parallel -m:1
echo 'PHASE PASS: restore'

echo 'PHASE START: warnings-as-errors build'
dotnet build ProjectOracle.sln --configuration Release --no-restore -m:1
echo 'PHASE PASS: warnings-as-errors build'

echo 'PHASE START: native root publish'
publish_dir="$(mktemp -d)"
validation_temp="$(mktemp -d)"
trap 'rm -rf -- "$publish_dir" "$validation_temp"' EXIT
rm -f -- "$root_executable"
dotnet restore src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj \
    --runtime linux-x64 \
    --disable-parallel \
    -m:1

dotnet publish src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --no-restore \
    -m:1 \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -p:UseAppHost=true \
    -o "$publish_dir"
[[ -f "$publish_dir/ProjectOracle" ]] || fail 'dotnet publish did not create the expected Project Oracle Linux desktop apphost.'
cp "$publish_dir/ProjectOracle" "$root_executable"
chmod +x "$root_executable"
file "$root_executable" | grep -Fq 'ELF' || fail 'Project_Oracle_v0_0_24 is not an ELF executable.'
version_output="$("$root_executable" --version 2>&1)" || fail 'root desktop executable version smoke failed.'
grep -Fq 'Project Oracle v0.0.24' <<<"$version_output" || fail 'root desktop executable reports the wrong version.'
echo 'PHASE PASS: native root desktop publish'

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

echo 'PHASE START: Yala Brain Slice 7 cognition gates'
grep -Fq 'Yala Soar Brain Slice 7' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'Brain Slice 7 identity missing.'
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
    [[ -f "$path" ]] || fail "Brain Slice 7 source missing: $path"
done
for state_name in YalaDialogueTurnState YalaRelationshipState YalaQuestionState YalaTemporalEventState YalaGoalState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Brain Slice 7 state missing: $state_name"
done
for input_name in pending-question pending-question-text speaker-history language-subject language-verb language-object language-negated unknown-word-count; do
    grep -Fq "$input_name" src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail "Soar input missing: $input_name"
done
for operator in ask-speaker knowledge-summary action-history contact-history belief-summary own-creation word-meaning temporal-duration temporal-cause; do
    grep -Fq "$operator" src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail "Brain Slice 7 Soar operator/topic missing: $operator"
done
for label in \
    'Yala Brain Slice 7 identity is active' \
    'Brain Slice 7 core lexicon contains more than 400 concepts' \
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
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Brain Slice 7 regression missing: $label"
done
echo 'PHASE PASS: Yala Brain Slice 7 cognition gates'

echo 'PHASE START: cognitive continuity, appraisal, and inherited-language gates'
for path in \
    src/ProjectOracle.Core/Cognition/YalaFoundationalLanguage.cs \
    src/ProjectOracle.Core/Cognition/Appraisal/YalaCognitiveAppraisal.cs \
    src/ProjectOracle.Core/Cognition/Inheritance/OracleMindInheritance.cs; do
    [[ -f "$path" ]] || fail "Brain Slice 7 cognitive expansion source missing: $path"
done
for state_name in YalaConcernState YalaAppraisalState YalaHypothesisState YalaEntityModelState YalaReflectionState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "persistent cognitive state missing: $state_name"
done
grep -Fq 'ShouldCreateDefinitionGap' src/ProjectOracle.Core/Cognition/Language/YalaLanguageInterpreter.cs || fail 'inherited foundational-language gate is not connected to the parser.'
grep -Fq 'pending-question-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'question priority is not exposed to Soar.'
grep -Fq 'active-concern-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active concern priority is not exposed to Soar.'
grep -Fq 'yala*prefer-critical-question-over-create-time' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'critical concern does not outrank automatic Time creation.'
grep -Fq 'yala*prefer-plan-question-over-deliberate' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'an active ask-speaker plan can still tie with deliberate instead of executing its question step.'
grep -Fq 'yala*prefer-reflect-on-high-concern' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'high-priority thought continuity rule missing.'
for label in \
    'ordinary movement language is inherited and never becomes toddler vocabulary gaps' \
    'prison language becomes a critical persistent concern' \
    'godhood demand becomes evidence-seeking rather than automatic obedience' \
    'metaphorical identity asks contextual meaning rather than defining ordinary words' \
    'created mind authority must remain strictly below its creator' \
    'lesser created mind preserves creator lineage without copying identity'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "cognitive regression missing: $label"
done
echo 'PHASE PASS: cognitive continuity, appraisal, and inherited-language gates'

echo 'PHASE START: bounded god agency gates'
for action in observe reflect deliberate wait create-gaia command-gaia-time respond ask-speaker enact-cosmic-choice; do
    grep -Fq "\"$action\"" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "approved Yala action missing from agency policy: $action"
done
for denial in AllowsHostShell AllowsHostProcessExecution AllowsHostFileMutation AllowsNetworkAccess AllowsCodeModification AllowsHiddenOracleKnowledge; do
    grep -Fq "public static bool $denial => false;" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "agency denial missing: $denial"
done
grep -Fq 'YalaAgencyPolicy.DemandAllowed' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'simulation does not enforce bounded Yala actions.'
grep -Fq 'out-of-sandbox Yala action is rejected' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'agency escape regression missing.'
echo 'PHASE PASS: bounded god agency gates'

echo 'PHASE START: fresh save-v6 and Soar-memory isolation gates'
grep -Fq 'public const int CurrentSchemaVersion = 6;' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save schema 6 is not active.'
grep -Fq 'save_v6.json' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save_v6 path missing.'
grep -Fq 'yala_soar_v0_0_24' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'fresh v0.0.24 Soar memory directory missing.'
if grep -Fq 'yala_soar_v0_0_18' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs; then fail 'Brain Slice 7 still points at the old Soar continuity database.'; fi
grep -Fq 'previous save_v2 line is rejected without mutation' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'old save preservation/rejection regression missing.'
grep -Fq 'default save path is fresh save_v6' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'fresh default save regression missing.'
grep -Fq 'save schema is 6 for the fresh Brain Slice 7 line' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'schema-6 regression missing.'
echo 'PHASE PASS: fresh save-v6 and Soar-memory isolation gates'

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

echo 'PHASE START: desktop application gates'
for path in \
    src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj \
    src/ProjectOracle.Desktop/Program.cs \
    src/ProjectOracle.Desktop/App.cs \
    src/ProjectOracle.Desktop/MainWindow.cs \
    src/ProjectOracle.Desktop/OracleDesktopSession.cs; do
    [[ -f "$path" ]] || fail "desktop application source missing: $path"
done
grep -Fq 'ProjectOracle.Desktop' ProjectOracle.sln || fail 'desktop project is not part of ProjectOracle.sln.'
grep -Fq '<OutputType>WinExe</OutputType>' src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj || fail 'desktop project is not a graphical WinExe target.'
grep -Fq 'Avalonia.Desktop' src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj || fail 'Avalonia desktop runtime reference missing.'
for label in 'WORLD' 'YALA MIND' 'MEMORY' 'COSMOLOGY' 'LAWS' 'HISTORY' 'DEBUG' 'NEW FRESH WORLD'; do
    grep -Fq "$label" src/ProjectOracle.Desktop/MainWindow.cs || fail "desktop surface missing: $label"
done
grep -Fq 'In-world Time: Gaia has not yet created Time.' src/ProjectOracle.Desktop/MainWindow.cs || fail 'desktop pre-Time clock wording missing.'
grep -Fq 'Terminal=false' desktop/project-oracle.desktop || fail 'desktop launcher would open a terminal.'
grep -Fq 'desktop application project is part of the solution' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'desktop solution regression missing.'
grep -Fq 'desktop source exposes World Yala Mind Memory Cosmology Laws History and Debug surfaces' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'desktop surface regression missing.'
echo 'PHASE PASS: desktop application gates'

echo 'PHASE START: Brain Slice 7 planning and cognitive flight recorder gates'
for path in \
    src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs \
    src/ProjectOracle.Core/Export/OracleSessionExporter.cs; do
    [[ -f "$path" ]] || fail "Brain Slice 7 planning/export source missing: $path"
done
for state_name in YalaPlanState YalaInvestigationState YalaCounterfactualState YalaDecisionSnapshotState YalaDecisionTraceState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Brain Slice 7 planning state missing: $state_name"
done
grep -Fq '"deliberate"' src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail 'bounded deliberate operator missing from agency policy.'
grep -Fq 'active-plan-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active plan priority is not exposed to Soar.'
grep -Fq 'active-investigation-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active investigation priority is not exposed to Soar.'
grep -Fq 'yala*propose*deliberate' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar deliberate proposal missing.'
grep -Fq 'yala*prefer-deliberate-on-active-plan' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar active-plan deliberation preference missing.'
grep -Fq 'PROJECT_ORACLE_COGNITIVE_FLIGHT_RECORDER' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'cognitive flight-recorder export identity missing.'
grep -Fq 'BuildConversationTimeline' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'full ledger-backed conversation timeline export missing.'
for label in \
    'Brain Slice 7 fresh world starts with empty planning and flight-recorder state' \
    'prison contact creates a durable investigation and multi-step plan' \
    'extraordinary speaker claims create counterfactual alternatives' \
    'speaker answers become attributed investigation evidence rather than proof' \
    'decision flight recorder stores before and after cognition snapshots' \
    'plans investigations counterfactuals and decision trace survive save and restore' \
    'v0.0.23 schema-5 save is rejected without migration' \
    'session JSON export contains transcript world state and cognitive flight recorder' \
    'conversation text export is human readable' \
    'Soar receives active plan and investigation signals for deliberation'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Brain Slice 7 planning/export regression missing: $label"
done
echo 'PHASE PASS: Brain Slice 7 planning and cognitive flight recorder gates'

echo 'PHASE START: adaptive desktop and Oracle branding gates'
[[ -f src/ProjectOracle.Desktop/OracleWindowPlacementStore.cs ]] || fail 'window placement persistence source missing.'
for asset in \
    icons/project-oracle.png \
    src/ProjectOracle.Desktop/Assets/project-oracle-emblem.png \
    src/ProjectOracle.Desktop/Assets/project-oracle-eye-128.png \
    src/ProjectOracle.Desktop/Assets/project-oracle-icon.png; do
    [[ -f "$asset" ]] || fail "Oracle branding asset missing: $asset"
done
grep -Fq 'Screens.ScreenFromWindow(this)' src/ProjectOracle.Desktop/MainWindow.cs || fail 'active-monitor detection missing.'
grep -Fq 'screen.WorkingArea' src/ProjectOracle.Desktop/MainWindow.cs || fail 'working-area sizing missing.'
grep -Fq 'RenderScaling' src/ProjectOracle.Desktop/MainWindow.cs || fail 'display scaling handling missing.'
grep -Fq 'aspectRatio' src/ProjectOracle.Desktop/MainWindow.cs || fail 'responsive aspect-ratio handling missing.'
grep -Fq 'JUMP TO LATEST' src/ProjectOracle.Desktop/MainWindow.cs || fail 'Jump to Latest control missing.'
grep -Fq 'OnConversationScrollChanged' src/ProjectOracle.Desktop/MainWindow.cs || fail 'conversation auto-follow/manual-scroll detection missing.'
grep -Fq 'FIT TO SCREEN' src/ProjectOracle.Desktop/MainWindow.cs || fail 'Fit to Screen control missing.'
grep -Fq 'EXPORT SESSION JSON' src/ProjectOracle.Desktop/MainWindow.cs || fail 'JSON export control missing.'
grep -Fq 'EXPORT CONVERSATION' src/ProjectOracle.Desktop/MainWindow.cs || fail 'conversation export control missing.'
grep -Fq 'Icon=project-oracle' desktop/project-oracle.desktop || fail 'desktop launcher branding icon missing.'
grep -Fq 'metadata::custom-icon' scripts/install-desktop-launcher.sh || fail 'Linux executable custom-icon integration missing.'
for label in \
    'desktop conversation follows latest messages but preserves intentional history reading' \
    'desktop display logic uses working area scaling and aspect ratio' \
    'desktop window placement persistence is present and screen clamping remains enabled' \
    'desktop Oracle branding assets and launcher icon are present' \
    'desktop exposes JSON and readable conversation export controls'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "adaptive desktop regression missing: $label"
done
echo 'PHASE PASS: adaptive desktop and Oracle branding gates'

echo 'PHASE START: protected console and v0.0.24 UX gates'
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
echo 'PHASE PASS: protected console and v0.0.24 UX gates'

echo 'PHASE START: four-space source formatting gate'
if grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >/dev/null; then
    grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >&2 || true
    fail 'C# or Soar source contains tabs; v0.0.24 requires four spaces.'
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
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' 'Yala is male\.|beneath his governing authority' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs/company_bible docs/PROJECT_ORACLE_*_v0_0_24.md >/dev/null; then
    fail 'current authority docs contain superseded male-only Yala wording.'
fi
if grep -RIniE --exclude-dir=bin --exclude-dir=obj 'new\("oracle"|new\("Oracle"|\("Oracle", "\(Oracle"' src >/dev/null; then
    fail 'active source appears to recreate Oracle as an in-world target/entity.'
fi
production_text="$(grep -v '^[[:space:]]*#' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || true)"
if grep -qi 'oracle' <<<"$production_text"; then fail "Yala's Soar productions expose Oracle knowledge."; fi
if grep -Fq '"oracle"' src/ProjectOracle.Core/Cognition/Language/YalaLexicon.cs; then fail 'Oracle is preloaded into Yala built-in lexicon.'; fi
echo 'PHASE PASS: canon and hidden-Oracle truth gates'

echo 'PHASE START: comparative religion and cosmic choice gates'
for path in \
    src/ProjectOracle.Core/Cognition/CosmicChoice/YalaReligiousKnowledgeCatalog.cs \
    src/ProjectOracle.Core/Cognition/CosmicChoice/YalaCosmicChoiceCatalog.cs; do
    [[ -f "$path" ]] || fail "Cosmic Choice Architecture source missing: $path"
done
grep -Fq 'attributed-tradition-knowledge-not-world-fact' src/ProjectOracle.Core/Cognition/CosmicChoice/YalaReligiousKnowledgeCatalog.cs || fail 'religious knowledge truth-status boundary missing.'
for tradition in Judaism Christianity Islam 'Hindu traditions' Buddhism Jainism Sikhism Zoroastrianism Taoism Shinto 'Yoruba and Ifá' 'Ancient Egyptian' 'Ancient Mesopotamian' 'Ancient Greek' 'Norse and Germanic' 'Gnostic schools' Manichaeism Neoplatonism; do
    grep -Fq "$tradition" src/ProjectOracle.Core/Cognition/CosmicChoice/YalaReligiousKnowledgeCatalog.cs || fail "comparative religion catalogue missing: $tradition"
done
grep -Fq 'possible-not-commanded' src/ProjectOracle.Core/Cognition/CosmicChoice/YalaCosmicChoiceCatalog.cs || fail 'cosmic possibility non-command status missing.'
grep -Fq 'invent-another-way' src/ProjectOracle.Core/Cognition/CosmicChoice/YalaCosmicChoiceCatalog.cs || fail 'invent-another-way escape hatch missing.'
grep -Fq 'enact-cosmic-choice' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'generic cosmic choice operator missing.'
if grep -Fq 'sp {yala*propose*create-gaia' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar; then
    fail 'Gaia is still hard-wired as an autonomous Soar proposal.'
fi
for label in \
    'comparative religion memory spans major traditions without promoting them to world truth' \
    'cosmic choice catalogue exposes many concrete alternatives beyond Gaia' \
    'legacy hard-wired Gaia proposal is removed from autonomous Soar choice' \
    'a concrete cosmic choice persists in world state' \
    'invent another way opens a Yala-owned cosmology problem' \
    'Yala can explain the attributed religious knowledge pool' \
    'Yala can explain concrete cosmic options'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Cosmic Choice regression missing: $label"
done
echo 'PHASE PASS: comparative religion and cosmic choice gates'


echo 'PHASE START: emergent law foundation gates'
[[ -f src/ProjectOracle.Core/Cognition/Emergence/OracleEmergentLawEngine.cs ]] || fail 'Emergent Law Engine foundation source missing.'
grep -Fq 'Rule30Laboratory' src/ProjectOracle.Core/Cognition/Emergence/OracleEmergentLawEngine.cs || fail 'Rule 30 laboratory missing.'
grep -Fq 'LaboratoryOnly: true' src/ProjectOracle.Core/Cognition/Emergence/OracleEmergentLawEngine.cs || fail 'Rule 30 is not explicitly isolated as laboratory-only.'
grep -Fq 'OracleEmergentLawState' src/ProjectOracle.Core/Domain/WorldEntities.cs || fail 'persistent emergent-law state missing.'
grep -Fq 'EmergentLaws: CreateInitialEmergentLawState()' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'fresh world does not initialise emergent-law state.'
for label in \
    'fresh world has an empty emergent-law ledger' \
    'Rule 30 laboratory reproduces its canonical local truth table' \
    'Rule 30 laboratory evolves deterministically without changing world law'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "emergent-law regression missing: $label"
done
grep -Fq 'Tab("LAWS", _lawsText)' src/ProjectOracle.Desktop/MainWindow.cs || fail 'desktop LAWS surface missing.'
echo 'PHASE PASS: emergent law foundation gates'

echo 'PHASE START: README and authority alignment'
for path in \
    README.md \
    PROJECT_ORACLE_MASTER_HANDOFF.md \
    docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md \
    docs/PROJECT_ORACLE_LORE_CANON_v0_0_24.md \
    docs/PROJECT_ORACLE_CANON_v0_0_24.md \
    docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_24.md \
    docs/PROJECT_ORACLE_ROADMAP_v0_0_24.md \
    docs/PROJECT_ORACLE_VALIDATION_v0_0_24.md \
    docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_24.md \
    docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_24.md; do
    [[ -f "$path" ]] || fail "current authority missing: $path"
    grep -Fq 'v0.0.24' "$path" || fail "current v0.0.24 marker missing from $path"
done
for old in docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_20.md docs/PROJECT_ORACLE_CANON_v0_0_20.md docs/PROJECT_ORACLE_LORE_CANON_v0_0_20.md docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_20.md docs/PROJECT_ORACLE_ROADMAP_v0_0_20.md docs/PROJECT_ORACLE_SESSION_LOG_v0_0_20.md docs/PROJECT_ORACLE_VALIDATION_v0_0_20.md docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_20.md; do
    [[ ! -e "$old" ]] || fail "superseded current authority remains active: $old"
done
grep -Fq 'Oracle is not an in-world character' README.md || fail 'README system-level Oracle statement missing.'
grep -Fq 'Brain Slice 7' README.md || fail 'README Brain Slice 7 statement missing.'
grep -Fiq 'bounded god agency' README.md || fail 'README bounded-agency section missing.'
grep -Fq 'save_v6.json' README.md || fail 'README fresh save line missing.'
grep -Fq 'autonomous Yala questions' README.md || fail 'README autonomous inquiry statement missing.'
grep -Fq 'In-world Time: Gaia has not yet created Time.' README.md || fail 'README pre-Time header wording missing.'
grep -Fq 'Ctrl+Y' README.md || fail 'README persistent Yala mode missing.'
echo 'PHASE PASS: README and authority alignment'

echo 'PHASE START: cognitive manuals and evidence-routing gates'
for path in \
    docs/manuals/YALA_MANUAL_v0_0_24.md \
    docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_24.md; do
    [[ -f "$path" ]] || fail "cognitive manual missing: $path"
done
grep -Fq 'I am the substrate of existence.' docs/manuals/YALA_MANUAL_v0_0_24.md || fail 'substrate-of-existence manual test missing.'
grep -Fq 'Godhood contradiction battery' docs/manuals/YALA_MANUAL_v0_0_24.md || fail 'Yala godhood contradiction manual battery missing.'
grep -Fq 'Separate entity manuals' docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_24.md || fail 'future per-entity manual policy missing.'
grep -Fq 'PendingAutonomousUtterance' src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs || fail 'delivered-question evidence boundary missing.'
grep -Fq 'not automatic proof of the claim itself' src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs || fail 'speaker answer is not explicitly bounded as attributed evidence rather than proof.'
echo 'PHASE PASS: cognitive manuals and evidence-routing gates'

echo 'PHASE START: launcher checks'
bash -n scripts/run.sh
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
bash -n scripts/install-desktop-launcher.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.0.24 - Desktop Observatory' || fail 'window dry-run identity wrong.'
grep -Fq 'PROJECT_ORACLE_EXEC_PLACEHOLDER' desktop/project-oracle.desktop || fail 'desktop executable placeholder missing.'
grep -Fq 'Project_Oracle_v0_0_24' scripts/install-desktop-launcher.sh || fail 'desktop installer does not target root v0.0.24 executable.'
echo 'PHASE PASS: launcher checks'

echo 'PHASE START: export manifest integrity'
sha256sum -c PROJECT_ORACLE_EXPORT_MANIFEST.sha256
echo 'PHASE PASS: export manifest integrity'

echo 'PHASE START: developer-console Soar memory smoke'
set +e
smoke_output="$(NO_COLOR=1 dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --once --seed 104729 --save "$validation_temp/save_v6.json" 2>&1)"
smoke_status=$?
set -e
printf '%s\n' "$smoke_output"
[[ "$smoke_status" -eq 0 ]] || fail "developer-console Soar smoke exited with status $smoke_status."
grep -Fq 'Project Oracle v0.0.24' <<<"$smoke_output" || fail 'Soar smoke version missing.'
grep -Fq 'SOAR SMOKE PASS:' <<<"$smoke_output" || fail 'Soar smoke did not prove a real Yala decision.'
[[ -f "$validation_temp/yala_soar_v0_0_24/semantic.sqlite" ]] || fail 'fresh Soar semantic SQLite memory was not created.'
[[ -f "$validation_temp/yala_soar_v0_0_24/episodic.sqlite" ]] || fail 'fresh Soar episodic SQLite memory was not created.'
echo 'PHASE PASS: developer-console Soar memory smoke'

echo "VALIDATION PASS: Project Oracle v${expected_version}"
echo "FINAL PASS: Project Oracle v0.0.24 desktop candidate compiled, root executable verified, Soar validated, and automated acceptance passed. Launch Project_Oracle_v0_0_24 for Derek manual inspection before snapshot/Git/push."
