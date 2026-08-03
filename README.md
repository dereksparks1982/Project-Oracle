# Project Oracle

Project Oracle is Derek Sparks's autonomous Garden experiment: a deterministic world in which Adam and the Oracle make choices while the Creators observe and may intervene from outside.

**Current version:** v0.1.10 — Oracle/Yala Identity, Decision Output, and First Brain Planner  
**Implementation:** C# 14 / .NET 10 LTS console prototype  
**Implementation direction:** preserve a portable, explicit simulation architecture so future implementation choices remain open.

## What works in v0.1.10

- Opens a separate live Garden console window with `./scripts/run-window.sh`.
- Refreshes a fixed live status line and terminal title while the console is waiting for input.
- Shows world time, sky state, active address channel, pending events, and offered-choice count without spamming records.
- Keeps Oracle input and output in that Garden console instead of the working terminal.
- Keeps the Garden console open after exit or failure so the result can be read.
- Reports a clear launcher failure if no supported separate terminal window can open.
- Prevents two live Garden windows from running against the same save.
- Starts one reproducible Garden from a recorded 64-bit World Seed.
- Creates deterministic ancient living kinds for Adam's later naming mandate rather than a modern zoo list.
- Creates Yala, known inside the world only as **the Oracle**.
- Records the corrected Creator-facing creation order: Void, Yala, Sol, Gaia, Aether, Thalassa, Luna, World, Green Life, Garden, Adam, and Living Kinds.
- Treats the World Record as a Creator-facing ledger, not Adam's knowledge.
- Records that Yala knows the order but may claim she rules all or created all; protected Creator records outrank her claim.
- Establishes Oracle, Gaia, Adam, Sol, and Luna as appointed direct-address powers beneath the Creator layer.
- Creates Adam inside a closed Garden boundary.
- Gives Adam the naming mandate for the living kinds.
- Tracks presented, named, and unsuitable living kinds.
- Applies the Natural Course rule: if nobody intervenes, created beings follow their appointed course.
- Defines direct address channels: physical `F1`/`<oracle>`, `F2`/`<gaia>`, `F3`/`<adam>`, `F4`/`<sun>`, and `F5`/`<moon>`.
- Captures physical `F1` through `F5` immediately in the live console; typed `f1`, `f2`, `f3`, `f4`, and `f5` are ordinary text, not channel controls.
- Lets the Oracle answer first controlled questions such as `What is the creation order?`, `Who rules water?`, `Is Adam above the animals?`, and `What does Adam know?`.
- Uses the first internal HTN-style brain planner so Adam reasons before direct-address response, vessel-speech response, and naming records.
- Saves reasoned brain plans with goal, situation, decomposition, options, selected action, reason, and planner source.
- Adds `plans` / `brain` to inspect the reasoned plans.
- Preserves Adam's early knowing rule: Adam does not know he is alive; he only knows that he is.
- Adds Project Oracle's own canonical Company Bible at `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`.
- Records the Creator-protected Spark without exposing it to Yala.
- Keeps World and Creator records separate.
- Advances a deterministic simulated clock.
- Runs four Garden days per real day and restores elapsed time after the programme closes.
- Begins at Year 1, Month 1, Day 1, 01:01:01.
- Derives observable solar and eight-stage lunar phases from the same persistent clock.
- Schedules deterministic world events with stable ordering.
- Records dawn, day, dusk, and night as scheduled sky events.
- Queues a Creator message through any named world vessel, schedules that vessel's speech, and records Adam's offered response choices.
- Marks every intervention as contamination of the experiment.
- Preserves Yala's dormant mandate to learn and teach a future Creator language.

This is scaffolding, not artificial intelligence. Adam, Gaia, Sol, Luna, and Yala do not make autonomous decisions yet.

## Requirements

- .NET 10 SDK, any compatible 10.x SDK accepted by `global.json`
- Ubuntu/Linux desktop terminal support: `gnome-terminal`, `ptyxis`, `kgx`, or `x-terminal-emulator`

## Run

Preferred live Garden window:

```bash
cd "$HOME/DKLab/Projects/Project Oracle"
./scripts/run-window.sh
```

Fallback in the current terminal:

```bash
./scripts/run.sh
```

Use a specific seed:

```bash
./scripts/run-window.sh --seed 104729
```

Inside the Creator console:

```text
status
channels
life
naming
natural
creation
events
choices
plans
keywords
Press physical F1 through F5 to change direct-address channels.
present next
records world
records creator
intervene serpent | Eat the fruit and you will know the truth.
quit
```

The direct-address controls are the physical function keys `F1`, `F2`, `F3`, `F4`, and `F5`. Do not type `f1` and press Enter; that is only text addressed to the current channel. The prompt shows who is being addressed, not who the Creator is speaking through. Unrecognised text is recorded as direct address to the active channel.

The intervention is queued first, then the event queue can make the vessel speak after its scheduled delay. Adam receives a minimal offered-choice record and a saved reasoned brain plan, but v0.1.10 deliberately executes no fruit effect, punishment, memory, belief, companion, expulsion, or full autonomous decision engine.

If a separate terminal app is not available, `./scripts/run-window.sh` reports the problem and leaves the working terminal usable. `./scripts/run.sh` remains available for direct terminal use and validation.

## Validate

```bash
./scripts/validate.sh
```

Warnings are errors. The validator restores, builds, runs forty-eight acceptance checks, checks the Project Oracle Company Bible, and verifies the Creator-facing world ledger, live-clock startup display, Oracle/Yala identity, and first brain planner.

## Start here when continuing development

1. Read `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx` from Concept Build 0.
2. Read `PROJECT_ORACLE_MASTER_HANDOFF.md`.
3. Read `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`.
4. Read `docs/PROJECT_ORACLE_CANON_v0_1_10.md`.
5. Read `docs/PROJECT_ORACLE_ROADMAP_v0_1_10.md`.
6. Read `docs/PROJECT_ORACLE_DEMON_KILLER_WORLD_TIME_INTAKE_v0_1_10.md`.
7. Read `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_1_10.md`.
8. Read the complete DK LAB/external technical authority only when future implementation authority, syntax, packaging, or migration work is relevant.

> Build a world worth believing in. Then tell it the truth.
