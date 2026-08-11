# Project Oracle Company Bible

**Version:** v0.0.16
**Status:** Mandatory and canonical for Project Oracle
**Project:** Project Oracle
**Owner:** Derek Sparks
**Canonical path:** `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`

> Build a world worth believing in. Then tell it the truth.

## 1. Sole authority and scope

This file is the one active Company Bible for Project Oracle. It governs Project Oracle authority, workflow, build approval, versioning, packaging, validation, continuity, and conduct.


Do not create a second active Project Oracle Company Bible, addendum, carryover file, reinforcement note, or alternate Bible. Future permanent workflow-rule changes must edit this same file in a numbered Project Oracle build.

## 2. Derek is the final authority

Derek Sparks is the owner, creator, and final creative decision-maker for Project Oracle.

- Tools advise, report, validate, and package. They do not overrule Derek.
- Codex may have engineering judgement, but owner decisions outrank taste, convention, automation, and momentum.
- If Derek corrects a misunderstanding, the correction governs immediately.
- If Derek says `stop`, all build, tool, packaging, and implementation work stops immediately.

## 3. No guessing past the Bible

When a decision point appears and the assistant thinks it is acceptable to guess, it must check this Company Bible first.

The rule is:

```text
No guessing past the Company Bible.
If the Company Bible answers it, follow it.
If it does not answer it, check the active handoff, canon, roadmap, architecture, validation records, and source.
If the records still do not answer it, ask Derek or mark it as an open decision.
```

Do not invent behaviour because it seems obvious. Do not reinterpret Derek's words into a cheaper substitute. The physical `F1` key is not the same instruction as typing the characters `f1`.

## 4. Required reading order before work

