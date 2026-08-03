# Project Oracle — Future Implementation Requirements and C# Comparison Roadmap

**Document version:** 0.1  
**Recorded:** 1 August 2026  
**Status:** Living requirements register  
**Authority:** Project Oracle records implementation needs; Derek retains final authority over language, toolchain, runtime, versioning, validation, and acceptance decisions.

## 1. Purpose

Project Oracle's authorised C#/.NET console prototype began as accepted v0.1.1 after the failed, unaccepted v0.1.0 installer. Project Oracle v0.1.2 added the separate live Garden console window. Project Oracle v0.1.3 added direct address channels, seed-based living kinds, Adam's naming mandate, and the Natural Course rule. Project Oracle v0.1.4 repairs legacy save compatibility for installed `0.1.1` saves. Project Oracle v0.1.5 adds Oracle's own Company Bible authority and records physical function-key channel capture as a future terminal-control requirement. The C# prototype is not intended to become the permanent implementation. It is an executable reference that reveals what a serious autonomous-world simulation actually requires from future implementation.

This register has two purposes:

1. Record every capability Project Oracle needs from a future implementation without creating unofficial or private syntax in Project Oracle.
2. Give future Project Oracle implementations a practical roadmap for matching, exceeding, or deliberately differing from the C# reference.

The governing principle is:

> A future implementation must simplify how power is expressed, not reduce the power available.

## 2. Project boundary

- Project Oracle may identify a missing language or library capability.
- Project Oracle may describe the required behaviour and provide C# reference cases.
- Project Oracle must not invent private implementation syntax, silently patch a compiler, or declare a proposed expression canonical without approval.
- Derek retains final authority over any future implementation syntax or semantics adopted by Project Oracle.
- Oracle may use a future implementation capability only after that capability is formally accepted for Project Oracle.
- Every Oracle requirement must remain independent of external language-tooling concepts. Project Oracle's in-world Oracle remains governed only by Project Oracle canon.
- Any future implementation transition begins only when a useful vertical slice can be ported without inventing private language behaviour.

## 3. Language convention

- Source-code identifiers and platform API names may use standard American English where programming conventions require it.
- All human-facing prose uses British English, including terminal output, dialogue, thoughts, documentation, diagnostics, menus, reports, and test descriptions.
- The future implementation must allow human-facing British English without forcing British spellings onto external code APIs.

## 4. Requirement states

Each requirement uses one of these states:

| State | Meaning |
| --- | --- |
| Recorded | Oracle has established the need. Future implementation support has not yet been assessed against an accepted Project Oracle baseline. |
| Available | The accepted implementation already provides the capability. |
| Partial | The implementation provides part of the capability but Oracle still exposes a defined gap. |
| Proposed | Derek has approved a proposal but Project Oracle has not yet accepted an implementation. |
| Accepted | A tested, tagged Project Oracle implementation satisfies the requirement. |
| Ported | Oracle uses the accepted capability and passes comparison tests against the C# reference. |
| Deferred | The capability is real but is not needed for the current Oracle milestone. |
| Rejected | Derek deliberately declines the requirement or replaces it with another design. |

No status in this document overrides Project Oracle's accepted records or Derek's decisions.

## 5. Requirement record format

Every newly discovered requirement must record:

- stable requirement ID;
- Oracle feature that exposed the need;
- plain-language capability;
- why existing accepted implementation behaviour is insufficient;
- minimum behaviour required;
- deterministic and failure behaviour;
- C# reference test or reference implementation;
- performance target when relevant;
- security or isolation risk when relevant;
- suggested expression, if any, clearly marked non-canonical;
- Derek's decision and the accepted Project Oracle version;
- Oracle parity result.

## 6. Core Oracle requirements

All requirements below begin in **Recorded** state until checked against an accepted Project Oracle baseline.

