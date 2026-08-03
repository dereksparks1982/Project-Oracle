# Project Oracle Validation — v0.1.6 Candidate

Date: 2 August 2026

## Required validation

The package validator performs:

1. .NET 10 SDK presence and major-version check.
2. Dependency restore.
3. Release build with all compiler warnings treated as errors.
4. Thirty-one dependency-free acceptance tests.
5. Project Oracle Company Bible presence and content check.
6. Physical function-key source check proving the console uses `ReadKey` and does not retain typed `f1` switch cases.
7. Console smoke test with seed `104729`.
8. Default World Record secrecy test: Yala's true name must not appear.
9. Live console launcher script syntax and dry-run check.

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
- A version `0.1.1` save upgrades through current world defaults.
- A version `0.1.4` save upgrades through current world defaults.
- A corrupt primary save can recover the last-good backup.
- Adam begins confined to the Garden.
- Yala knows the dormant future-language mandate.
- Yala cannot read or rewrite the Spark.
- Yala's true name stays out of the World Record.
- Creator and Spark truth stay out of the World Record.
- The World Seed creates deterministic ancient living kinds.
- Direct address channels are appointed as F1 Oracle, F2 Gaia, F3 Adam, F4 Sun, and F5 Moon.
- Physical function keys select the appointed address channels, while digit keys and F6 do not.
- Adam begins with the naming mandate active.
- The Natural Course rule is active.
- Presenting a living kind lets Adam name it without finding a suitable mate.
- Direct address to Adam is recorded without puppeteering his response.
- A vessel message remains queued without forcing Adam.
- Creator intervention contamination is recorded.
- Record sequence order remains stable.
- Every version surface reports `0.1.6`.
- The validator checks that the Project Oracle Company Bible exists and preserves the no-guessing and physical-function-key rules.

## Builder environment result

- Bash syntax for run and validation scripts: PASS.
- Company Bible authority preserved: PASS.
- `global.json`: PASS; version is `10.0.100`, `rollForward` is `latestFeature`.
- v0.1.0 failure evidence: PRESERVED in changelog and handoff.
- v0.1.1 accepted-base evidence: PRESERVED in handoff.
- Local .NET restore/build/tests: BLOCKED; `dotnet` is not installed in the builder workspace.
- Console smoke test: BLOCKED for the same reason.
- Live Garden console launcher dry-run: BLOCKED for the same reason because the validator invokes the project script chain.

Derek's local install-side validation remains the authoritative acceptance evidence.
