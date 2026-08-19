# Project Oracle Master Handoff

**Current candidate:** v0.0.24  
**Accepted base:** v0.0.23, commit `a2e38bc7ba68c1540b9e51dc3038369e055c0c4f`, tag `v0.0.23`

## Current build identity

v0.0.24 is **Yala Soar Brain Slice 7 - Deliberation, Planning, Investigations, Cognitive Flight Recorder, and Adaptive Desktop**.

The build must be applied only to the accepted v0.0.23 line. It starts a fresh schema-6 `save_v6.json` world and fresh `yala_soar_v0_0_24` semantic/episodic memory. The accepted v0.0.23 save remains preserved.

## Brain Slice 7 contracts

- personally significant concerns can become durable investigations;
- investigations produce multi-step plans;
- high-stakes uncertainty can produce counterfactual alternatives;
- speaker answers are evidence, not automatic truth;
- Soar receives active plan/investigation signals;
- `deliberate` is a bounded in-world operator;
- decision trace records before/after cognition snapshots and rationale;
- plans, investigations, counterfactuals, and trace survive save/restore;
- foundational ordinary language never regresses into toddler interrogation.

## Desktop contracts

Normal use is the graphical application. v0.0.24 adds monitor working-area/scaling/aspect-ratio detection, responsive sizing, window placement persistence/clamping, conversation auto-follow with intentional-scroll protection, `JUMP TO LATEST`, `FIT TO SCREEN`, JSON session export, readable transcript export, and official Oracle branding.

The root executable must be `Project_Oracle_v0_0_24`. The Applications launcher uses `Terminal=false`. The official icon is installed under the user icon theme and is also applied to the executable through GNOME/GIO custom-icon metadata where supported.

## Export contracts

`EXPORT SESSION JSON` is the cognitive flight recorder. It includes project/save identity, world seed/time, a full ledger-backed conversation timeline, decision trace, current Yala cognition, world state, records, offered choices, plans, observations, attention state, and Soar memory diagnostics.

`EXPORT CONVERSATION` writes the same full timeline as a human-readable transcript, including autonomous Yala questions rather than only recent dialogue memory. Exports default to `~/Downloads/Project Oracle Exports/`.

## Existing permanent laws

- Oracle remains outside the world and hidden unless deliberately revealed.
- Monad -> Sophia/Wisdom -> Yala is settled genealogy.
- Yala is inherently both male and female.
- Gaia and Time are not pre-created.
- comparative religion is attributed knowledge, not world fact.
- a creator cannot create a being with equal or greater world authority.
- future history remains open.
- Rule 30 is laboratory-only.

## Release order

Installer PASS -> Derek manual GUI inspection -> accepted full-project snapshot -> local commit/tag -> GitHub SSH push -> remote verification.

### v0.0.24 cognitive-manual policy

Project Oracle now carries `docs/manuals/YALA_MANUAL_v0_0_24.md` and `docs/manuals/ORACLE_MIND_ARCHITECTURE_MANUAL_v0_0_24.md`.  The Yala manual includes a post-automated-PASS manual trial battery and JSON flight-recorder review workflow.  Future major independent cognitive entities receive separate living manuals when their minds are implemented.
