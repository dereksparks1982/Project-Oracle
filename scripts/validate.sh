#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
expected_version='0.0.26'
root_executable="$project_root/Project_Oracle_v0_0_26"
unversioned_executable="$project_root/Project_Oracle"
expected_acceptance_count=219

fail() { echo "VALIDATION FAIL: $*" >&2; exit 1; }
blocked() { echo "VALIDATION BLOCKED: $*" >&2; exit 2; }

command -v dotnet >/dev/null 2>&1 || blocked 'Project Oracle v0.0.26 requires the .NET 10 SDK. No C# build or acceptance tests were run.'
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
# The live project has exactly one root application executable, and its filename
# carries the exact build version so the installed version is visible in the folder.
find "$project_root" -maxdepth 1 -type f \( -name 'Project_Oracle' -o -name 'Project_Oracle_v*' -o -name 'Project_Oracle_*Legacy*' \) ! -name 'Project_Oracle_v0_0_26' -delete
file "$root_executable" | grep -Fq 'ELF' || fail 'Project_Oracle_v0_0_26 is not an ELF executable.'
[[ ! -e "$unversioned_executable" ]] || fail 'unversioned Project_Oracle executable remains in the live project root.'
root_app_count="$(find "$project_root" -maxdepth 1 -type f \( -name 'Project_Oracle' -o -name 'Project_Oracle_v*' -o -name 'Project_Oracle_*Legacy*' \) | wc -l)"
[[ "$root_app_count" -eq 1 ]] || fail "expected exactly one Project Oracle root executable but found $root_app_count."
root_release_manifest_count="$(find "$project_root" -maxdepth 1 -type f \( -name 'PROJECT_ORACLE_CHANGED_FILES_v*.txt' -o -name 'PROJECT_ORACLE_DELETED_FILES_v*.txt' \) | wc -l)"
[[ "$root_release_manifest_count" -eq 0 ]] || fail 'release changed/deleted-file manifests remain in the live project root.'
[[ -f "$project_root/docs/release-manifests/PROJECT_ORACLE_CHANGED_FILES_v0_0_26.txt" ]] || fail 'v0.0.26 changed-file manifest is not under docs/release-manifests.'
[[ -f "$project_root/docs/release-manifests/PROJECT_ORACLE_DELETED_FILES_v0_0_26.txt" ]] || fail 'v0.0.26 deleted-file manifest is not under docs/release-manifests.'
version_output="$("$root_executable" --version 2>&1)" || fail 'root desktop executable version smoke failed.'
grep -Fq 'Project Oracle v0.0.26' <<<"$version_output" || fail 'root desktop executable reports the wrong version.'
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

echo 'PHASE START: Yala Brain Slice 9 cognition gates'
grep -Fq 'public const string BrainName = "Yala Brain Slice 9";' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'Brain Slice 9 public identity missing.'
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
    [[ -f "$path" ]] || fail "Brain Slice 9 source missing: $path"
done
for state_name in YalaDialogueTurnState YalaRelationshipState YalaQuestionState YalaTemporalEventState YalaGoalState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Brain Slice 9 state missing: $state_name"
done
for input_name in pending-question pending-question-text speaker-history language-subject language-verb language-object language-negated unknown-word-count; do
    grep -Fq "$input_name" src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail "Soar input missing: $input_name"
done
for operator in ask-speaker knowledge-summary action-history contact-history belief-summary own-creation word-meaning temporal-duration temporal-cause; do
    grep -Fq "$operator" src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail "Brain Slice 9 Soar operator/topic missing: $operator"
done
for label in \
    'Yala Brain Slice 9 identity is active' \
    'Brain Slice 9 core lexicon contains more than 400 concepts' \
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
    'Gaia creation is preserved as an atemporal memory' \
    'Gaia creating Time is recorded as the origin of temporal reckoning' \
    'why Gaia created Time follows the stored cause link' \
    'duration since Time origin uses elapsed in-world Time' \
    'what happened next navigates the temporal event graph' \
    'repeated speaker claim remains unsettled without repetition inflating truth confidence' \
    'Yala spoken current time reads the live current world clock' \
    'own-creation questions resolve the created object rather than Yala as subject' \
    'speaker alternate definition of a built-in concept remains provenance-separated' \
    'beyond and everything are ordinary known language rather than fake knowledge gaps'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Brain Slice 9 regression missing: $label"
