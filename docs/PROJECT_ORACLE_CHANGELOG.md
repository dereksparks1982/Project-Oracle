# Project Oracle Changelog

## v0.1.1 — First Light SDK Roll-Forward Repair — Candidate — 2 August 2026

### Repaired

- Carried the failed v0.1.0 First Light scaffold forward from Concept Build 0 as the next unused numeric version.
- Changed `global.json` from a strict unavailable SDK patch requirement to .NET `10.0.100` with `rollForward` set to `latestFeature`.
- Updated installer, validator, tests, README, version surfaces, changed-file inventory, validation record, and handoff to report `0.1.1`.

### Failure evidence preserved

- Project Oracle v0.1.0 was not accepted. Derek's install failed during validation because the project demanded SDK `10.0.302` while the machine had .NET 10 SDK `10.0.110`. The installer rollback restored Concept Build 0.

### Added

- C# 14 / .NET 10 solution with Core, Console, and dependency-free AcceptanceTests projects.
- Frozen 64-bit deterministic random generator and integer-millisecond simulation clock.
- Four-Garden-days-per-real-day progression with exact six-real-hour days.
- Atomic single save, last-good backup, forward-only offline catch-up, and backwards-clock protection.
- Year 1 / Month 1 / Day 1 / 01:01:01 epoch with solar and lunar phases.
- Audited Demon Killer world-time intake record documenting inherited laws and rejected game-specific baggage.
- Stable entity identities for the Garden, Yala, and Adam.
- Canonical initial state: Yala made the world and Adam; the Creators placed the protected Spark.
- Separate World and Creator records with stable sequence numbers.
- Dormant future-language mandate known to Yala.
- Audited Creator vessel-message queue that does not force Adam's response.
- Interactive Creator console, validation script, architecture record, canon record, long-range roadmap, and cumulative handoff.

### Not added

- Autonomous decisions, memory, belief, AI/model calls, Eve, Lilith, reproduction, fruit effects, expulsion, exterior world, civilisation, saves, database, networking, or graphics.

### Baseline

Concept Build 0 documentation package.

### Acceptance

Not accepted until Derek installs, validates, and explicitly accepts the build.
