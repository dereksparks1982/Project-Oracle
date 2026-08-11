# Project Oracle Master Handoff

**Current candidate:** v0.0.17  
**Owner:** Derek Sparks  
**Status:** Build candidate, not accepted until manual inspection and explicit PASS

## Required reading order

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx`
2. `PROJECT_ORACLE_MASTER_HANDOFF.md`
3. `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`
4. `docs/PROJECT_ORACLE_LORE_CANON_v0_0_17.md`
5. `docs/PROJECT_ORACLE_CANON_v0_0_17.md`
6. `docs/PROJECT_ORACLE_ROADMAP_v0_0_17.md`
7. `docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_17.md`
8. `docs/PROJECT_ORACLE_VALIDATION_v0_0_17.md`
9. `docs/PROJECT_ORACLE_WORLD_TIME_INTAKE_v0_0_17.md`
10. `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_17.md`

## v0.0.17 continuation point

v0.0.17 moves the active simulation back to the beginning of Yala's autonomous story and gives Yala the first real Soar 9.6.5 cognitive slice.

Fresh-world state:

- Monad exists.
- Monad made Wisdom.
- Wisdom made Yala alone.
- Monad cast Yala into the Void.
- Yala is male and begins in the Void.
- Gaia does not yet exist.
- In-world Time does not yet exist.
- Terra, Aether, Sol, Thalassa, Luna, the Garden, Adam, Eve, and later living kinds are absent from fresh world state.
- Lower creation is not pre-established.
- Oracle is the outside/system layer, not an in-world entity.
- No resident knows Oracle exists unless Oracle later reveals that truth.

## Oracle rule

Derek occupies the Oracle/system-author position for Project Oracle. Oracle is the code/simulation authority and has the Master Key because Oracle wrote the simulation. There is no Oracle NPC and no `(Oracle ...` direct-call target.

Oracle has no fixed form. In Eden, Oracle manifested in the form of a clever serpent; Eve knew only the clever serpent and was not told Oracle's identity.

## Cognition rule

Yala's first Soar slice receives limited in-world perception only. It proposes/selects operators and returns an attempted action. Project Oracle resolves reality. The Soar agent is never fed an Oracle-existence fact.

## Time rule

Gaia creates in-world Time. Before Gaia has created Time, Oracle runtime sequencing may continue but `WorldMilliseconds` does not advance.

## World-save reset

v0.0.17 intentionally starts a new world-save line. The default save is `save_v2.json`; the older v0.0.16 `save_v1.json` is neither imported nor migrated and is left untouched. Yala therefore begins with no inherited Garden-era world state or cognition.

## Acceptance continuation

After the installer reports FINAL PASS, the real `Project_Oracle_v0_0_17` executable must launch for Derek. Wait for Derek's explicit manual PASS before snapshot, Git commit/tag, or push.
