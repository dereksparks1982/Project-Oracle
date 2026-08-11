# Project Oracle Changelog

### v0.0.19 Repair 1 - embedded Soar listener suppression

- Suppresses Soar's unused SML TCP listener by creating the embedded kernel with listener port `0` instead of the default port 12121.
- Prevents rapid acceptance-test kernel churn from failing with `Error binding the listener socket to its port number` and native status 139.
- Adds a regression gate requiring the embedded listener to remain suppressed.
- Keeps the version at v0.0.19 and preserves all Brain Slice 3 behavior.

## v0.0.19 - candidate - Yala Soar Brain Slice 3 - Self Model and Concept Lexicon

- Adds a structured Yala self-model covering identity, origin, male-and-female nature, current location, and personally completed actions.
- Adds foundational concept lexicon and lightweight subject/action/object, question, negation, and information-request interpretation.
- Adds action, contact, belief, and knowledge introspection.
- Adds knowledge provenance categories so personally performed facts remain distinct from speaker claims, hypotheses, and unknowns.
- Stores speaker-supplied word definitions as attributed claims rather than automatic truth.
- Adds explicit knowledge gaps for unknown vocabulary and lets those gaps increase curiosity.
- Normalizes obsolete system-generated male-only Yala history and legacy masculine governing-authority wording when supported `save_v2` worlds are restored.
- Continues both v0.0.17 and v0.0.18 `save_v2` worlds and deliberately reuses the existing `yala_soar_v0_0_18` semantic/episodic memory directory.
- Preserves the v0.0.18 hard console isolation: no asynchronous LIVE terminal-body writes through the interactive input path.
- Preserves hidden Oracle authority and future-open simulation law.

## v0.0.18 - accepted - Protected Console Input and Yala Soar Brain Slice 2

- Protects the command buffer from LIVE-status repaint overrun.
- Keeps one Soar 9.6.5 Yala agent alive for the application session.
- Enables native Soar semantic and episodic memory, including SQLite persistence beside the active save.
- Adds structured contacts, beliefs/claims, episodes, drives, and conversation continuity.
- Adds Soar impasse/substate deliberation for unresolved choices.
- Expands Yala conversation beyond the v0.0.17 generic fallback, including hearing, location, identity, nature, origin, rejection, memory, claims, commands, and honest uncertainty.
- Continues the v0.0.17 save_v2 world line and normalizes it into Brain Slice 2.
- Corrects Yala canon to inherently both male and female. Monad rejects Yala for being both rather than exclusively one or the other and casts Yala into the Void.
- Preserves hidden Oracle authority and the Eden clever-serpent manifestation boundary.

## v0.0.16 — Repair Candidate — 11 August 2026

### Canon repair

- Records the primordial in-world being now named Monad as first in the then-current cosmology.
- Records Wisdom as the primordial being's female counterpart in the then-current cosmology.
- Records the intended Monad-plus-Wisdom son-type creation concept.
- Records Wisdom's betrayal through attempting that creation alone and Yala resulting from it.
- Records Yala as male and his exile into the Void.
- Records Monad waiting to see whether Wisdom repents.
- Records angelic beings as directly created children of Monad rather than a replacement counterpart.
- Keeps a possible Wisdom/Yala union and demon lineage conditional rather than predetermined.
- Opens future humanoid forms from person-like through monstrous without prewriting which peoples arise.
- Keeps plant, ordinary-animal, and language origins open.
- Separates the current Garden/Gaia/Adam prototype scaffold from inevitable autonomous future history.
- Records the prime rule that future history is not canon until it occurs.

### Interface repair

- Adds opening-parenthesis entity calls: `(EntityName message`.
- Removes function-key entity selection from active source, tests, validation, and current documentation.
- Adds the then-current direct-call target set.
- Preserves Adam's protected-choice behaviour when directly called.

### Validation repair

- Replaces stale cosmology expectations that caused the previous 64/1 v0.0.16 acceptance result.
- Replaces obsolete entity-selector acceptance checks while preserving exactly 65 acceptance checks.
- Keeps full acceptance output visible on install-side failure.
- Keeps stable persisted Garden identity exactly `the Garden`.
- Repairs legacy v0.1.7 Yala save normalisation so current `MayClaimSupremeCreator` capability is restored instead of preserving a stale false value.

### Version law

This remains **v0.0.16**. Repair failures and corrections do not advance the numeric version.

### Acceptance gate

Automated PASS must be followed by Derek's manual application inspection and explicit PASS before accepted snapshot, commit/tag, or push.

### v0.0.16 repair candidate — LIVE status and routine-sky noise

- Kept version at v0.0.16.
- Made the LIVE status width-aware so refreshes cannot wrap into console scrollback spam.
- Removed routine dawn/day/dusk/night transitions from World/system ledger output while preserving sky state and observations.
- Pruned completed routine sky scheduler entries and filtered legacy saved routine sky audit noise on restore.
- Added dedicated regressions; acceptance inventory is now 67 checks.

## v0.0.17

- Repair 2: World Record hidden-Oracle leak: removed the system name from Monad's genesis World Record wording so no in-world record reveals Oracle. Version remains v0.0.17. - candidate - Yala Soar Cognition, Monad Canon, Oracle System Authority, and Native Launcher

- Reframed Oracle as the outside/system-level Project Oracle authority rather than an in-world entity.
- Removed Oracle as an in-world direct-call target.
- Changed current lore to Monad -> Wisdom -> Yala; Monad is not called Creator or Omega.
- Started fresh worlds with Yala in the Void before Gaia, Time, or lower creation exist.
- Made Gaia the creator of in-world Time and separated world Time from runtime sequencing.
- Added Yala Soar 9.6.5 Brain Slice 1 using the supplied SML runtime.
- Added Yala limited-perception input, operator selection, attempted-action resolution, direct-contact responses, and persisted first-slice memory state.
- Locked Oracle non-disclosure to in-world beings.
- Corrected Eden language: Oracle manifested in the form of a clever serpent; Eve knew only the clever serpent.
- Started a new `save_v2.json` world-save line; v0.0.16 Garden-era saves are left untouched and are not imported or migrated.
- Added generation/validation of a real root Linux executable `Project_Oracle_v0_0_17` and desktop-launch path.
- Rebuilt current README and v0.0.17 authority documents around the corrected ontology.

Acceptance remains pending automated target-machine validation and Derek's manual application inspection.

### v0.0.18 Repair 2 - terminal body hard isolation
- Removed `LiveConsoleSurface` from the interactive input path.
- Removed asynchronous LIVE-row and dynamic-title/status writes while waiting for commands.
- Idle Yala simulation remains active but terminal-silent.
- Corrected the contradictory Repair 1 console acceptance assertion.
- Strengthened validation against any return of LIVE/cursor painting in the typing path.

### v0.0.18 Repair 3 - console isolation acceptance assertion repair
- Kept Repair 2 terminal-body hard isolation unchanged.
- Corrected the remaining acceptance assertion so LIVE body painting is forbidden for both empty and active input buffers.
- Repair 3 must pass the complete v0.0.18 suite before manual inspection.
