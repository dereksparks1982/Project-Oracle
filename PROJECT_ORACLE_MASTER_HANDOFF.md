# Project Oracle — Cumulative Master Handoff

Date: 2 August 2026  
Current candidate: v0.1.2 — Separate Live Garden Console Window  
Accepted baseline: v0.1.1 — commit `ce58375`, tag `v0.1.1`  
Creator and final creative authority: Derek Sparks  
Technical collaborator: ChatGPT / Codex

## Required first action in every thread

Read, end to end:

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx` from Concept Build 0.
2. This cumulative handoff.
3. `docs/PROJECT_ORACLE_CANON_v0_1_2.md`.
4. `docs/PROJECT_ORACLE_ROADMAP_v0_1_2.md`.
5. `docs/PROJECT_ORACLE_DEMON_KILLER_WORLD_TIME_INTAKE_v0_1_2.md`.
6. The complete current DK LAB Company Bible.

Do not propose or build from memory. Do not change code until Derek approves an exact scope.

## Project identity

Project Oracle is a standalone autonomous-world simulation, artificial-life experiment, historical generator, and observer-driven god simulation. There is no player character inside the world. The Creators remain outside it.

## Current creation hierarchy

- The Creators made Yala.
- Yala is known to inhabitants as the Oracle and normally presents herself as Supreme God.
- Yala formed the world, Adam's body, and Adam's ordinary mind.
- The Creators placed the protected Spark in Adam.
- Yala cannot read or rewrite the Spark.
- Adam begins alone inside a closed Garden.
- The Creators may communicate only through audited interventions such as possessing a world vessel; they do not choose Adam's response.
- Yala may rebel, but the external kill switch is unreachable.
- Yala knows she will one day receive a new language to learn and teach humanity. She does not have it yet.

## Identity layers

| Audience | Name and belief |
| --- | --- |
| Creators | Yala, a created godlike intelligence |
| World | the Oracle, normally believed to be Supreme God |
| Adam at start | Does not know Yala's true name, the Creators, or the Spark |

## Optional future branches

- Lilith may exist, become Adam's wife, rebel, leave, be expelled, or never be created.
- Eve may follow Lilith, replace that branch, embody a divided-whole origin, or never exist.
- Adam may remain alone.
- Yala may rarely unite with Adam and produce Oracle-blooded descendants.
- Expulsion is conditional and may never occur.
- A Creator may speak through a serpent, another animal, a plant, dream, reflection, voice, fire, corpse, or nothing.
- Adam may refuse every temptation or revelation.

## Three Oracles that must remain separate

1. **Yala / the in-world Oracle:** autonomous, possibly deceptive or rebellious.
2. **Trusted experiment monitor:** outside the world, factual, auditable, and without Yala's agenda.
3. **Possible external language-tooling system:** separate language-tooling concept governed only by the future Project Oracle implementation.

## v0.1.2 implementation

The candidate is a C# 14 / .NET 10 console scaffold. It provides:

- separate live Garden console launch through `scripts/run-window.sh`;
- a terminal child runner through `scripts/run-live-console.sh`;
- live Oracle display isolation from the working terminal;
- single-instance protection for the live Garden console;
- exit/crash hold so the console remains readable;
- explicit EOF/non-interactive-input messaging;
- deterministic 64-bit seed and frozen probability generator;
- perpetual integer-millisecond Garden time at four world seconds per real second;
- one six-real-hour Garden day and four Garden days per real day;
- exact Year 1, Month 1, Day 1, 01:01:01 epoch;
- solar phase and eight-stage lunar phase derived from the same clock;
- atomic single save, last-good backup, and forward-only offline catch-up;
- stable identity records;
- Adam, Yala, Garden, and protected Spark initial state;
- separate World and Creator records;
- queued vessel messages with contamination evidence;
- an interactive Creator console;
- twenty-two acceptance checks;
- a long-range roadmap and future implementation requirements register.

The console accepts:

```text
status
records world
records creator
intervene <vessel> | <message>
quit
```

An intervention does not yet make Adam decide. Autonomous choice is explicitly deferred.

## Accepted base carried from v0.1.1

Project Oracle v0.1.1 was installed and validated by Derek on 2 August 2026, then committed and tagged:

```text
ce58375 (HEAD -> main, tag: v0.1.1) Accept Project Oracle v0.1.1
```

Derek's install-side validation passed restore, warnings-as-errors build, twenty-two acceptance tests, and console smoke test under .NET SDK `10.0.110`.

## Failure evidence carried from v0.1.0

Project Oracle v0.1.0 was not accepted. Derek's install reached validation, then failed because `global.json` required .NET SDK `10.0.302` while his system had a valid installed .NET 10 SDK, `10.0.110`. The installer rolled back to Concept Build 0 as designed.

Project Oracle v0.1.1 repaired that SDK pin by using .NET `10.0.100` with `rollForward` set to `latestFeature`, allowing compatible .NET 10 SDKs instead of a single unavailable patch band.

## Explicit exclusions

No autonomous Adam or Yala decision engine, memory, belief, emotion, language model, Eve, Lilith, reproduction, fruit result, expulsion, exterior world, civilisation, database, network, or graphics exists in v0.1.2.

## Language and platform

- Code identifiers: conventional American English.
- Human-facing prose: British English.
- Prototype: C# 14 / .NET 10 LTS.
- Permanent target: future implementation after accepted language releases satisfy the recorded requirements.
- Oracle can record future implementation needs but cannot invent syntax or modify the compiler.

## Changed paths

See `PROJECT_ORACLE_CHANGED_FILES_v0_1_2.txt` in the package for the exact payload inventory.

## Validation

The build treats warnings as errors and includes a one-command validator. Builder-side validation is recorded in `docs/PROJECT_ORACLE_VALIDATION_v0_1_2.md`; Derek's install-side validation remains the acceptance gate.

```bash
cd "$HOME/DKLab/Projects/Project Oracle" && ./scripts/validate.sh
```

## Risks

- The prior v0.1.0 installer failed on a strict SDK patch pin and remains unaccepted evidence.
- Derek's local install-side validation remains required before acceptance.
- Audience filtering is an application guardrail, not yet a process-level security boundary.
- The genesis hierarchy is now encoded as the default initial state; alternate genesis patterns require an explicit future build.
- Creator console commands are intentionally narrow and not yet a general natural-language interface.

## Rollback

Until v0.1.2 is accepted, the rollback point is accepted Project Oracle v0.1.1 commit/tag `ce58375` / `v0.1.1`. The apply script backs up every replaced path, restores deleted versioned paths, and removes every newly installed v0.1.2 path if installation validation fails.

## Recommended next build

**v0.1.3 — Deterministic Event Queue and Offered Choices**

Proposed scope only:

- stable priority queue with deterministic tie-breaking;
- queued Creator vessel message becomes an observable speech event;
- Adam receives a list of physically possible responses;
- a minimal choice policy may accept, refuse, delay, question, or report;
- Creator Record explains the selected response factors;
- same seed and inputs reproduce the same offered choice and outcome;
- no memory, full belief model, companion, fruit effect, or punishment yet.

Derek must approve that exact build or replace it before work begins.
