# Project Oracle Session Log — v0.0.16

Date: 10 August 2026

## Why this build exists

Manual inspection prevented three bad candidates from becoming accepted history:

- v0.1.11 failed release and observation truth gates;
- v0.1.12 passed automated repair validation but exposed the false Green Life entity/domain split;
- v0.1.13 passed 59 automated checks but the live console rejected an existing v0.1.12 save.

- v0.1.14 compiled cleanly, then failed at acceptance-test execution because one authority-caveat assertion still required the obsolete phrase `may claim`; the validator also hid failing test output because command substitution exited under `set -e` before printing it.

The lore discussion then substantially corrected the cosmology: Oracle is no longer Yala, Eden is a prison, Oracle is the serpent and living Master Key, Gaia rules the elements, the elements control weather, Yala did not create ordinary animals, and Sophia/Yala jointly originate humanoids.

## v0.0.16 response

This repair candidate:

- carries the observation repairs from rejected v0.1.12;
- carries the Gaia/Green-Life correction forward while removing Green Life language as a category entirely;
- separates Oracle from Yala in state, direct address, records, answers, Company Bible, canon, and tests;
- adds the dedicated lore authority document;
- supports saves through 0.1.13;
- adds a real Linux Applications-menu launcher and `project-oracle` executable command;
- keeps the manual application inspection gate ahead of snapshots and Git;
- repairs the authority-caveat regression to assert the actual current invariant instead of obsolete wording;
- makes acceptance-test output visible even when the test executable returns failure, before installer rollback.

## Open lore deliberately preserved

- final four-or-five elemental roster;
- exact ordinary-animal creator inside Gaia/elemental branch;
- exact language origin;
- exact mechanics/timing of Sophia becoming Deception;
- whether Yala later turns against Deception;
- final origin mechanism of Oracle beyond the current emergent-Master-Key working concept.

## v0.1.15 failed validation

- Restore/build passed warnings-as-errors.
- Acceptance result: **60 passed; 4 failed**.
- Failures were all observation/attention default or persistence checks.
- Root cause: `GardenState.Name` had drifted from stable `the Garden` to `Eden / the Garden`.
- Installer rollback restored accepted v0.1.10.

## v0.0.16 response

- Restore `GardenState.Name` to `the Garden` for new worlds and normalised saves.
- Keep Eden as the lore/prison identity in creation records and lore documents.
- Add a dedicated stable Garden identity regression.
- Keep complete acceptance failure output visible before rollback.