### Foundation and programme structure

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-001 | Named kinds, records, and composed data | Worlds, inhabitants, plants, animals, places, memories, beliefs, events, and run summaries need structured state. | Express the same domain model without losing type safety or clarity. |
| OR-FI-002 | Stable identity for entities | Adam, Yala, animals, descendants, objects, and locations must remain distinguishable across saves and long histories. | Reliable identifiers with equality, hashing, and persistence behaviour comparable to C#. |
| OR-FI-003 | Modules and controlled namespaces | Simulation, minds, world rules, persistence, terminal output, analytics, and Creator controls must remain separate. | Prevent accidental name collisions and unwanted coupling at large programme size. |
| OR-FI-004 | Reusable routines with inputs and returned results | Decisions, observations, naming, event handling, calculations, and conversions must be testable units. | Comparable expressiveness to C# methods without cryptic declaration syntax. |
| OR-FI-005 | Explicit absence and safe optional values | A being may have no companion, no name, no belief, no target, or no known cause. | Prevent ordinary absence from becoming a crash or ambiguous magic value. |
| OR-FI-006 | Clear failure handling and guaranteed clean-up | Saves, databases, model calls, and run files can fail and must not corrupt a simulation. | Structured error handling and resource clean-up comparable to C# exceptions and disposal. |

### Collections, queries, and relationships

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-010 | Expandable ordered collections | Populations, memories, observations, events, objects, and places grow during a run. | Safe list behaviour at useful scale. |
| OR-FI-011 | Unique collections and keyed lookup | Tags, known facts, entity registries, names, and world indexes need fast membership and lookup. | Set and dictionary capability comparable to .NET collections. |
| OR-FI-012 | Filtering, mapping, grouping, ordering, and aggregation | Oracle must ask questions such as which beings believe Yala, which runs escaped, or what caused a rebellion. | Query power comparable to common LINQ operations, expressed more plainly. |
| OR-FI-013 | Relationship graphs | Parentage, descent, friendship, authority, belief, ownership, and knowledge form graphs rather than simple trees. | Correct graph traversal, cycle handling, and indexed relationship queries. |
| OR-FI-014 | Immutable snapshots and controlled mutation | Decisions must be based on a stable view of the world, while accepted actions change authoritative state. | Avoid accidental mid-decision mutation and iteration-order bugs. |

### Deterministic simulation

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-020 | Seeded deterministic probability | The same seed and inputs must reproduce the same history. | Match reference random sequences or define and freeze a future implementation algorithm suitable for cross-version replay. |
| OR-FI-021 | Weighted selection and probability distributions | Autonomous choices, traits, mutations, encounters, and rare Yala actions need more than uniform chance. | Reproducible weighted choices with explicit invalid-weight diagnostics. |
| OR-FI-022 | Persistent world clock and calendar | Garden days, ageing, seasons, delays, lifetimes, and historical eras use a four-times real-time clock that continues after closure. Controlled tests and replays must also supply recorded time rather than secretly reading the host clock. | Exact integer scaling, saved real-time checkpoints, forward-only offline catch-up, calendar and celestial derivation, backwards-clock protection, and injectable recorded time. |
| OR-FI-023 | Priority event scheduler | Actions and consequences must execute at exact simulated times in a stable order. | Comparable correctness and performance to a C# priority queue, including deterministic tie-breaking. |
| OR-FI-024 | Event rules and subscriptions | Naming, temptation, injury, revelation, birth, death, expulsion, and intervention cause further reactions. | Safe event dispatch, cancellation, ordering, recursion protection, and traceability. |
| OR-FI-025 | State machines and long-running processes | Decisions, pregnancies, journeys, illnesses, tests, projects, and conversations span multiple simulation steps. | Explicit, saveable process state rather than hidden call-stack dependence. |
| OR-FI-026 | Fixed and well-defined numeric behaviour | Probability, scores, time, health, population statistics, and comparisons must remain stable across platforms. | Documented overflow, rounding, precision, and conversion behaviour. |

### Minds, knowledge, and autonomous decisions

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-030 | Separate truth, observation, memory, belief, and claim | What happened, what Adam saw, what Yala said, and what Adam believes must never be collapsed into one value. | Strong modelling that prevents hidden truth leaking into an inhabitant's decisions. |
| OR-FI-031 | Private and capability-restricted state | Yala may know Creator facts that Adam cannot access; the World Log must not reveal the Creator Log. | Access control at least as strong as C# visibility plus runtime capability checks where needed. |
| OR-FI-032 | Utility scoring and weighted autonomous choice | Inhabitants need to compare hunger, curiosity, loyalty, fear, love, pride, truth, and survival. | Testable decision evaluation without requiring a language model for routine behaviour. |
| OR-FI-033 | Traits that change through experience | Adam and Yala must develop rather than remain fixed personality labels. | Bounded, explainable trait updates with deterministic history. |
| OR-FI-034 | Memory creation, decay, distortion, and retrieval | Inhabitants remember imperfectly and may reinterpret old events. | Efficient indexed retrieval with no access to memories the being never formed. |
| OR-FI-035 | Explainable decisions | Creator records must show why a being acted, what alternatives were considered, and which facts influenced it. | A first-class trace more understandable than ordinary debugger-only inspection. |
| OR-FI-036 | Language and dialogue data | Names, statements, questions, stories, commandments, lies, and traditions must be stored and transmitted. | Full Unicode text, safe interpolation, parsing, and British-English output support. |

