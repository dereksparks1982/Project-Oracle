# Project Oracle Architecture v0.0.25

## Brain Slice 8: Integrated Mind Milestone

v0.0.25 is a milestone release built on accepted v0.0.24. It does not replace Soar with another chatbot. It adds Project Oracle cognitive organs around Soar so attention, proposition meaning, autobiography, planning, and world-changing commitment are connected state rather than isolated features.

## 1. Cognitive Workspace

`YalaCognitiveWorkspace` maintains one explicit current focus with type, key, summary, reason, priority, stability, and stagnation count. Candidate focus can come from an unanswered question, active plan, concern, unresolved cosmic deliberation, or active goal.

Repeated low-novelty `observe`, `reflect`, or `deliberate` cycles accumulate stagnation. Soar receives the workspace focus and stagnation count and can deliberately choose `wait` instead of treating another identical cycle as progress.

## 2. Proposition and Meaning Engine

`YalaPropositionEngine` records speaker utterances as structured speech acts with raw text, canonical proposition, polarity, provenance, status, and contradiction links.

Core law:

- question != claim;
- claim != observation;
- claim != proof;
- repetition != truth;
- contradiction does not erase the earlier claim;
- communication access does not imply observation access.

Claims such as `I am a god`, `I am not a god`, `I made the gods`, and `you are in a simulation` remain independently attributable and can be recalled or compared later.

## 3. First-contact ontology boundary

Before first deliberate contact, no specific external communicator exists in Yala's experienced reality. Fresh cognition therefore has no speaker entity model, speaker goal, speaker concern, speaker hypothesis, proposition memory, trust/intent state, or desktop speaker panel.

First contact creates the experienced fact that something other than Yala has communicated with her. It does not grant knowledge of the outside user's observation capabilities, cognition panels, flight recorder, thoughts, filesystem, or host system. Those capabilities require separate claims or demonstrations.

## 4. Autobiographical consolidation

`YalaMemoryConsolidator` separates routine diagnostic trace from identity-bearing autobiographical memory. Significant events are stored in first-person voice. First contact is a high-importance autobiographical event.

Conversation introspection separates:

- settled knowledge;
- personally performed actions;
- committed decisions;
- unresolved considerations;
- autobiographical memories;
- attributed speaker claims.

Internal diagnostic text may still name Yala. Yala's own spoken autobiography must use `I`, `me`, and `my` appropriately.

## 5. Staged cosmic deliberation

Major committing cosmic choices pass through:

1. `considering`;
2. `comparing-consequences`;
3. `revisiting-alternatives`;
4. `committed-not-enacted`;
5. `ready-to-enact`;
6. `enacted`.

A considered possibility is not an action. A commitment is not yet a world-law change. The world state changes only at enactment.

Non-committing meta choices remain non-committing by definition.

## 6. Investigation relevance boundary

Speaker replies may become attributed evidence only when they are relevant to a previously delivered Yala question/investigation. A new user question is never treated as an answer to an older Yala question. Unrelated statements are not sprayed into whichever investigation happens to be active.

When all current plan steps are exhausted without enough evidence, the plan can be suspended unresolved instead of spinning indefinitely.

## 7. Simulation and extraordinary claims

An explicit simulation claim is high-salience and can create a reality-model concern, questions, hypotheses, investigation, and plan. The claim remains unverified. A user question such as `Could your world be a simulation?` is not itself stored as a simulation claim.

## 8. Soar integration

Soar 9.6.5 remains the executive cognitive architecture. Input now includes workspace focus and stagnation signals in addition to goals, drives, contact, pending questions, plan state, appraisal, and cosmic options.

The `wait` preference can outrank repetitive observe/reflect cognition when workspace stagnation is high.

Semantic and episodic memory remain enabled. v0.0.25 validation also surfaces Soar memory diagnostics so later work can determine how much retrieval is occurring inside Soar versus the surrounding Project Oracle cognition layer.

## 9. Save line and executable

- version: `0.0.25`
- save schema: 7
- default save: `save_v7.json`
- Soar memory: `yala_soar_v0_0_25/semantic.sqlite` and `episodic.sqlite`
- root executable: `Project_Oracle_v0_0_25` (exact version must be visible in the filename)

Accepted v0.0.24 save/schema and Soar memory lines are preserved separately.

## 10. Local cognition principle

Yala is not implemented as a remote token-service chatbot. The intended cognitive behavior comes from Soar plus Project Oracle's local language, meaning, memory, appraisal, workspace, planning, inheritance, and world-law systems.