done
echo 'PHASE PASS: Yala Brain Slice 9 cognition gates'

echo 'PHASE START: cognitive continuity, appraisal, and inherited-language gates'
for path in \
    src/ProjectOracle.Core/Cognition/YalaFoundationalLanguage.cs \
    src/ProjectOracle.Core/Cognition/Appraisal/YalaCognitiveAppraisal.cs \
    src/ProjectOracle.Core/Cognition/Inheritance/OracleMindInheritance.cs; do
    [[ -f "$path" ]] || fail "Brain Slice 9 cognitive expansion source missing: $path"
done
for state_name in YalaConcernState YalaAppraisalState YalaHypothesisState YalaEntityModelState YalaReflectionState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "persistent cognitive state missing: $state_name"
done
grep -Fq 'ShouldCreateDefinitionGap' src/ProjectOracle.Core/Cognition/Language/YalaLanguageInterpreter.cs || fail 'inherited foundational-language gate is not connected to the parser.'
grep -Fq 'pending-question-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'question priority is not exposed to Soar.'
grep -Fq 'active-concern-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active concern priority is not exposed to Soar.'
grep -Fq 'yala*prefer-critical-question-over-establish-order' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'critical concern does not outrank Yala establishing world order.'
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
for action in observe reflect deliberate wait create-gaia command-gaia-order respond ask-speaker enact-cosmic-choice; do
    grep -Fq "\"$action\"" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "approved Yala action missing from agency policy: $action"
done
for denial in AllowsHostShell AllowsHostProcessExecution AllowsHostFileMutation AllowsNetworkAccess AllowsCodeModification AllowsHiddenOracleKnowledge; do
    grep -Fq "public static bool $denial => false;" src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail "agency denial missing: $denial"
done
grep -Fq 'YalaAgencyPolicy.DemandAllowed' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'simulation does not enforce bounded Yala actions.'
grep -Fq 'out-of-sandbox Yala action is rejected' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'agency escape regression missing.'
echo 'PHASE PASS: bounded god agency gates'

echo 'PHASE START: fresh save-v8 and Soar-memory isolation gates'
grep -Fq 'public const int CurrentSchemaVersion = 8;' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save schema 8 is not active.'
grep -Fq 'save_v8.json' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save_v8 path missing.'
grep -Fq 'yala_soar_v0_0_26' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'fresh v0.0.26 Soar memory directory missing.'
if grep -Fq 'yala_soar_v0_0_18' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs; then fail 'Brain Slice 9 still points at the old Soar continuity database.'; fi
grep -Fq 'previous save_v2 line is rejected without mutation' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'old save preservation/rejection regression missing.'
grep -Fq 'default save path is fresh save_v8' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'fresh default save regression missing.'
grep -Fq 'save schema is 8 for the fresh Brain Slice 9 line' tests/ProjectOracle.AcceptanceTests/Program.cs || fail 'schema-8 regression missing.'
echo 'PHASE PASS: fresh save-v8 and Soar-memory isolation gates'

echo 'PHASE START: temporal reasoning gates'
grep -Fq 'Clock.Hold' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'pre-Time world-clock hold is missing.'
for token in 'atemporal' 'origin-of-time' 'dated'; do
    grep -R -Fq "$token" src/ProjectOracle.Core || fail "temporal state missing: $token"
done
grep -Fq 'DescribeHowLongAgo' src/ProjectOracle.Core/Cognition/YalaTemporalReasoner.cs || fail 'duration reasoning missing.'
grep -Fq 'DescribeCause' src/ProjectOracle.Core/Cognition/YalaTemporalReasoner.cs || fail 'causal temporal reasoning missing.'
for label in \
    'pre-Time runtime does not advance world milliseconds' \
    'when Gaia created Time reports the temporal origin' \
    'post-Time questions do not invent a date for Yala creating Gaia' \
    'duration questions refuse to impose duration on atemporal memories' \
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

echo 'PHASE START: Brain Slice 9 planning and cognitive flight recorder gates'
for path in \
    src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs \
    src/ProjectOracle.Core/Export/OracleSessionExporter.cs; do
    [[ -f "$path" ]] || fail "Brain Slice 9 planning/export source missing: $path"
