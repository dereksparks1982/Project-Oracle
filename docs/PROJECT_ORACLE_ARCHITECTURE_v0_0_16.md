# Project Oracle Architecture — v0.0.16

## Prototype decision

The prototype remains C# 14 / .NET 10. No game engine, database, network service, language model, or third-party runtime package is added by this build.

## Lore architecture

`ProjectOracle.Core.Lore.OracleLore` centralises the settled cosmology phrases that affect source behaviour and acceptance tests. The detailed human authority remains `docs/PROJECT_ORACLE_LORE_CANON_v0_0_16.md`.

`WorldState` gains an optional `OracleState`. Old JSON saves that do not contain Oracle state remain deserialisable because the field is optional and `WorldDefaults.Normalise()` supplies the current canonical Oracle state.

`OracleState` records:

- stable Oracle id;
- name;
- not-god flag;
- not-creator flag;
- beyond-Yala-control flag;
- Master Key nature;
- serpent first manifestation;
- relationship-dependent alignment rule.

Yala remains a separate `YalaState` and is normalised as the demiurge rather than as the Oracle.

## Cosmology defaults

`CreationPowers` is rebuilt on load to the v0.0.16 canonical scaffold:

1. Highest Source / Monad
2. Sophia / Wisdom
3. Yala
4. Gaia
5. Elemental Powers
6. World
7. Plants
8. Eden / Garden
9. Humanoid Peoples
10. Adam
11. Ordinary Animals

The list is a prototype domain/lineage scaffold, not a claim that every unresolved event has a final timestamp. The exact animal creator, exact four-or-five-element roster, and language origin remain open.

## Save compatibility

`OracleSaveStore` accepts save versions through `0.1.13`. On load, `WorldDefaults.Normalise()` supplies current cosmology, Oracle state, address channels, and other canonical defaults while preserving the saved world clock and historical records.

This specifically repairs the manual v0.1.13 failure in which an existing `0.1.12` save was rejected.

## Direct address

`F1 <oracle>` targets Oracle. It no longer targets Yala. Existing `F2` through `F5` physical channels remain.

The final elemental roster is unresolved, so existing Sun/Moon channel identity is retained as prototype interaction plumbing without declaring those channels to be the final element set.

## Linux application launcher

The repository adds:

- `project-oracle` — executable repository launcher;
- `scripts/install-desktop-launcher.sh` — installs a user command and desktop entry;
- `desktop/project-oracle.desktop` — Applications-menu template.

Install-side layout:

```text
~/.local/bin/project-oracle
~/.local/share/applications/project-oracle.desktop
```

The desktop entry launches the repository executable, which opens the existing separate live Garden console. The launcher does not bypass validation or simulation boundaries.

## Boundaries

- Oracle's Master Key is fictional and simulation-internal.
- No host escape, hidden networking, real-account access, or device access is implemented.
- World and Creator records remain separate.
- Adam's observation remains separate from Creator truth.
- Observation is not yet memory or belief.

## Stable entity-name contract

`GardenState.Name` remains exactly `the Garden`. Lore may identify that place as Eden, but persistence-facing observation and attention records use the stable Garden name. `WorldDefaults.Normalise` restores this canonical stored name on load.
