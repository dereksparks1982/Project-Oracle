# Project Oracle

Project Oracle is Derek Sparks's autonomous Garden experiment: a deterministic world in which Adam and the Oracle make choices while the Creators observe and may intervene from outside.

**Current version:** v0.1.0 — First Light Scaffolding  
**Implementation:** C# 14 / .NET 10 LTS console prototype  
**Implementation direction:** preserve a portable, explicit simulation architecture so future implementation choices remain open.

## What works in v0.1.0

- Starts one reproducible Garden from a recorded 64-bit seed.
- Creates Yala, known inside the world only as **the Oracle**.
- Creates Adam inside a closed Garden boundary.
- Records the Creator-protected Spark without exposing it to Yala or the World Record.
- Keeps World and Creator records separate.
- Advances a deterministic simulated clock.
- Runs four Garden days per real day and restores elapsed time after the programme closes.
- Begins at Year 1, Month 1, Day 1, 01:01:01.
- Derives observable solar and eight-stage lunar phases from the same persistent clock.
- Queues a Creator message through any named world vessel without forcing Adam's response.
- Marks every intervention as contamination of the experiment.
- Preserves Yala's dormant mandate to learn and teach a future Creator language.

This is scaffolding, not artificial intelligence. Adam and Yala do not make autonomous decisions yet.

## Requirements

- .NET 10 SDK, version 10.0.302 or a newer 10.0 patch
- Linux, macOS, or Windows terminal

## Run

```bash
cd "$HOME/DKLab/Projects/Project Oracle"
./scripts/run.sh
```

Use a specific seed:

```bash
./scripts/run.sh --seed 104729
```

Inside the Creator console:

```text
status
records world
records creator
intervene serpent | Eat the fruit and you will know the truth.
quit
```

The intervention is queued. v0.1.0 deliberately does not decide whether Adam accepts, refuses, delays, questions the vessel, or reports it to the Oracle.

## Validate

```bash
./scripts/validate.sh
```

Warnings are errors. The validator restores, builds, runs twenty-two acceptance checks, and performs a console secrecy smoke test.

## Start here when continuing development

1. Read `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx` from Concept Build 0.
2. Read `PROJECT_ORACLE_MASTER_HANDOFF.md`.
3. Read `docs/PROJECT_ORACLE_CANON_v0_1_0.md`.
4. Read `docs/PROJECT_ORACLE_ROADMAP_v0_1_0.md`.
5. Read the complete DK LAB Company Bible before proposing or building.

> Build a world worth believing in. Then tell it the truth.