done
for state_name in YalaPlanState YalaInvestigationState YalaCounterfactualState YalaDecisionSnapshotState YalaDecisionTraceState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Brain Slice 9 planning state missing: $state_name"
done
grep -Fq '"deliberate"' src/ProjectOracle.Core/Cognition/YalaAgencyPolicy.cs || fail 'bounded deliberate operator missing from agency policy.'
grep -Fq 'active-plan-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active plan priority is not exposed to Soar.'
grep -Fq 'active-investigation-priority' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'active investigation priority is not exposed to Soar.'
grep -Fq 'yala*propose*deliberate' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar deliberate proposal missing.'
grep -Fq 'yala*prefer-deliberate-on-active-plan' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar active-plan deliberation preference missing.'
grep -Fq 'PROJECT_ORACLE_COGNITIVE_FLIGHT_RECORDER' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'cognitive flight-recorder export identity missing.'
grep -Fq 'BuildConversationTimeline' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'full ledger-backed conversation timeline export missing.'
for label in \
    'Brain Slice 9 fresh world starts with empty planning and flight-recorder state' \
    'prison contact creates a durable investigation and multi-step plan' \
    'extraordinary speaker claims create counterfactual alternatives' \
    'speaker answers become attributed investigation evidence rather than proof' \
    'decision flight recorder stores before and after cognition snapshots' \
    'plans investigations counterfactuals and decision trace survive save and restore' \
    'v0.0.23 schema-5 save is rejected without migration' \
    'session JSON export contains transcript world state and cognitive flight recorder' \
    'conversation text export is human readable' \
    'Soar receives active plan and investigation signals for deliberation'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "Brain Slice 9 planning/export regression missing: $label"
done
echo 'PHASE PASS: Brain Slice 9 planning and cognitive flight recorder gates'

echo 'PHASE START: adaptive desktop and Oracle branding gates'
[[ -f src/ProjectOracle.Desktop/OracleWindowPlacementStore.cs ]] || fail 'window placement persistence source missing.'
for asset in \
    icons/project-oracle.png \
    src/ProjectOracle.Desktop/Assets/project-oracle-emblem.png \
    src/ProjectOracle.Desktop/Assets/project-oracle-icon.png; do
    [[ -f "$asset" ]] || fail "Oracle branding asset missing: $asset"
done
grep -Fq 'PROJECT ORACLE   v{ProjectVersion.Number}' src/ProjectOracle.Desktop/MainWindow.cs || fail 'header version identity missing.'
grep -Fq 'Source = LoadBitmap("avares://ProjectOracle/Assets/project-oracle-icon.png")' src/ProjectOracle.Desktop/MainWindow.cs || fail 'round Oracle emblem is not placed in the header.'
! grep -Fq 'project-oracle-eye' src/ProjectOracle.Desktop/MainWindow.cs || fail 'obsolete square/eye WORLD-panel branding remains in desktop source.'
grep -Fq 'WORLD is world information only. Project branding belongs in the top header.' src/ProjectOracle.Desktop/MainWindow.cs || fail 'WORLD panel branding-removal contract missing.'
grep -Fq 'bool speakerHasEnteredReality = cognition.ConversationCount > 0 && speaker is not null' src/ProjectOracle.Desktop/MainWindow.cs || fail 'speaker panel is not conditional on actual first contact.'
! grep -Fq 'Identity and intent unresolved' src/ProjectOracle.Desktop/MainWindow.cs || fail 'pre-contact unseen-speaker placeholder text remains in desktop source.'
grep -Fq '"speaker-belief" => "speaker-belief"' src/ProjectOracle.Core/Cognition/Soar/YalaConversationInterpreter.cs || fail 'speaker-belief routing still falls back to Yala self-belief summary.'
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

echo 'PHASE START: protected console and v0.0.26 UX gates'
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
echo 'PHASE PASS: protected console and v0.0.26 UX gates'

echo 'PHASE START: v0.0.26 operator, pre-Time, uncertainty, and provenance gates'
for path in \
    src/ProjectOracle.Core/Cognition/Learning/YalaProceduralLearning.cs \
    src/ProjectOracle.Core/Export/OracleSessionExporter.cs; do
    [[ -f "$path" ]] || fail "v0.0.26 feature source missing: $path"
