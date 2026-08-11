# Project Oracle — Cumulative Master Handoff

Date: 10 August 2026
Current candidate: v0.0.16 — Cosmology Foundation, Save Compatibility, Desktop Launcher, Validation, and Garden Identity Repair
Required accepted base: Project Oracle v0.1.10, commit `3b3bf58fd22ee2a0236b09c9a818d6b756811d1c`, tag `v0.1.10`, branch `main`, clean tree
Creator and final creative authority: Derek Sparks
Technical collaborator: ChatGPT / Codex

## Required first action in every thread

Read end to end:

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx`
2. this handoff
3. `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`
4. `docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md`
5. `docs/PROJECT_ORACLE_CANON_v0_0_16.md`
6. `docs/PROJECT_ORACLE_ROADMAP_v0_0_16.md`
7. `docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_16.md`
8. `docs/PROJECT_ORACLE_VALIDATION_v0_0_16.md`
9. `docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_16.md`
10. `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_16.md`

Do not reconstruct lore from chat memory when the lore canon answers it.

## Project identity

Project Oracle is a standalone autonomous-world simulation, artificial-life experiment, historical generator, and observer-driven god simulation. There is no conventional player character inside the world. Autonomous beings are the active agents, while the external experiment layer records and may intervene under audit.


## v0.0.16 cosmology authority

The current settled genealogy is:

```text
Highest Source / Monad
        ↓
Sophia / Wisdom
        ↓ creates
Yala
        ↓ creates
Gaia
        ↓ creates and commands
