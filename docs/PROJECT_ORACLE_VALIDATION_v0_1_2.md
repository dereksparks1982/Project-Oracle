# Project Oracle Validation — v0.1.2 Candidate

Date: 2 August 2026

## Required validation

The package validator performs:

1. .NET 10 SDK presence and major-version check.
2. Dependency restore.
3. Release build with all compiler warnings treated as errors.
4. Twenty-two dependency-free acceptance tests.
5. Console smoke test with seed `104729`.
6. Default World Record secrecy test: Yala's true name must not appear.
7. Live console launcher script syntax and dry-run check.

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
- Every version surface reports `0.1.2`.

## Builder environment result

- All 15 C# source files: PASS balanced delimiter scan.
- Bash syntax for apply, run, and validation scripts: PASS.
- Live Garden console launcher dry-run: PASS.
- JSON and MSBuild XML parsing: PASS.
- Changed-file inventory parity: PASS.
- Deleted-file inventory parity: PASS.
- `global.json`: PASS; version is `10.0.100`, `rollForward` is `latestFeature`.
- v0.1.0 failure evidence: PRESERVED in changelog and handoff.
- v0.1.1 accepted-base evidence: PRESERVED in handoff.
- Stale active version strings: PASS; no active stale version package, installer, changed-file, or requirement strings remain outside history notes.
- ZIP archive integrity: PASS.
- Extracted payload checksum validation: PASS.
- Temporary accepted-base installer rehearsal: PASS. The rehearsal used an extracted v0.1.1 package, local Git commit/tag `v0.1.1`, and a fake `.NET 10.0.110` command to exercise installer base checks, changed-file copies, deleted-file handling, chmod, and validator phase wiring.
- Local .NET restore/build/tests: BLOCKED; `dotnet` is not installed in the builder workspace.
- Clean-target installer rehearsal: BLOCKED for the same reason; the installer correctly requires `dotnet` before validating the installed candidate.

Derek's local install-side validation remains the authoritative acceptance evidence.