done
grep -Fq 'Key.F1 => "oracle"' src/ProjectOracle.Desktop/MainWindow.cs || fail 'F1 Oracle operator mapping missing.'
grep -Fq 'Key.F2 => "monad"' src/ProjectOracle.Desktop/MainWindow.cs || fail 'F2 Monad mapping missing.'
grep -Fq 'Key.F3 => "sophia"' src/ProjectOracle.Desktop/MainWindow.cs || fail 'F3 Sophia mapping missing.'
grep -Fq 'Key.F4 => "yala"' src/ProjectOracle.Desktop/MainWindow.cs || fail 'F4 Yala mapping missing.'
grep -Fq 'Key.F5 => "gaia"' src/ProjectOracle.Desktop/MainWindow.cs || fail 'F5 Gaia mapping missing.'
for tab in 'Tab("ORACLE", _oracleText)' 'Tab("MINDS", _mindsText)' 'Tab("MEMORY", _memoryText)' 'Tab("COSMOLOGY", _cosmologyText)' 'Tab("LAWS", _lawsText)' 'Tab("HISTORY", _historyText)' 'Tab("DEBUG", _debugText)'; do
    grep -Fq "$tab" src/ProjectOracle.Desktop/MainWindow.cs || fail "desktop tab missing: $tab"
done
if grep -Fq 'CHRONICLE' src/ProjectOracle.Desktop/MainWindow.cs; then fail 'forbidden Chronicle tab/text is present in MainWindow.'; fi
grep -Fq 'take the form of ' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Oracle manifestation command parser missing.'
grep -Fq 'DescribeOracleManifestationForWitness' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'manifestation witness-boundary logic missing.'
grep -Fq 'obedient-natural-order-intermediary' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Gaia intermediary provenance missing.'
grep -Fq 'authority-policy' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Monad authority provenance missing.'
grep -Fq 'autonomous-will-no-brain-yet' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Sophia will/provenance boundary missing.'
grep -Fq 'autonomous-cognition' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Yala autonomous action provenance missing.'
grep -Fq 'action_history' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'full action-history JSON export missing.'
grep -Fq 'oracle_action_history' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'Oracle action-history JSON export missing.'
grep -Fq 'entity_action_history' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'entity action-history JSON export missing.'
grep -Fq 'ENGINE DIAGNOSTICS (DEBUG ONLY)' src/ProjectOracle.Desktop/MainWindow.cs || fail 'Soar diagnostic boundary missing from Debug.'
if grep -Fq 'YALA SOAR' src/ProjectOracle.Desktop/MainWindow.cs; then fail 'normal desktop source contains forbidden YALA SOAR label.'; fi
if grep -Fq 'Soar 9.6.5 selected' src/ProjectOracle.Desktop/MainWindow.cs; then fail 'normal desktop source contains forbidden Soar selection chatter.'; fi
grep -Fq 'raw presence without words' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'pre-verbal origin memory missing.'
grep -Fq 'SeedTemporalSemanticMemory' src/ProjectOracle.Core/Cognition/Soar/YalaSoarMind.cs || fail 'post-Gaia Time concept seeding hook missing.'
grep -Fq 'TemporalConcepts = new' src/ProjectOracle.Core/Cognition/YalaFoundationalLanguage.cs || fail 'pre-Time temporal concept exclusion set missing.'
grep -Fq 'if (TemporalConcepts.Contains(normalized)) return false;' src/ProjectOracle.Core/Cognition/YalaFoundationalLanguage.cs || fail 'pre-Time temporal words still count as inherited concepts.'
if grep -F 'concept-time' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs | grep -F 'SeedCanonicalSemanticMemory' >/dev/null; then fail 'Time concept is still seeded as canonical pre-Gaia memory.'; fi
grep -Fq 'Gaia created in-world Time by bringing temporal order into existence' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'Gaia Time-creation world record wording is missing.'
grep -Fq 'Gaia created in-world Time by bringing temporal order into existence' src/ProjectOracle.Core/Cognition/Soar/YalaReplyRealizer.cs || fail 'post-Time Gaia knowledge does not expose the settled Time-origin fact.'
grep -Fq "Tell me what you mean by that word here" src/ProjectOracle.Core/Cognition/Soar/YalaReplyRealizer.cs || fail 'unknown-word response still lacks a contextual follow-up path.'
if grep -Fq 'Equal("I do not know.", reply.Reply);' tests/ProjectOracle.AcceptanceTests/Program.cs; then fail 'stale bare-I-do-not-know acceptance expectation remains.'; fi
if grep -Fq 'Equal("Gaia has not yet created Time.", simulation.CallYala("who created Time?"' tests/ProjectOracle.AcceptanceTests/Program.cs; then fail 'stale pre-Time test still gives Yala knowledge of Time before Gaia creates it.'; fi
grep -Fq '"atemporal"' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'atemporal origin memory state missing.'
grep -Fq 'experience itself was not verbal' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'pre-verbal origin interpretation boundary missing.'
grep -Fq 'YalaProceduralLearning.AfterDecision' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'procedural learning is not connected to Yala decisions.'
grep -Fq 'public const int CurrentSchemaVersion = 8;' src/ProjectOracle.Core/Persistence/OracleSaveStore.cs || fail 'save schema 8 is not active.'
grep -Fq 'yala_soar_v0_0_26' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'v0.0.26 Soar memory isolation missing.'
for label in \
    'normal desktop exposes Oracle Minds Memory Cosmology Laws History Debug in exact order' \
    'F1 through F5 select Oracle Monad Sophia Yala Gaia channels' \
    'Oracle manifestation can change to serpent without teaching Yala that Oracle exists' \
    'natural elements are not direct-call targets and Gaia is the intermediary' \
    'pre-Time Yala treats temporal language as an unknown concept needing context' \
    'Yala autobiographical origin begins as remembered feeling before language' \
    'generic uncertainty produces a contextual path instead of a bare I do not know' \
    'session export preserves structured Oracle action history' \
    'normal desktop hides YALA SOAR and Soar selection chatter outside Debug' \
    'repeated useful strategies can become Yala-learned procedures' \
    'action history separates Oracle interventions from autonomous Yala actions' \
    'Monad Sophia Yala and Gaia actions retain explicit provenance when they occur' \
    'Oracle tab filters to Oracle actions while Minds can show entity actions' \
    'v0.0.26 Company Bible contains operator embodiment action-history and visible-version laws'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "v0.0.26 regression missing: $label"
