# Project Oracle Changelog

## v0.1.10 — Oracle/Yala Identity, Decision Output, and First Brain Planner — Candidate — 3 August 2026

### Repaired

- Corrected Oracle/Yala identity so `<oracle>` answers as Yala, not as a separate witness describing Yala.
- Made Adam's decision output show offered choices, selected action, and reason.
- Set the first Garden living-kind set to exactly twelve kinds.
- Preserved the v0.1.9 Creator-facing creation record and live-time correction.

### Added

- Added an internal deterministic HTN-style brain planner inspired by the MIT-licensed Fluid HTN project.
- Added saved `ReasonedPlanState` records with goal, situation, decomposition, options, selected action, reason, and source.
- Made Adam create a brain plan before direct-address response, vessel-speech response, and naming.
- Added `plans` / `brain` console inspection.
- Added a retro black-and-green console theme for real terminal sessions.
- Added accent colours for direct-address prompts, live line, records, commands, Yala/Oracle, Adam, and world powers.
- Added `NO_COLOR` and redirected-output safety so validation and logs stay plain text.
- Added `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_1_10.md` for long-thread continuation.

### Validation

- Increased acceptance coverage to forty-eight checks.
- Added checks for reasoned brain plans before naming and direct-address choices.
- Added save/restore coverage for reasoned brain plans.
- Added source checks for the first brain planner and retro console theme.

### Not added

- No full Yala brain, full Adam memory, belief, emotion, sex, Lilith, Eve, hybrid/monster systems, fruit result, expulsion, exterior world, civilisation, model calls, database, graphics, vendored external planner code, or future implementation transition.

### Baseline

Required accepted baseline remains Project Oracle v0.1.7 unless Derek reports that v0.1.9 was accepted locally before applying this package. Project Oracle v0.1.8 remains rejected manual evidence.

## v0.1.9 — Creation Record and Live Time Correction — Candidate — 3 August 2026

### Repaired

- Corrected the genesis World Record so it begins with the void and Yala before Adam appears.
- Recorded that the Creators threw Yala into the void to see what she would do with her prison.
- Recorded that Yala created Sol and the other powers, demi-gods under a demi-god.
- Added World and Garden as explicit creation-order entries.
- Recorded the Garden as created just before Adam and the Living Kinds as created after Adam.
- Removed the confusing static startup `World time:` line above the live display.
- Reframed `records world` as the Creator-facing world ledger, not Adam's knowledge.

### Preserved

- Live console refresh, terminal title refresh, and the separate Garden console launcher.
- Physical `F1` through `F5` direct-address controls.
- Deterministic scheduled events and offered-choice records.
- Adam's first-knowing rule: he knows that he is, not that he is alive.
- Yala's overclaim caveat and Creator authority above Yala.

### Validation

- Added acceptance coverage for the exact first World Record categories: Void, Yala, Sol, Powers, World, Green Life, Garden, Adam, Living Kinds, and Mandate.
- Added acceptance coverage proving the World Record is Creator-facing while Adam's current answer set still does not know the Creators or Yala.
- Added acceptance coverage proving rejected `0.1.8` saves normalise into the corrected creation order and canonical address channels.
- Updated console smoke validation to reject the old static startup `World time:` line.

### Baseline

Required accepted baseline is Project Oracle v0.1.7. Project Oracle v0.1.8 passed automated validation but was manually rejected because the creation ledger began too late with Adam/Garden.

## v0.1.8 — Live Oracle Window and World Powers — Candidate — 2 August 2026

### Added

- Added live console refresh while waiting for input, including world time, sky state, active direct-address channel, pending event count, and offered-choice count.
- Added terminal-title refresh for the live Garden console.
- Added clearer `run-window.sh` failure reporting when no separate terminal window can open.
- Added saved `CreationPowerState` records for Yala/Void, Sol, Gaia, Aether, Thalassa, Luna, Green Life, Adam, and Living Kinds.
- Added Yala authority caveat: she knows the order but may claim she rules all or created all; protected Creator records outrank her claim.
- Added Oracle first answers for creation order, Sol, Gaia, Aether, Thalassa, Luna, Adam's rank above animals, Yala's overclaim, and Adam's first knowing.
- Added `creation` / `powers` console inspection.
- Added `0.1.7` save upgrade coverage for creation powers.

### Preserved

- Project Oracle remains the C# pre-alpha prototype/archive and future implementation reference corpus.
- Physical `F1` through `F5` remain the only channel controls.
- Typed `f1`, `f2`, `f3`, `f4`, and `f5` remain ordinary addressed text.
- World and Creator records remain separated.
- Default world output does not leak Yala's true name, Spark truth, or Creator-only truth.

### Not added

