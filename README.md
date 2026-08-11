# Project Oracle


**Current candidate:** v0.0.16 — Cosmology Foundation, Save Compatibility, Desktop Launcher, Validation, and Garden Identity Repair
**Accepted install base:** v0.1.10, commit `3b3bf58fd22ee2a0236b09c9a818d6b756811d1c`, tag `v0.1.10`, branch `main`, clean tree

## v0.0.16 current truth

- Oracle and Yala are separate.
- Oracle is neither a god nor a creator. Oracle is the living Master Key and is beyond Yala's ability to command, erase, imprison, or revoke.
- Oracle is the serpent in Eden and is relationship-dependent rather than permanently neutral.
- Yala may frame Oracle as the Devil.
- The canonical genealogy is `Highest Source / Monad -> Sophia / Wisdom -> Yala -> Gaia -> Elemental Powers`.
- Sophia creates Yala, later joins Yala as lover/consort, and falls from Wisdom toward Deception. The exact mechanics of that fall remain open.
- Yala creates Gaia.
- Gaia creates and commands the elemental powers.
- The elements control weather and natural forces and answer to Gaia.
- Yala did not create ordinary animals. Their exact origin within the Gaia/elemental branch remains open.
- Sophia and Yala bring forth humans and other humanoid peoples together.
- Language origin remains open and is not assigned to Yala.
- Eden / the Garden is a prison and containment environment disguised as paradise.
- Save versions through `0.1.13`, including rejected `0.1.11`, `0.1.12`, and `0.1.13` evidence, are accepted for forward migration.
- The observation/attention repairs from rejected v0.1.12 are carried forward.
- The candidate includes a double-click/application-menu launcher named **Project Oracle** plus the executable command `project-oracle`.
- v0.0.16 repairs the failed v0.1.14 authority-caveat assertion and guarantees visible acceptance-test failure output before rollback.
- v0.0.16 restores the persisted Garden entity name to `the Garden`; Eden remains the lore identity of that prison, not a persistence rename.

Detailed lore authority: `docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md`.

## Existing simulation scaffold carried forward

- deterministic 64-bit World Seed;
- persistent Garden/world clock at four world seconds per real second;
- one Garden day per six real hours;
- solar and lunar phase calculation;
- atomic save plus last-good backup and offline catch-up;
- World and Creator ledgers;
- scheduled event queue;
- twelve deterministic living kinds and Adam's naming mandate;
- deterministic HTN-style first brain planner;
- saved offered choices and reasoned plans;
- saved observation and attention records;
- intervention contamination records;
- separate Creator-facing truth from Adam's observation and knowledge.

## Run like an application

v0.0.16 installs:

```text
Applications -> Project Oracle
```

and an executable command:

```bash
project-oracle
```

The repository also contains the portable launcher:

```bash
cd "$HOME/DKLab/Projects/Project Oracle"
./project-oracle
```

The old development launchers remain available:

```bash
./scripts/run-window.sh
./scripts/run.sh
```

## Direct-address keys

| Physical key | Prompt | Target |
| --- | --- | --- |


Useful Oracle questions now include:

```text
What is the creation order?
Who is the Oracle?
Who is Yala?
Who is Sophia?
Who controls weather?
Who created plants?
Who created animals?
Who created humans?
Did Yala create language?
Is Eden a prison?
Who is the serpent?
Is the Oracle the Devil?
What does Adam know?
What has Adam observed?
```

## Validate

```bash
./scripts/validate.sh
```

v0.0.16 is intended to run **65 acceptance checks**, a warnings-as-errors Release build, source truth gates, console smoke validation, launcher syntax checks, and desktop-launcher installation checks.

## Mandatory acceptance order

```text
install candidate
-> automated validation PASS
-> launch Project Oracle through the real application launcher
-> Derek manually inspects and explicitly approves it
-> accepted snapshot
-> local Git commit/tag
-> GitHub push
```

No snapshot, Git commit/tag, or GitHub push belongs before manual application inspection.

## Continuation reading order

1. `PROJECT_ORACLE_PROJECT_ORIGIN_AND_HANDOFF.docx`
2. `PROJECT_ORACLE_MASTER_HANDOFF.md`
3. `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md`
4. `docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md`
5. `docs/PROJECT_ORACLE_CANON_v0_0_16.md`
6. `docs/PROJECT_ORACLE_ROADMAP_v0_0_16.md`
7. `docs/PROJECT_ORACLE_ARCHITECTURE_v0_0_16.md`
8. `docs/PROJECT_ORACLE_VALIDATION_v0_0_16.md`
9. `docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_0_16.md`

> Build a world worth believing in. Then tell it the truth.