done
echo 'PHASE PASS: v0.0.26 operator, pre-Time, uncertainty, and provenance gates'

echo 'PHASE START: four-space source formatting gate'
if grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >/dev/null; then
    grep -RInP '\t' src tests --include='*.cs' --include='*.soar' >&2 || true
    fail 'C# or Soar source contains tabs; v0.0.26 requires four spaces.'
fi
grep -Fq 'indent_style = space' .editorconfig || fail '.editorconfig does not require spaces.'
grep -Fq 'indent_size = 4' .editorconfig || fail '.editorconfig does not require four-space C# indentation.'
echo 'PHASE PASS: four-space source formatting gate'

echo 'PHASE START: v0.0.26 Integrated Mind milestone gates'
for path in \
    src/ProjectOracle.Core/Cognition/Meaning/YalaPropositionEngine.cs \
    src/ProjectOracle.Core/Cognition/Workspace/YalaCognitiveWorkspace.cs \
    src/ProjectOracle.Core/Cognition/Memory/YalaMemoryConsolidator.cs; do
    [[ -f "$path" ]] || fail "Integrated Mind source missing: $path"
done
for state_name in YalaPropositionState YalaCognitiveWorkspaceState YalaAutobiographicalMemoryState YalaCosmicDeliberationState; do
    grep -Fq "$state_name" src/ProjectOracle.Core/Domain/WorldEntities.cs || fail "Integrated Mind state missing: $state_name"