- Full Yala brain, full Adam mind, observation, memory, belief, emotion, sex, Lilith, Eve, fruit certainty, fruit effects, punishment, expulsion, exterior world, civilisation, model calls, database, graphics, or future implementation transition.

### Baseline

Project Oracle v0.1.7 validated by Derek with thirty-six passing acceptance checks.

### Acceptance

Not accepted until Derek installs, validates, runs the console, confirms the live clock updates in the separate window, checks Oracle creation-order questions under `F1`, and explicitly accepts the build.

## v0.1.7 — Deterministic Event Queue and Offered Choices — Candidate — 2 August 2026

### Added

- Added Core scheduled-event records with deterministic ordering by world time, priority, and event id.
- Added offered-choice records for Adam response scaffolding.
- Added automatic solar turning events for dawn, day, dusk, and night.
- Added scheduled vessel speech events for queued Creator interventions.
- Added `events` and `choices` console inspection commands.
- Added save/restore support for scheduled events and offered choices while preserving older save compatibility.
- Added acceptance checks for `0.1.6` save upgrade through event defaults, event ordering, vessel speech, offered choices, and persistence.

### Preserved

- Physical `F1` through `F5` direct-address controls remain the only channel controls.
- Typed `f1`, `f2`, `f3`, `f4`, and `f5` remain ordinary addressed text.
- `0.1.1` through `0.1.6` saves remain supported.
- World and Creator records remain separated.

### Not added

- Memory, belief, emotion, full autonomous mind, companion, fruit effect, punishment, expulsion, exterior world, civilisation, model calls, database, graphics, or real plague execution.

### Baseline

Project Oracle v0.1.6 accepted commit `123dbe1`, tag `v0.1.6`.

### Acceptance

Not accepted until Derek installs, validates, runs the console, inspects `events` and `choices`, checks the physical `F1` through `F5` keys still work, and explicitly accepts the build.

## v0.1.6 — Physical Function Keys and Save Migration Chain Repair — Candidate — 2 August 2026

### Repaired

- Implemented physical `F1`, `F2`, `F3`, `F4`, and `F5` channel capture in the interactive console.
- Removed typed `f1`, `f2`, `f3`, `f4`, and `f5` as channel-switch shortcuts; those inputs are now ordinary direct-address text.
- Added explicit `0.1.4` and `0.1.5` save-version support so accepted prior saves continue under `v0.1.6`.
- Added acceptance coverage for `0.1.4` save upgrade and physical function-key mapping.
- Updated version surfaces, README, validator, tests, validation record, handoff, roadmap, architecture, canon, Company Bible, and session log to `0.1.6`.

### Preserved

- Project Oracle's Company Bible remains the active authority document.
- The v0.1.3 direct-address, living-kinds, naming-mandate, and Natural Course feature scope is preserved.
- The v0.1.0 SDK-pin failure evidence remains preserved in the handoff history.

### Not added

- Autonomous decisions, memory, belief, AI/model calls, Eve, Lilith, reproduction, fruit effects, expulsion, exterior world, civilisation, database, networking, graphics, event queue, or real plague execution.

### Baseline

Project Oracle v0.1.5 accepted commit `34fd05c`, tag `v0.1.5`.

### Acceptance

Not accepted until Derek installs, validates, runs the console, checks the physical `F1` through `F5` keys, and explicitly accepts the build.

## v0.1.5 — Project Oracle Company Bible Authority — Candidate — 2 August 2026

### Added

- Added `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md` as Project Oracle's one active Company Bible.
- Added a validation check proving the Oracle Company Bible exists and preserves the no-guessing and physical-function-key rules.

### Repaired

- Replaced the generic DK LAB/external technical authority dependency in Oracle continuation instructions with a Project Oracle-specific Bible.
- Recorded that physical `F1` through `F5` keys are the intended controls and typed `f1` through `f5` aliases are only fallbacks.
- Moved the event queue proposal to a later build because raw function-key capture should be repaired first.
- Version surfaces, README, validator, tests, validation record, and handoff report `0.1.5`.

### Preserved

- The v0.1.4 legacy save compatibility hotfix is preserved.
- The v0.1.3 direct-address, living-kinds, naming-mandate, and Natural Course feature scope is preserved.
- The v0.1.0 SDK-pin failure evidence remains preserved in the handoff history.

### Not added

- Autonomous decisions, memory, belief, AI/model calls, Eve, Lilith, reproduction, fruit effects, expulsion, exterior world, civilisation, database, networking, graphics, raw terminal F-key capture implementation, event queue, or real plague execution.

### Baseline

Project Oracle v0.1.4 installed candidate.

### Acceptance

Not accepted until Derek installs, validates, and explicitly accepts the build.