Elemental Powers
```

### Sophia / Wisdom / Deception

- Sophia begins as Wisdom.
- Sophia creates Yala.
- Sophia later joins Yala as lover/consort.
- Sophia falls toward Deception.
- Exact timing/mechanics of the fall remain open.
- Whether Yala later turns against Deception remains open.

### Yala

- Yala is a powerful created demiurge.
- Yala is cast into the void prison after her monstrous/disordered creation.
- Yala creates Gaia.
- Yala is not Oracle.
- Yala cannot control, remove, erase, imprison, or revoke Oracle.
- Yala did not create weather.
- Yala did not create ordinary animals.
- Yala is not the established creator of language.
- Sophia and Yala together bring forth humans and other humanoid peoples.

### Gaia and elements

- Gaia creates the elemental beings.
- The elements are separate entities.
- The elements answer to Gaia.
- The elements control weather and natural forces.
- The elements bring forth plants.
- There is no Green Life being/power/category.
- Final four-or-five-element roster remains open.

### Ordinary animals

- Yala did not create them.
- Their exact origin within the Gaia/elemental natural branch remains open.

### Eden

- Eden / the Garden is a beautiful containment environment and prison.
- Adam begins confined inside it.

## Oracle identity

Oracle is separate from the divine genealogy.

Oracle is:

- neither god nor creator;
- the living Master Key, not merely a holder of a key;
- beyond Yala's control or removal;
- able to cross created boundaries inside the mythology without owning the domains behind them;
- relationship- and conduct-dependent rather than permanently neutral;
- the serpent in Eden;
- a being Yala may frame as the Devil because Yala cannot control or remove her.

Oracle's final origin mechanism remains open. The current working concept is emergence as creation's living access layer rather than deliberate divine manufacture.

The Master Key is fictional and simulation-internal. It grants no host escape, real network/account access, or real-device authority.

## Direct address

| Physical key | Prompt | Target |
| --- | --- | --- |
| F1 | `<oracle>` | Oracle, not Yala |
| F2 | `<gaia>` | Gaia |
| F3 | `<adam>` | Adam |
| F4 | `<sun>` | current Sun/Sol channel |
| F5 | `<moon>` | current Moon/Luna channel |

Physical function keys remain mandatory. Typed `f1` through `f5` are ordinary addressed text.

## v0.0.16 implementation

v0.0.16 carries the full v0.1.14 cosmology/save/launcher candidate forward from accepted v0.1.10 and repairs the failed acceptance gate. The authority regression now tests current canon semantics, and validation always prints the complete acceptance-test output before returning failure.

The candidate carries the validated observation/attention repair work forward from the accepted v0.1.10 base and adds:

- `OracleState` as a saved/normalised world-state concept;
- Oracle/Yala identity separation;
- central `OracleLore` source constants for simulation-relevant canon;
- rebuilt cosmology/domain defaults;
- new Creator and World genesis records for the current cosmology;
- deterministic Oracle answers for Oracle, Yala, Sophia, Gaia, weather, plants, animals, humanoids, language, Eden, serpent, and Devil framing questions;
- explicit unresolved-answer behaviour for ordinary-animal origin, final element roster, language origin, Sophia's fall mechanics, and Oracle origin;
- compatibility for `0.1.12` and `0.1.13` saves;
- dedicated lore canon documentation;
- `project-oracle` repository executable;
- `~/.local/bin/project-oracle` user executable installed by script;
- Applications-menu `Project Oracle` desktop entry;
- manual acceptance launched through the application path rather than only the development script;
- 64 intended acceptance checks.

## Observation/attention repair carried forward

The candidate retains the repair work first attempted in failed v0.1.11 and repaired in v0.1.12:

- saved ObservationState and AttentionState;
- Adam/Yala observation separation;
- CreatorTruthHidden evidence;
- scheduled observations use scheduled event world time;
- stable `signal:unplaced-voice` identity;
- Garden focus is not a wildcard subject match;
- missing or empty attention arrays restore defaults;
- observation, attention, reasoned plans, offered choices, and scheduled events survive save/restore.

## Accepted v0.1.10 foundation carried forward

v0.1.10 remains the accepted Git baseline. It provides:

- deterministic seed and persistent world clock;
- atomic save and last-good backup;
- physical F1-F5 handling;
- scheduled event queue;
- offered-choice scaffold;
- twelve deterministic living kinds and Adam naming mandate;
- HTN-style reasoned plans;
- retro green console theme;
- separate World and Creator records.

Some v0.1.10 lore assumptions are deliberately superseded by v0.0.16, especially Oracle=Yala and Yala-created animals.

## Failed/rejected versions

### v0.1.11 — failed

Failure analysis found stale checksum truth, incorrect documented test count, scheduled-observation timestamp error, unstable voice subject identity, Garden attention wildcarding, and empty-attention migration inconsistency.

### v0.1.12 — rejected manually

Automated repair validation passed with 58 checks, but Derek's manual inspection found the Green Life canon wrong. v0.1.12 did not become accepted history.

### v0.1.13 — rejected manually

Automated validation passed with 59 checks and opened the live console, but the console failed to load an existing v0.1.12 save:

```text
Save version 0.1.12 is not supported by Project Oracle v0.1.13.
```

That failure is why v0.0.16 explicitly tests both `0.1.12` and `0.1.13` forward migration.

## Linux application launcher

v0.0.16 installs:

```text
~/.local/bin/project-oracle
~/.local/share/applications/project-oracle.desktop
```

The Applications menu should expose **Project Oracle**. The launcher opens the separate live Garden console using the existing terminal-window infrastructure.

Manual acceptance must test the installed launcher itself.

## Acceptance workflow

Mandatory order:

```text
candidate install
-> automated validation PASS
-> desktop/executable launcher install
-> launch real Project Oracle application
-> Derek manual inspection and explicit approval
-> accepted snapshot
-> local Git commit/tag
-> GitHub push
```

No snapshot, Git commit/tag, or GitHub push may precede manual inspection.

## Rollback

Until v0.0.16 is manually accepted, rollback authority remains accepted v0.1.10 at commit `3b3bf58fd22ee2a0236b09c9a818d6b756811d1c`, tag `v0.1.10`.

The installer must verify exact commit/tag/branch/clean tree and accepted-base hashes before mutation. Any automated validation failure must restore repository files and remove candidate launcher files installed during that failed attempt.

A later manual rejection consumes v0.0.16 and requires the next unused version from the accepted v0.1.10 baseline unless Derek accepts another baseline first.

## Next work

No next build is authorised by this handoff. After v0.0.16 is manually accepted or rejected, Derek decides the next scope.

## v0.0.16 Garden identity repair

v0.1.15 exposed four observation/attention persistence failures after the Garden's stored name drifted to `Eden / the Garden`. v0.0.16 restores the stable persisted name `the Garden`; Eden remains the lore identity and prison interpretation. A dedicated regression locks this contract.