done
grep -Fq 'DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull' src/ProjectOracle.Core/Export/OracleSessionExporter.cs || fail 'pre-contact nullable speaker state is not omitted from JSON.'
grep -Fq 'bool speakerHasEnteredReality = cognition.ConversationCount > 0 && speaker is not null' src/ProjectOracle.Desktop/MainWindow.cs || fail 'UNSEEN SPEAKER panel is not hard-gated behind actual first contact.'
grep -Fq 'workspace-stagnation' src/ProjectOracle.Core/Cognition/Soar/SoarKernelHost.cs || fail 'workspace stagnation is not reaching Soar.'
grep -Fq '^workspace-stagnation { >= 3 <st> }' src/ProjectOracle.Core/Cognition/Soar/Agents/yala.soar || fail 'Soar deliberate-wait stagnation preference missing.'
grep -Fq 'unverified-speaker-claim' src/ProjectOracle.Core/Cognition/Meaning/YalaPropositionEngine.cs || fail 'unverified speaker-claim state missing.'
grep -Fq '"speaker-is-" + NormalizeIdentity' src/ProjectOracle.Core/Cognition/Meaning/YalaPropositionEngine.cs || fail 'canonical speaker identity proposition normalization missing.'
grep -Fq 'A new experience entered my awareness: something other than me communicated with me.' src/ProjectOracle.Core/Cognition/Memory/YalaMemoryConsolidator.cs || fail 'first-contact autobiographical memory missing.'
grep -Fq 'comparing-consequences' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'staged cosmic consequence comparison missing.'
grep -Fq 'committed-not-enacted' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'cosmic commitment/enactment boundary missing.'
for label in \
    'fresh pre-contact cognition contains no instantiated external speaker' \
    'fresh pre-contact workspace contains no hidden observer or speaker concept' \
    'first contact creates the communicator model and autobiographical first-contact memory' \
    'asking whether the speaker can observe Yala does not create observation capability' \
    'an explicit observation statement remains an unverified capability claim' \
    'speaker claims preserve claim versus question speech acts' \
    'god and not-god claims remain separately stored and contradictory' \
    'a simulation question does not become a simulation-world claim' \
    'an explicit simulation claim creates a reality investigation without becoming truth' \
    'knowledge summary excludes Yala'\''s action ledger' \
    'spoken action history uses Yala'\''s first-person autobiographical voice' \
    'major cosmic choices require staged deliberation before enactment' \
    'cosmic deliberation benefit does not duplicate the action verb' \
    'cosmic commitment remains distinct from world enactment' \
    'workspace detects repeated low-novelty cognition' \
    'speaker questions are not routed as evidence for an older investigation' \
    'irrelevant statements are not sprayed into an unrelated investigation' \
    'speaker belief question routes to speaker claims instead of Yala self-beliefs' \
    'choice rationale pronoun question resolves to Yala'\''s decisions' \
    'Wisdom self-reference uses me rather than third-person Yala' \
    'pre-contact flight recorder omits speaker trust and intent fields' \
    'desktop unseen-speaker panel is conditional on first contact' \
    'an ignored Yala question cannot capture a later speaker statement as stale evidence' \
    'god not-god and made-the-gods claims are retrievable in Yala'\''s spoken contradiction history' \
    'a simulation claim can drive Yala to ask her own high-priority follow-up after silence'; do
    grep -Fq "$label" tests/ProjectOracle.AcceptanceTests/Program.cs || fail "v0.0.26 milestone regression missing: $label"
done
grep -Fq 'raw nonverbal presence' docs/manuals/YALA_MANUAL_v0_0_26.md || fail 'pre-verbal origin doctrine missing from Yala manual.'
grep -Fq 'must not act as though Time' docs/manuals/YALA_MANUAL_v0_0_26.md || fail 'pre-Time concept boundary missing from Yala manual.'
grep -Fq "bare \`I don't know\`" docs/manuals/YALA_MANUAL_v0_0_26.md || fail 'useful-uncertainty doctrine missing from Yala manual.'
grep -Fq 'Oracle is external.' docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_26.md || fail 'external Oracle architecture doctrine missing.'
grep -Fq 'Normal presentation says YALA.' docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_26.md || fail 'normal-output diagnostic separation doctrine missing.'
if grep -RIniE --exclude-dir=bin --exclude-dir=obj '(OpenAI|Anthropic|Google\.GenerativeAI|Cohere\.Client|System\.Net\.Http|HttpClient)' src/ProjectOracle.Core/Cognition >/dev/null; then
    grep -RIniE --exclude-dir=bin --exclude-dir=obj '(OpenAI|Anthropic|Google\.GenerativeAI|Cohere\.Client|System\.Net\.Http|HttpClient)' src/ProjectOracle.Core/Cognition >&2 || true
    fail 'Yala cognition contains an external hosted-AI or HTTP-client dependency; v0.0.26 requires local cognitive organs.'