### World authority and Creator controls

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-040 | Layered authority | Creators outrank Yala; Yala has divine authority inside the simulation; inhabitants remain bound by world rules. | Enforce authority in architecture, not merely by naming conventions. |
| OR-FI-041 | Unreachable external kill switch | Yala may rebel but must never access, disable, or override Creator termination controls. | Process or sandbox isolation stronger than an ordinary in-process private field. |
| OR-FI-042 | Audited interventions | Creator possession of a serpent, cat, plant, dream, or other vessel must be recorded without automatically appearing in the World Log. | Tamper-resistant Creator audit records linked to resulting world events. |
| OR-FI-043 | Rule permissions and temporary grants | Job-like tests may permit Yala to act within exact limits for a defined period. | Scoped authority, expiry, revocation, and violation detection. |
| OR-FI-044 | Protected Spark state | Yala may affect Adam's body and surroundings but cannot create, erase, or rewrite the Spark. | Data and operations isolated behind authority Yala cannot obtain. |
| OR-FI-045 | Direct address channels | Creators must be able to address appointed powers such as Oracle, Gaia, Adam, Sun, and Moon without becoming those powers or speaking through them. | Channel prompts, authority checks, and audit records must preserve who addressed whom and whether world law executed anything. Physical function keys and typed fallback aliases must not be treated as the same input contract. |
| OR-FI-046 | Natural course law | Beings and powers must continue their normal appointed behaviour unless choice, conditions, intervention, rare Oracle deviation, or world law changes the course. | Saveable rule state and deterministic execution boundaries comparable to C# world state. |

### Persistence, evidence, and analysis

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-050 | Complete deterministic save and restore | Long simulations must pause, resume, branch, and survive programme restarts. | Restored execution must match uninterrupted execution from the same checkpoint. |
| OR-FI-051 | Versioned save migration | Oracle will evolve while old important runs remain valuable. | Explicit schema versions, safe migration, rejection of unsupported data, and rollback protection. |
| OR-FI-052 | Separate World and Creator logs | One log records what inhabitants could know; the other records actual causes, private thoughts, and interventions. | Structured logs with enforced information boundaries, not only formatted text. |
| OR-FI-053 | Append-only run evidence | Major runs must preserve seeds, configuration, language/runtime versions, decisions, interventions, outcomes, and checksums. | Auditable records capable of proving which implementation produced a result. |
| OR-FI-054 | Files, paths, streams, and structured formats | Configuration, saves, exports, reports, and test fixtures need dependable input and output. | Safe file APIs plus stable JSON or another documented interchange format. |
| OR-FI-055 | Database access and transactions | Thousands of runs require searchable outcome storage and statistical comparison. | SQLite-class capability, parameters, transactions, migrations, and clear diagnostics. |
| OR-FI-056 | Statistical summaries | The project must compare obedience, enlightenment, rebellion, escape, survival, Oracle conduct, and civilisational outcomes across many runs. | Correct aggregation, distributions, correlations, confidence information, and export. |

