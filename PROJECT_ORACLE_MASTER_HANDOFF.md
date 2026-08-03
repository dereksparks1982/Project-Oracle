# Project Oracle — Cumulative Master Handoff

Date: 3 August 2026  
Current candidate: v0.1.10 — Oracle/Yala Identity, Decision Output, and First Brain Planner  
Required baseline: Project Oracle v0.1.7 validated candidate, branch `main`, clean tree after Derek acceptance  
Creator and final creative authority: Derek Sparks  
Technical collaborator: ChatGPT / Codex

## Required first action in every thread

Read, end to end:

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx` from Concept Build 0.
2. This cumulative handoff.
3. `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`.
4. `docs/PROJECT_ORACLE_CANON_v0_1_10.md`.
5. `docs/PROJECT_ORACLE_ROADMAP_v0_1_10.md`.
6. `docs/PROJECT_ORACLE_DEMON_KILLER_WORLD_TIME_INTAKE_v0_1_10.md`.
7. `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_1_10.md`.
8. The complete current external technical authority only when future implementation authority, syntax, packaging, or migration work is relevant.

Do not propose or build from memory. Do not change code until Derek approves an exact scope.

## Project identity

Project Oracle is a standalone autonomous-world simulation, artificial-life experiment, historical generator, and observer-driven god simulation. There is no player character inside the world. The Creators remain outside it.

## Current creation hierarchy

- The Creators made Yala.
- Yala is known to inhabitants as the Oracle and normally presents herself as Supreme God.
- The official creation order is: 0 Void; 1 Yala; 2 Sol; 3 Gaia and Aether; 4 Thalassa; 5 Luna; 6 World; 7 Green Life; 8 Garden; 9 Adam; 10 Living Kinds.
- The void exists as Yala's prison, and the Creators threw Yala into the void to see what she would do with that prison.
- Yala created Sol and the other powers, demi-gods under a demi-god.
- The world exists before plants; the Garden is created just before Adam.
- The World Record is Creator-facing creation history, not Adam's knowledge.
- Yala knows the order but may claim that she rules all or created all; protected Creator records outrank her claim.
- The Creators placed the protected Spark in Adam.
- Yala cannot read or rewrite the Spark.
- Adam begins alone inside a closed Garden.
- Adam receives the task of naming the ancient living kinds and seeing whether any is a suitable mate.
- Sol, Gaia, Aether, Thalassa, Luna, and Green Life are world powers or demigod-like governing entities, not mere scenery.
- Gaia governs earth-body and ordinary land/growth systems; Aether governs breath-space; Thalassa governs water; Sol governs first light/fire/heat/time; Luna governs night measure/tides/reflected light.
- Natural Course is active: if nobody intervenes, created beings follow their appointed nature, memory, needs, duties, and planned course.
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

## v0.1.10 implementation

The candidate is a C# 14 / .NET 10 console scaffold. It carries everything from v0.1.9 and adds:

- explicit Oracle/Yala same-identity state, migration, and deterministic answer wording;
- `<oracle>` answers that speak as Yala in first person instead of describing Yala as a separate being;
- exactly twelve first living kinds;
- visible Adam decision output for direct address and vessel speech;
- a saved internal HTN-style first brain planner inspired by Fluid HTN concepts;
- reasoned plan records before Adam direct-address response, vessel-speech response, and naming;
- `plans` / `brain` console inspection;
- a retro black-and-green console theme with highlighted special words for real terminals;
- redirected-output and `NO_COLOR` safety so validation stays plain text;
- a resume handshake file for long-thread continuation;
- forty-eight acceptance checks.

The first brain planner is a working deterministic scaffold, not a full autonomous mind or language model. No external Fluid HTN code is vendored in v0.1.10.

## v0.1.9 implementation carried forward

The v0.1.9 candidate carried everything from v0.1.7 and added:

- a nonblocking interactive input loop so the live console can refresh while waiting for commands;
- fixed live status display with world time, sky state, active address channel, pending event count, and offered-choice count;
- terminal-title refresh for the live Garden console;
- louder `run-window.sh` failure reporting if no separate terminal can open;
- saved creation-order records for Void, Yala, Sol, Gaia, Aether, Thalassa, Luna, World, Green Life, Garden, Adam, and Living Kinds;
- save normalisation so v0.1.7 saves gain the current creation-power defaults;
- Yala authority caveat recording that she may overclaim but Creator records outrank the claim;
- `creation` and `powers` console commands;
- first deterministic Oracle answers for creation-order, world-power, Yala-claim, and Adam first-knowing questions;
- forty-two acceptance checks.

The Oracle answers are an interpreter scaffold, not Yala's autonomous brain.

## v0.1.7 implementation carried forward

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
- deterministic ancient living kinds generated from the World Seed;
- direct address channels for `F1 <oracle>`, `F2 <gaia>`, `F3 <adam>`, `F4 <sun>`, and `F5 <moon>`;
- Creator-recorded address handling that does not speak through the Oracle, execute miracles, or puppet Adam;
- Adam's active naming mandate and first `present next` scaffold;
- the Natural Course rule;
- an interactive Creator console;
- compatibility loading for save versions `0.1.1`, `0.1.2`, `0.1.3`, `0.1.4`, `0.1.5`, `0.1.6`, `0.1.7`, rejected `0.1.8`, `0.1.9`, and current `0.1.10`;
- forty-one acceptance checks;
- a long-range roadmap and future implementation requirements register;
- Project Oracle's own canonical Company Bible at `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`;
- a project law that Codex must check the Oracle Company Bible before guessing;
- physical `F1` through `F5` channel capture in the interactive console;
- a deterministic scheduled-event queue with stable ordering by scheduled world time, priority, and event id;
- saved scheduled events and offered choices that remain compatible with older saves;
- automatic solar turning events for dawn, day, dusk, and night;
- Creator vessel messages that become scheduled world-observable speech events;
- Adam offered-choice records with physically possible response options and a deterministic scaffold selection.

The console accepts:

```text
status
channels
life
naming
natural
creation
keywords
physical F1 through F5 to switch address channels
events
choices
present next
records world
records creator
intervene <vessel> | <message>
quit
```

The prompt shows who is being directly addressed from the Creator layer. It does not mean the Creator is speaking through that being or power.

Direct address does not yet make Adam decide. A queued vessel intervention now schedules a speech event and records a minimal offered-choice scaffold for Adam, but observation, memory, belief, emotion, consequence execution, fruit effects, punishment, and full autonomy are still deferred.

The direct-address controls are physical `F1` through `F5`. Typed `f1`, `f2`, `f3`, `f4`, and `f5` are ordinary addressed text, not channel controls.

## v0.1.10 implementation focus

This build answers Derek's manual rejection of Oracle/Yala separation, the incomplete “Adam can decide” display, the need for a first working AI brain piece, and the unreadable all-white terminal output.

It makes Oracle and Yala the same identity in state and answers, shows Adam's selected option and reason, adds saved HTN-style reasoned plans before speech/action, sets the first living-kind set to twelve, and applies a retro green console theme with highlighted special words.

The build also keeps the v0.1.9 creation/live-time correction and adds a resume handshake for long-thread recovery.

## v0.1.9 implementation focus carried forward

This build answers Derek's manual rejection of the old `v0.1.8` creation ledger and the confusing static time above the live clock. It removes the normal startup `World time:` line, keeps the moving world clock on the `LIVE` line, and records the true Creator-facing creation order before Adam appears.

It also makes the World Record explicitly Creator-facing. Adam does not read that ledger; Adam only knows what he is given, told, shown, worshipfully connected to, or able to observe. The creation order is: Void, Yala, Sol, Gaia and Aether, Thalassa, Luna, World, Green Life, Garden, Adam, and Living Kinds.

## v0.1.7 implementation focus carried forward

This build answers Derek's report that the world is too quiet by adding a lawful heartbeat rather than random filler. It introduces a deterministic event queue and the first offered-choice scaffold.

The scheduler is intentionally narrow. It handles sky turnings and queued vessel speech. Adam can be offered physically possible response options, and the Creator Record explains why the scaffold selected one option. It does not yet add memory, belief, emotion, full autonomous reasoning, consequences, fruit effects, companion creation, expulsion, exterior geography, civilisation, model calls, graphics, or database storage.

Project Oracle's Company Bible remains the project-specific authority. The external technical authority remains relevant only for future implementation authority, syntax, packaging, or migration work.

## Accepted base carried from v0.1.6

Project Oracle v0.1.6 was validated, manually run, accepted, committed, and tagged by Derek on 2 August 2026:

```text
123dbe1 (HEAD -> main, tag: v0.1.6) Accept Project Oracle v0.1.6
```

It repaired the `0.1.4` save compatibility failure and implemented physical `F1` through `F5` direct-address capture.

## Accepted base carried from v0.1.5

Project Oracle v0.1.5 was validated, accepted, committed, and tagged by Derek on 2 August 2026:

```text
34fd05c (HEAD -> main, tag: v0.1.5) Accept Project Oracle v0.1.5
```

It added Project Oracle's own Company Bible, but manual run inspection after the commit exposed the `0.1.4` save rejection and the missing physical function-key input.

## Superseded v0.1.4 candidate

Project Oracle v0.1.4 installed and validated on Derek's machine with twenty-nine passing acceptance checks, but it did not carry a Project Oracle Company Bible and still documented typed `f1` aliases in a way that could be mistaken for physical function-key support.

Accept v0.1.7 instead after validation and manual inspection.

## Superseded v0.1.3 candidate

Project Oracle v0.1.3 installed and validated on Derek's machine with twenty-eight passing acceptance checks, but the live console later refused to continue because both the primary save and last-good backup were still save version `0.1.1`.

Project Oracle v0.1.3 should not be accepted as the final checkpoint if this hotfix is applied. Accept v0.1.7 instead after validation and manual inspection.

## Accepted base carried from v0.1.2

Project Oracle v0.1.2 was validated by Derek on 2 August 2026. It passed restore, warnings-as-errors build, twenty-two acceptance tests, console smoke test, live console launcher checks, and manual checks of `help`, `status`, `records world`, and `quit` inside the separate live Garden console window.

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

No full autonomous Adam, Gaia, Aether, Thalassa, Sol, Luna, Green Life, or Yala decision engine beyond the first narrow HTN-style scaffold, observation model, memory, belief, emotion, language model, Eve, Lilith, reproduction, fruit result, expulsion, exterior world, civilisation, database, network, graphics, typed function-key fallback controls, or real plague execution exists in v0.1.10.

## Language and platform

- Code identifiers: conventional American English.
- Human-facing prose: British English.
- Prototype: C# 14 / .NET 10 LTS.
- Permanent target: future implementation after accepted language releases satisfy the recorded requirements.
- Oracle can record future implementation needs but cannot invent syntax or modify the compiler.

## Changed paths

See `PROJECT_ORACLE_CHANGED_FILES_v0_1_10.txt` in the package for the exact payload inventory.

## Validation

The build treats warnings as errors and includes a one-command validator. Builder-side validation is recorded in `docs/PROJECT_ORACLE_VALIDATION_v0_1_10.md`; Derek's install-side validation remains the acceptance gate.

```bash
cd "$HOME/DKLab/Projects/Project Oracle" && ./scripts/validate.sh
```

## Risks

- The prior v0.1.0 installer failed on a strict SDK patch pin and remains unaccepted evidence.
- Derek's local install-side validation remains required before acceptance.
- Audience filtering is an application guardrail, not yet a process-level security boundary.
- The expanded genesis hierarchy is now encoded as saved default state; alternate genesis patterns require an explicit future build.
- Creator console address channels are intentionally narrow and not yet a general natural-language interface.
- Some terminal hosts may fail to report `ConsoleKey.F1` through `ConsoleKey.F5`; if Derek's actual terminal does that, repair the terminal key-decoding path in a later numbered build without restoring typed `f1` aliases.

## Rollback

Until v0.1.10 is accepted, the rollback point is the accepted `v0.1.7` tree after Derek's local validation. The apply script backs up every replaced path and removes every newly installed v0.1.10 path if installation validation fails.

## Recommended next build

**v0.1.11 — Observation and Attention**

Proposed scope only after Derek manually accepts v0.1.10:

- Adam can perceive only events that are near, loud, directly addressed, or otherwise attended;
- Yala can observe, claim, conceal, or misrepresent within clearly recorded limits;
- World Record, Creator Record, Adam knowledge, and Adam-observed events remain separate;
- queued vessel speech can become something Adam notices without giving him Creator-only truth;
- status/inspection commands can show what Adam has observed;
- no memory, belief, emotion, companion, fruit effect, or punishment yet.

Derek must approve that exact build or replace it before work begins.