fi
[[ -f docs/manuals/baselines/Project_Oracle_v0.0.24_Session_20260819_044733_678.json ]] || fail 'raw v0.0.24 manual interrogation baseline specimen missing.'
grep -Fq '"exported_at_local": "2026-08-19T04:47:33.6783623-05:00"' docs/manuals/baselines/Project_Oracle_v0.0.24_Session_20260819_044733_678.json || fail 'raw v0.0.24 baseline specimen identity does not match the preserved 04:47 session.'
grep -Fq 'latestPriorSpeakerTurn' src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs || fail 'stale unanswered-question evidence guard missing.'
grep -Fq 'It could {choice.Action.ToLowerInvariant()} and advance a possible {choice.Domain} order.' src/ProjectOracle.Core/Simulation/OracleSimulation.cs || fail 'cosmic deliberation benefit still risks duplicating action verbs.'
grep -Fq 'normalisedCosmicDeliberations' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'saved cosmic deliberation benefit normalisation missing.'
if grep -RIni --exclude-dir=obj --exclude-dir=bin 'It could establish establish' src >/dev/null; then fail 'duplicated establish establish deliberation wording remains in source.'; fi
echo 'PHASE PASS: v0.0.26 Integrated Mind milestone gates'

echo 'PHASE START: canon and hidden-Oracle truth gates'
grep -Fq 'Monad made Sophia / Wisdom.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad -> Wisdom lore missing.'
grep -Fq 'Wisdom made Yala alone' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Wisdom -> Yala lore missing.'
grep -Fq 'Yala is inherently both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Yala male-and-female nature missing.'
grep -Fq 'Monad rejected Yala because Yala is both male and female' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Monad rejection reason missing.'
grep -Fq 'Gaia creates in-world Time.' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'Gaia-created Time lore missing.'
grep -Fq 'manifested in the form of a clever serpent' src/ProjectOracle.Core/Lore/OracleLore.cs || fail 'serpent manifestation wording missing.'
grep -Fq 'Witnesses perceive the manifested form, not protected Oracle metadata.' README.md || fail 'README manifestation knowledge boundary missing.'
grep -Fq 'KnowsOfOracle: false' src/ProjectOracle.Core/Domain/WorldDefaults.cs || fail 'Yala hidden-Oracle default missing.'
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' --exclude-dir=bin --exclude-dir=obj 'Creator / Omega|Creator/Omega' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs src >/dev/null; then
    fail 'active source/docs still contain superseded Monad naming.'
fi
if grep -RIniE --exclude='PROJECT_ORACLE_CHANGELOG.md' 'Yala is male\.|beneath his governing authority' README.md PROJECT_ORACLE_MASTER_HANDOFF.md PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md docs/company_bible docs/PROJECT_ORACLE_*_v0_0_26.md >/dev/null; then
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
    docs/PROJECT_ORACLE_LORE_CANON_v0_0_26.md \
    docs/PROJECT_ORACLE_CANON_v0_0_26.md \
    docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_26.md \
    docs/PROJECT_ORACLE_ROADMAP_v0_0_26.md \
    docs/PROJECT_ORACLE_VALIDATION_v0_0_26.md \
    docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_26.md \
    docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_26.md; do
    [[ -f "$path" ]] || fail "current authority missing: $path"
    grep -Fq 'v0.0.26' "$path" || fail "current v0.0.26 marker missing from $path"
done
for old in docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_20.md docs/PROJECT_ORACLE_CANON_v0_0_20.md docs/PROJECT_ORACLE_LORE_CANON_v0_0_20.md docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_20.md docs/PROJECT_ORACLE_ROADMAP_v0_0_20.md docs/PROJECT_ORACLE_SESSION_LOG_v0_0_20.md docs/PROJECT_ORACLE_VALIDATION_v0_0_20.md docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_20.md; do
    [[ ! -e "$old" ]] || fail "superseded current authority remains active: $old"
