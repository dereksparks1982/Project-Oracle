# Project Oracle Architecture — v0.1.0

## Decision

The prototype uses C# 14 on .NET 10 LTS as a console application. .NET 10 is the active LTS line as of this build. No game engine, database, network service, language model, or external package is required.

## Project boundaries

| Project | Responsibility | Forbidden responsibility |
| --- | --- | --- |
| `ProjectOracle.Core` | Domain state, deterministic time and probability, audit records, interventions | Terminal input, files, database, network, model calls |
| `ProjectOracle.Console` | Human-facing Creator console | Owning simulation truth or bypassing Core rules |
| `ProjectOracle.AcceptanceTests` | Dependency-free behavioural checks | Shipping runtime behaviour |

## Trusted separation

The `AuditLedger` stores audience on every record and exposes separately filtered World and Creator views. World-facing output receives only `WorldRecords`. Yala's true name, the Spark, the Creators, and the dormant language mandate exist only in protected state or Creator records.

This is an initial guardrail, not the final security boundary. Later builds must move trusted experiment control into a separate process before untrusted or model-generated behaviour is allowed.

## Determinism

- Seeds are unsigned 64-bit integers.
- Probability uses a locally implemented SplitMix64 algorithm with frozen constants.
- The world clock uses integer milliseconds: one real millisecond becomes four world milliseconds.
- One Garden day takes exactly six real hours; four Garden days pass in one real day.
- The epoch is Year 1, Month 1, Day 1, 01:01:01.
- A saved Unix-millisecond checkpoint allows forward-only offline catch-up after closure.
- A backwards host-clock change never rewinds the world and creates a Creator warning.
- The numbered calendar uses twelve conventional month lengths and a 365-day year in schema 1; leap rules remain deferred.
- Solar phase and an eight-stage 29.530588-day lunar cycle are derived from world time. The epoch begins during night under a new moon.
- Events receive monotonically increasing sequence numbers.
- Any future change that alters recorded outcomes requires an explicit replay-format version and migration decision.

## Intervention contract

`QueueVesselMessage` accepts a vessel description and message, records Creator contamination, and creates a world-observable approach event. It does not make the vessel speak and does not decide Adam's response. The later event scheduler and decision engine will turn the queued request into an offered choice.

## Future implementation boundary

The C# prototype is an executable reference, not a permanent implementation constraint. Code favours explicit records, integer time, deterministic algorithms, small methods, and no reflection or framework magic. Each substantive C# capability must map to an Oracle requirement in `PROJECT_ORACLE_FUTURE_IMPLEMENTATION_REQUIREMENTS_ROADMAP_v0_1.md`. Project Oracle records implementation needs and Derek retains final authority over any future language or toolchain change.

## Deferred architecture

- Full event replay logs beyond the v0.1.0 atomic single-save checkpoint
- Priority event scheduler
- Memory, observation, knowledge, belief, and decision systems
- Yala policy and autonomy
- Adam agency
- Creator command language
- Companion and reproduction systems
- Exterior world and civilisation simulation
- SQLite experiment catalogue
- Parallel isolated runs
- Optional model-assisted reasoning
- Separate trusted monitor process