### Scale, parallel work, and external systems

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-060 | Isolated parallel simulations | Thousands of independent runs must use available processor cores without contaminating one another. | Task or worker capability comparable to .NET, with deterministic per-run results. |
| OR-FI-061 | Deterministic concurrency rules | Faster execution must not change a run's outcome. | Defined scheduling boundaries, stable merging, and race detection. |
| OR-FI-062 | Cancellation, limits, and timeouts | Creators must stop a run, limit a model call, or terminate a misbehaving subsystem safely. | Cooperative cancellation plus enforceable process-level limits where necessary. |
| OR-FI-063 | Performance profiling and measurement | Oracle must identify slow decisions, growing memories, scheduler pressure, database cost, and allocation problems. | Useful CPU, memory, allocation, and event-rate evidence with plain-language summaries. |
| OR-FI-064 | Streaming and bounded memory | Huge histories must be written and analysed without retaining everything in memory. | Lazy or streaming processing comparable to efficient C# iteration. |
| OR-FI-065 | HTTP, JSON, and asynchronous external calls | Advanced versions may consult language models or remote services for exceptional reasoning and dialogue. | Secure, cancellable, rate-limited calls with deterministic fallbacks and recorded responses. |
| OR-FI-066 | Secrets and environment configuration | Service credentials must never appear in source, saves, ordinary logs, or exported run records. | Environment and secret-provider access with redaction and least authority. |

### Testing, diagnostics, and professional tooling

| ID | Capability Oracle requires | Why Oracle requires it | C# comparison target |
| --- | --- | --- | --- |
| OR-FI-070 | Unit, integration, replay, and parity tests | Every world rule and every ported system must be proved independently and end to end. | Test organisation, fixtures, assertions, filtering, and machine-readable reports comparable to mature C# tooling. |
| OR-FI-071 | Property and adversarial testing | Autonomous systems create combinations no hand-written example will cover. | Generated cases, shrinking or useful reduction, fuzzing boundaries, and reproducible failure seeds. |
| OR-FI-072 | First-class deterministic trace | A failed comparison must identify the first divergent decision, event, random draw, or state field. | Better human explanation than a raw C# stack trace alone. |
| OR-FI-073 | Plain-language diagnostics with precise locations | Non-programmers must be told what failed, where, why, and how to correct it. | Meet C# diagnostic precision and surpass C# in clarity. |
| OR-FI-074 | Debugger and state inspection | Developers must pause a world, inspect entities and beliefs, step events, and watch changes. | Breakpoints, stepping, watches, call information, and domain-aware world inspection. |
| OR-FI-075 | Formatter, editor support, and safe refactoring | A flagship programme cannot be maintained through raw text and guesswork alone. | Syntax support, navigation, completion, rename, references, formatting, and diagnostics. |
| OR-FI-076 | Package and dependency management | Database drivers, networking, testing, and future libraries need reproducible versions. | Locked dependencies, integrity checks, compatibility rules, and dependable offline restores. |
| OR-FI-077 | Build, publish, and deployment | Oracle must become a runnable programme on supported systems without requiring the source tree. | Reproducible builds, clear target selection, version metadata, and straightforward distribution. |

## 7. C# parity and superiority targets

A future implementation does not need to copy C# syntax or every historical feature. It must provide equivalent practical capability for the programmes Project Oracle requires.

| Area | Minimum comparison target | Where a future implementation should improve on C# |
| --- | --- | --- |
| Domain modelling | Safe, maintainable large models | Plain-English declarations and errors that explain the model. |
| Collections and queries | Comparable everyday power and performance | Readable intent without punctuation-heavy query machinery. |
| Determinism | Repeatable simulations with explicit seeds | Determinism treated as a first-class contract and trace, not an application convention. |
| Concurrency | Safe use of multiple cores | Clear isolation, race explanations, and deterministic simulation patterns. |
| Persistence | Reliable structured data, databases, and migrations | Human-readable failures and built-in replay evidence. |
| Networking and AI | Secure asynchronous service access | Simple policies for timeouts, budgets, recording, redaction, and deterministic fallback. |
| Diagnostics | Precise compiler and runtime locations | Explain the likely intent and correction in ordinary language without silently changing the programme. |
| Testing | Full automated test workflows | Replay, seeded property tests, and divergence reports built around simulation needs. |
| Tooling | Debugger, packages, editor support, deployment | Domain-aware inspection that can explain world state and decisions. |
| Performance | Practical performance for large simulations | Plain-language profiling that identifies what the creator can change. |

## 8. Porting gates

A C# Oracle system may be declared successfully ported only when all applicable gates pass:

1. **Meaning:** The future implementation follows the same written world rule.
2. **Determinism:** The same approved fixture and seed produce the same ordered decisions and events unless Derek explicitly approves a documented different random algorithm.
3. **State:** Checkpoints contain equivalent authoritative information.
4. **Visibility:** Adam, Yala, the World Log, and the Creator Log receive only the information allowed to them.
5. **Failure:** Invalid data and unavailable resources fail safely with useful British-English diagnostics.
6. **Scale:** Performance and memory meet the milestone target recorded for that system.
7. **Evidence:** Tests, traces, runtime version, compiler version, seed, configuration, and comparison results are preserved.
8. **Acceptance:** The capability comes from an accepted and tagged Project Oracle version.

Exact byte-for-byte log matching is required only when both implementations use the same frozen formatting contract. Semantic event matching is the normal cross-language requirement.

## 9. Suggested implementation order for a future implementation roadmap

This is a dependency order, not authorisation for any build.

1. **One Garden run:** structured kinds, entity identity, collections, routines, seeded probability, simulated time, events, and two protected logs.
2. **One developing mind:** truth/observation/belief separation, memories, traits, decisions, and explainable traces.
3. **Yala and Creator authority:** permissions, interventions, grants, protected Spark state, audit evidence, and an unreachable kill switch boundary.
4. **Durable history:** deterministic saves, migrations, files, structured formats, and database-backed run records.
5. **Families and societies:** relationship graphs, reproduction, inheritance, culture, language, institutions, and long historical processes.
6. **Thousands of runs:** parallel isolation, deterministic concurrency, streaming, profiling, statistics, cancellation, and resource limits.
7. **Exceptional AI reasoning:** secure asynchronous model access, budgets, recorded responses, fallbacks, and reproducible replay fixtures.
8. **Flagship release:** debugger, editor support, packages, publishing, cross-platform validation, and complete behavioural comparison against the C# reference.

## 10. First vertical-slice target

The first future implementation slice should contain only:

- one Garden;
- Adam;
- Yala observing but not required to intervene;
- several plants and animals;
- a deterministic seed;
- a simulated clock;
- observation and naming events;
- separate World and Creator records;
- save, restore, and replay;
- an explanation of every autonomous choice.

Eve, Lilith, reproduction, demigod offspring, expulsion, civilisation, Creator possession, rebellion, language-model calls, databases, and parallel world batches remain outside that first slice.

## 11. Requirement intake rule

When C# development exposes a gap, Project Oracle records it here before future implementation work begins. The requirement must describe the problem and acceptance behaviour first. Proposed syntax is optional and never becomes Project Oracle canon merely because it appears in a note or mock-up.

Derek then decides whether to:

- satisfy the requirement with existing accepted behaviour;
- extend an existing feature;
- add a new language capability;
- add a standard-library or tooling capability;
- support it through temporary .NET interoperability;
- defer it;
- or reject it in favour of a safer or clearer design.

## 12. Current decision

Project Oracle v0.1.1 is the first accepted installable C#/.NET console prototype after the unaccepted v0.1.0 SDK-pin failure. Project Oracle v0.1.2 adds the live-window launcher evidence future implementation will eventually need for separate tool surfaces. Project Oracle v0.1.3 adds direct address channels, seed-based living kinds, Adam's naming mandate, and Natural Course evidence. Project Oracle v0.1.4 adds legacy save compatibility evidence. Project Oracle v0.1.5 adds project-specific authority and no-guessing evidence through Oracle's own Company Bible. The accepted C# reference already provides evidence for structured records, stable identities, modules, routines, collections, deterministic probability, exact real-time scaling, calendar and celestial state, atomic files, save validation, forward-only catch-up, protected World and Creator records, and behavioural tests.

These C# capabilities expose or exercise OR-FI-001 through OR-FI-006, OR-FI-010, OR-FI-012, OR-FI-014, OR-FI-020, OR-FI-022, OR-FI-026, OR-FI-030, OR-FI-031, OR-FI-035, OR-FI-036, OR-FI-040, OR-FI-041, OR-FI-042, OR-FI-045, OR-FI-046, OR-FI-050, OR-FI-052, OR-FI-053, OR-FI-080, OR-FI-082, and OR-FI-083. They remain **Recorded**, not Accepted or Ported, until Derek assesses an exact accepted tag and owns any resulting language work.

future implementation remains the long-term permanent language for Project Oracle. This document records the destination and evidence; it does not authorise future implementation syntax or a compiler build.