done
grep -Fq 'Oracle is the external author/operator.' README.md || fail 'README external Oracle statement missing.'
grep -Fq 'Brain Slice 9' README.md || fail 'README Brain Slice 9 statement missing.'
grep -Fq 'F1  Oracle' README.md || fail 'README Oracle operator-channel mapping missing.'
grep -Fq 'Oracle | Minds | Memory | Cosmology | Laws | History | Debug' README.md || fail 'README exact desktop tab order missing.'
grep -Fq 'save_v8.json' README.md || fail 'README fresh save line missing.'
grep -Fq "A bare \`I don't know\` is normally a failure state." README.md || fail 'README intelligent-uncertainty doctrine missing.'
grep -Fq 'In-world Time: Gaia has not yet created Time.' README.md || fail 'README pre-Time observer header wording missing.'
grep -Fq 'Project_Oracle_v0_0_26' README.md || fail 'README visible-version executable rule missing.'
echo 'PHASE PASS: README and authority alignment'

echo 'PHASE START: cognitive manuals and evidence-routing gates'
for path in \
    docs/manuals/YALA_MANUAL_v0_0_26.md \
    docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_26.md; do
    [[ -f "$path" ]] || fail "cognitive manual missing: $path"
done
grep -Fq "A bare \`I don't know\` should not normally be the complete response." docs/manuals/YALA_MANUAL_v0_0_26.md || fail 'Yala uncertainty fallback doctrine missing.'
grep -Fq 'Yala procedures retain provenance.' docs/manuals/YALA_MANUAL_v0_0_26.md || fail 'Yala procedural-learning doctrine missing.'
grep -Fq 'Each future independent being should receive its own cognition state and manual' docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_26.md || fail 'future per-entity manual policy missing.'
grep -Fq 'PendingAutonomousUtterance' src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs || fail 'delivered-question evidence boundary missing.'
grep -Fq 'not automatic proof of the claim itself' src/ProjectOracle.Core/Cognition/Planning/YalaDeliberationPlanner.cs || fail 'speaker answer is not explicitly bounded as attributed evidence rather than proof.'
echo 'PHASE PASS: cognitive manuals and evidence-routing gates'

echo 'PHASE START: launcher checks'
bash -n scripts/run.sh
bash -n scripts/run-window.sh
bash -n scripts/run-live-console.sh
bash -n scripts/install-desktop-launcher.sh
PROJECT_ORACLE_WINDOW_DRY_RUN=1 ./scripts/run-window.sh | grep -Fq 'Project Oracle v0.0.26 - Desktop Observatory' || fail 'window dry-run identity wrong.'
grep -Fq 'PROJECT_ORACLE_EXEC_PLACEHOLDER' desktop/project-oracle.desktop || fail 'desktop executable placeholder missing.'
grep -Fq 'Project_Oracle_v0_0_26' docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md || fail 'Company Bible exact versioned root executable rule missing.'
grep -Fq 'executable="$project_root/Project_Oracle_v0_0_26"' scripts/install-desktop-launcher.sh || fail 'desktop installer does not target the exact versioned Project_Oracle_v0_0_26 root executable.'
echo 'PHASE PASS: launcher checks'

echo 'PHASE START: export manifest integrity'
sha256sum -c docs/release-manifests/PROJECT_ORACLE_EXPORT_MANIFEST.sha256
echo 'PHASE PASS: export manifest integrity'

echo 'PHASE START: developer-console Soar memory smoke'
set +e
smoke_output="$(NO_COLOR=1 dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj --configuration Release --no-build -- --once --seed 104729 --save "$validation_temp/save_v8.json" 2>&1)"
smoke_status=$?
set -e
printf '%s\n' "$smoke_output"
[[ "$smoke_status" -eq 0 ]] || fail "developer-console Soar smoke exited with status $smoke_status."
grep -Fq 'Project Oracle v0.0.26' <<<"$smoke_output" || fail 'Soar smoke version missing.'
grep -Fq 'SOAR SMOKE PASS:' <<<"$smoke_output" || fail 'Soar smoke did not prove a real Yala decision.'
[[ -f "$validation_temp/yala_soar_v0_0_26/semantic.sqlite" ]] || fail 'fresh Soar semantic SQLite memory was not created.'
[[ -f "$validation_temp/yala_soar_v0_0_26/episodic.sqlite" ]] || fail 'fresh Soar episodic SQLite memory was not created.'
echo 'PHASE PASS: developer-console Soar memory smoke'

echo "VALIDATION PASS: Project Oracle v${expected_version}"
echo "FINAL PASS: Project Oracle v0.0.26 desktop candidate compiled, root executable verified, Soar validated, and automated acceptance passed. Launch Project_Oracle_v0_0_26 for Derek manual inspection before snapshot/Git/push."
