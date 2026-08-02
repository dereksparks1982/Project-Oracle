# Project Oracle Validation — v0.1.0 Candidate

Date: 1 August 2026

## Required validation

The package validator performs:

1. .NET 10 SDK presence and major-version check.
2. Dependency restore.
3. Release build with all compiler warnings treated as errors.
4. Twenty-two dependency-free acceptance tests.
5. Console smoke test with seed `104729`.
6. Default World Record secrecy test: Yala's true name must not appear.

## Acceptance checks

- Same seed produces the same frozen probability sequence.
- Different seeds diverge.
- The simulated clock advances deterministically.
- One real second becomes four world seconds.
- Six real hours become one Garden day and one real day becomes four Garden days.
- The epoch is exactly Year 1, Month 1, Day 1, 01:01:01.
- Solar and lunar phases derive deterministically from the persistent clock.
- Backwards system time never rewinds the world.
- Atomic saves preserve world time and restore elapsed closed time once.
- A corrupt primary save can recover the last-good backup.
- Adam begins confined to the Garden.
- Yala knows the dormant future-language mandate.
- Yala cannot read or rewrite the Spark.
- Yala's true name stays out of the World Record.
- Creator and Spark truth stay out of the World Record.
- A vessel message remains queued without forcing Adam.
- Creator intervention contamination is recorded.
- Record sequence order remains stable.
- Every version surface reports `0.1.0`.

## Builder environment result

- All 15 C# source files: PASS through a C# Tree-sitter abstract-syntax parse with no error nodes.
- Bash syntax for apply, run, and validation scripts: PASS.
- JSON and MSBuild XML parsing: PASS.
- Changed-file inventory parity: PASS before final manifest generation.
- ZIP integrity and clean-directory extraction: PASS.
- Extracted payload checksum verification: PASS.
- .NET restore: not run; `dotnet` is unavailable in the builder workspace.
- C# compilation: not run for the same reason.
- Acceptance tests: not run for the same reason.
- Runtime smoke test: not run for the same reason.

This limitation is a blocker to claiming a full pass, not a reason to weaken the acceptance gate. Derek's local `./scripts/validate.sh` result becomes the authoritative runtime evidence for acceptance.