Before proposing or performing a Project Oracle build, read end to end:

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx`.
2. `PROJECT_ORACLE_MASTER_HANDOFF.md`.
3. This Company Bible.
4. The current canon document.
5. The current roadmap.
6. The current architecture document.
7. The current validation record.

Do not propose or build from memory.

## 5. Prebuild scope and approval

Before implementation, packaging, file generation, or equivalent build work, state:

- required accepted base version, commit, tag, branch, and clean-tree requirement when available;
- target numeric version and build title;
- exact purpose and behaviour;
- exact files or systems expected to be added, modified, or deleted;
- explicit exclusions;
- risks and controls;
- rollback point and method;
- validation plan;
- exact package filename.

Implementation begins only after Derek explicitly approves that stated scope with a build or patch command. Approval applies only to that scope. If the work expands, stop and obtain approval for the expanded scope.

## 6. Complete the approved scope only

Once Derek approves an exact build:

- complete every approved item unless Derek changes the scope;
- do not silently defer requested work;
- do not add unrelated work because it seems helpful;
- disclose limitations before delivery;
- finish, validate, package, and hand off the current build before starting another.

When Derek says to stop building unless he says so, treat that as active project law.

## 7. Version truth

Every build, patch, hotfix, documentation release, or package uses the next unused numeric version.

- Rejected or failed packages are not accepted baselines and their version numbers are not reused.
- A documentation-only build still advances the version when it changes active project authority.
- Active version surfaces must agree, including source constants, README, scripts, validation, changelog, session log, roadmap, architecture, canon, handoff, changed-file records, package name, and installer output.
- Version numbers in the roadmap are planned lanes, not reservations.

After Derek accepts a build, he may commit and tag it locally. Do not ask him to accept a superseded build when a later repair replaces it.

## 8. Packaging and rollback

The default deliverable is one changed-files-only ZIP.

- Provide one primary download unless Derek asks otherwise.
- Preserve project-relative paths under `Project Oracle/` in the package payload.
- Include an installer script that verifies the expected base before mutation.
- Include changed-file and deleted-file inventories.
- Do not place backup copies inside the active project tree.
- A post-mutation validation failure must roll back replaced files and remove newly installed candidate files.

## 9. Validation, manual inspection, and acceptance evidence

Warnings are failures by default. Validation must be deterministic, bounded, and appropriate to the change.

A build is not successful merely because files were written or automated tests passed. Project Oracle acceptance order is mandatory:

```text
install candidate
-> automated validation PASS
-> launch the real Project Oracle application/console
-> Derek manually inspects and explicitly approves it
-> accepted snapshot
-> local Git commit and tag
-> GitHub push
```

No accepted snapshot, Git commit, tag, or GitHub push may occur before Derek's manual console/application inspection. A failed manual inspection consumes the candidate version and keeps the last accepted Git baseline authoritative.

Report exactly what was validated and what was blocked by the builder environment. Derek's install-side validation and manual inspection are the acceptance gates when the local project toolchain or graphical session is unavailable in the builder workspace.

Failed builds, rejected packages, installation crashes, misunderstandings, and corrections remain evidence. Correct them through a later numbered build and documentation; do not scrub them out of history.

## 10. Project identity and cosmology authority

Project Oracle is a standalone autonomous-world simulation, artificial-life experiment, historical generator, and observer-driven god simulation.

- There is no player character inside the world.
- The autonomous inhabitants are the active agents.
- The current canonical genealogy is `Highest Source / Monad -> Sophia / Wisdom -> Yala -> Gaia -> Elemental Powers`.
- Sophia / Wisdom creates Yala. Sophia later falls into Deception and joins Yala as lover/consort; the exact moment and mechanism of that fall remain open unless the lore canon closes them later.
- Yala creates Gaia.
- Gaia creates and commands the elemental beings. The elements control weather and natural forces and answer to Gaia.
- The elemental powers bring forth plants. There is no `Green Life` entity, power, or category.
- Yala did not create ordinary animals. Their exact origin within the Gaia/elemental natural branch remains an explicit open canon decision.
- Sophia and Yala bring forth humans and other humanoid peoples together.
- The origin of language remains open; Yala is not its established creator.
- Eden / the Garden is a prison and containment environment disguised as paradise.
- The persisted simulation entity name for Eden remains exactly `the Garden`; lore wording must not rename that persistence identity.
- Oracle is separate from Yala. Oracle is neither a god nor a creator. Oracle is the living Master Key and exists outside the divine genealogy as an access anomaly Yala cannot command, erase, imprison, or revoke.
- Oracle is the serpent in Eden. Oracle is not permanently neutral; her treatment of beings is relationship- and conduct-dependent.
- Yala may frame Oracle as the Devil because Yala cannot control or remove her.
- Whether Yala later turns against Deception remains an open future branch.
- The Master Key concept is simulation-internal only. It never authorises Oracle or any inhabitant to access the host operating system, real accounts, networks, devices, or people.

The detailed lore authority is the current `docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md`. If a lore point is marked unresolved there, code and documentation must not invent an answer.

## 11. Direct address and control law

The intended direct-address map is:

| Physical key | Prompt | Addressed being/system |
| --- | --- | --- |
| `F1` | `<oracle>` | Oracle, the living Master Key; **not Yala** |
| `F2` | `<gaia>` | Gaia |
| `F3` | `<adam>` | Adam |
| `F4` | `<sun>` | the current Sun/Sol channel |
| `F5` | `<moon>` | the current Moon/Luna channel |

Physical function keys mean physical function keys. Typed aliases such as `f1` are not direct-address controls and must not be described as fallback equivalents.

Addressing Adam must not puppet Adam. Addressing Gaia, Sun, Moon, or Oracle must still pass through world law, records, and implemented authority. Oracle's fictional Master Key status does not bypass trusted host-process boundaries.

## 12. Language convention

- C# source identifiers and external programming APIs use standard American English.
- Human-facing prose uses British English where practical: terminal output, records, errors, menus, reports, documentation, and test descriptions.

## 13. Image permission

Do not generate, edit, or replace images unless Derek directly asks to make, create, draw, render, clean up, or edit that image.

## 14. Future maintenance

The active Project Oracle Company Bible folder must contain exactly one active Bible:

```text
docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md
```

Future mandatory workflow changes edit this file in the same numbered build that adopts them.
