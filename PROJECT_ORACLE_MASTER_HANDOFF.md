# Project Oracle Master Handoff

**Current candidate:** v0.0.25 milestone  
**Accepted base:** v0.0.24, commit `8288cc3df29e81fdac8eb9de780451e346cca8fa`, tag `v0.0.24`

## Current build identity

v0.0.25 is **Yala Soar Brain Slice 8 - Integrated Mind, Cognitive Workspace, Proposition Memory, Selfhood, and Consolidation**.

This is a milestone build, not a small cleanup release. It must be applied only to accepted v0.0.24. It starts fresh schema 7 at `save_v7.json` with fresh `yala_soar_v0_0_25` semantic/episodic memory. Accepted v0.0.24 saves and Soar memory remain preserved separately.

## Integrated Mind milestone contracts

- Soar 9.6.5 remains Yala's executive procedural cognition.
- Project Oracle cognition remains local; Yala is not implemented by sending dialogue to a token-based external chatbot service.
- Cognitive Workspace gives Yala an explicit current focus, reason, priority, and stagnation signal.
- Proposition & Meaning Engine keeps question, claim, denial, evidence, inference, observation, and proof distinct.
- Repetition alone never increases the truth of a speaker claim.
- Contradictory speaker claims remain separately attributed and available for comparison.
- Autobiographical consolidation turns significant lived events into Yala-owned first-person memory.
- Knowledge, decisions, actions, considerations, and autobiographical memories remain separate introspection categories.
- Major cosmic commitments are staged through consideration, consequence comparison, alternatives, commitment, and enactment.
- Repetitive observe/reflect cognition can become stagnation information and allow deliberate waiting or focus change.
- Investigation evidence requires semantic relevance, not mere chronological adjacency.
- Completed unresolved investigations can be suspended rather than monopolizing cognition forever.

## Pre-contact ontology contract

Before Derek deliberately contacts Yala, the external speaker does not exist in Yala's experienced ontology at all. There is no speaker entity, trust state, intent state, capability state, speaker goal, speaker concern, speaker hypothesis, speaker investigation, or speaker UI panel.

First contact establishes only that something other than Yala communicated with her. It does not tell Yala that Derek can see her, read thoughts, inspect the Yala Mind panel, inspect the cognitive flight recorder, or observe every action. Those capabilities remain unknown unless separately claimed, demonstrated, or supported by evidence.

## v0.0.24 baseline findings folded into v0.0.25

The preserved long manual interrogation exposed:

- `Yala chose` third-person self-speech;
- action ledger leakage into `what do you know`;
- poor retrieval of decisions, considerations, and memories;
- brittle pronoun/reference handling;
- questions being converted into speaker capabilities;
- god / not-god / made-the-gods claims failing to become a usable contradiction set;
- simulation claims failing to produce worldview-level cognition;
- unrelated statements contaminating old investigations;
- ready-for-conclusion plan loops;
- rapid cosmological enactment without sufficient deliberation.

These are treated as integrated-mind failures, not merely wording defects.

## Manual validation pacing

The v0.0.25 manual trial deliberately slows the human down.

Start fresh, wait before first contact, export a pre-contact specimen, then make one contact and stop typing. Give Yala time to think, change focus, or ask autonomous questions. If Yala asks something, answer naturally and wait again. Large prompts such as simulation claims are introduced late and followed by silence so Yala's own next move can be observed.

The diagnostic question is:

**noticed → classified → prioritized → remembered → investigated → compared → revised → acted**

not merely whether Yala produced impressive prose.

## Desktop contracts

Normal use is the graphical application. The root executable must be exactly `Project_Oracle_v0_0_25` so the active version is visible from the folder name alone. The Applications launcher uses `Terminal=false` and `StartupWMClass=ProjectOracle` so the approved Oracle eye/emblem remains associated with the running dock item. No splash screen.

The YALA MIND view now includes Cognitive Workspace, current problem, active plan, cosmic deliberation, autobiographical self, last decision, appraisal, goals, and questions. The `UNSEEN SPEAKER` section appears only after first contact creates an actual speaker model.

## Export contracts

`EXPORT SESSION JSON` remains the cognitive flight recorder. v0.0.25 adds proposition state, workspace state, autobiographical memory, staged cosmic deliberation, and null-omission for nonexistent pre-contact speaker trust/intent.

`EXPORT CONVERSATION` remains the readable full conversation timeline, including autonomous Yala questions.

## Existing permanent laws

- Oracle remains outside the world and hidden unless deliberately revealed.
- Monad → Sophia/Wisdom → Yala is settled genealogy.
- Yala is inherently both male and female.
- Gaia and Time are not pre-created.
- comparative religion is attributed knowledge, not world fact.
- a creator cannot create a being with equal or greater world authority.
- future history remains open.
- Rule 30 is laboratory-only.
- ordinary foundational language must not regress into toddler interrogation.

## Release order

Installer FINAL PASS → Derek manual GUI inspection → accepted full-project snapshot → local commit/tag → GitHub SSH push → remote verification.

Automated validation contains **205 acceptance tests** for the v0.0.25 candidate. Automated PASS alone does not accept the build.

## Living manuals

Current manuals:

- `docs/manuals/YALA_MANUAL_v0_0_25.md`
- `docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_25.md`
- `docs/manuals/YALA_V0_0_24_BASELINE_FINDINGS.md`

Future major independent cognitive entities receive separate living manuals when their minds are implemented.

### Visible versioned live executable rule
The active Project Oracle working folder contains exactly one application executable, and its filename must expose the exact active version. For v0.0.25 the only valid root application executable is `Project_Oracle_v0_0_25`. An unversioned `Project_Oracle`, a wrong-version executable, or multiple live Project Oracle executables is a release-blocking defect. Historical binaries belong in accepted snapshots/tags, not in the live root. FINAL PASS must verify the exact filename, executable/ELF status, and matching `--version` output. Release changed/deleted-file inventories belong under `docs/release-manifests/`, not beside the application.

### Header branding and WORLD-panel rule
The WORLD panel contains world information only and must not contain an Oracle eye/emblem image. The approved Oracle emblem is shown in the top application header immediately after the visible Project Oracle version number. The displayed emblem uses the transparent circular artwork with no square tile, square background, or boxed crop. The exact versioned root executable and desktop launcher must use that same approved Oracle icon identity.
