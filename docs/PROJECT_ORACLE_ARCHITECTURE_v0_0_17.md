# Project Oracle Architecture v0.0.17

## Layer model

```text
Derek / Oracle system authority
        |
        v
Project Oracle runtime
  - world law
  - records
  - persistence
  - Master Key/system intervention
  - cognition I/O boundary
        |
        v
In-world state
  Monad -> Wisdom -> Yala -> future lower creation
        |
        v
Yala Soar 9.6.5 Brain Slice 1
```

Oracle is not stored as an in-world `WorldState` entity.

## Soar boundary

`YalaSoarMind` uses the official Soar 9.6.5 C# SML bridge through a reflection host. The Linux x86-64 SML bridge/kernel supplied by the owner is vendored under `vendor/soar/9.6.5/linux-x86-64` with its license.

Each first-slice decision provides Yala with:

- location;
- whether Gaia exists;
- whether in-world Time exists;
- decision count;
- last action/result;
- optional unplaced-contact message and intent.

It does **not** provide an Oracle identity/existence fact.

The `.soar` production memory proposes/selects an operator. Project Oracle then resolves that attempt in C# world law. Soar is Yala's cognitive decision engine, not the authoritative world engine.

## Time boundary

`PersistentWorldClock.Hold` advances host-reference timestamps without advancing world milliseconds before Gaia creates Time. Once `CosmicState.TimeCreated` is true, the existing 4x world-time clock may advance.

## Fresh Void object boundary

A fresh v0.0.17 `WorldState` does not instantiate the later Garden/Adam/living-kind scaffold. Those objects are nullable/empty until future autonomous history actually establishes them. v0.0.17 does not migrate the v0.0.16 Garden-era save. This makes "Yala in the Void before lower creation" an actual state-model fact rather than a UI fiction.

## Persistence

`WorldState.YalaCognition` persists decision count, last action/result, and the first bounded memory list across save/restore. Rich Soar episodic/semantic persistence remains future work.

## Native launcher

Validation publishes `ProjectOracle.Console` as a framework-dependent Linux x64 single-file apphost and installs it at project root as:

```text
Project_Oracle_v0_0_17
```

When launched without a controlling console, the application attempts to reopen itself in GNOME Terminal first, with terminal fallbacks.
