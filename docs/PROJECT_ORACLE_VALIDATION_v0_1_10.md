# Project Oracle Validation — v0.1.10 Candidate

Date: 3 August 2026

## Required validation

The package validator performs:

1. .NET 10 SDK presence and major-version check.
2. Dependency restore.
3. Release build with all compiler warnings treated as errors.
4. Forty-eight dependency-free acceptance checks.
5. Project Oracle Company Bible presence and content check.
6. Physical function-key source check.
7. Deterministic event-queue source check.
8. First-knowing source check.
9. Creation-powers source check.
10. Oracle/Yala same-identity source and acceptance checks.
11. Adam decision-output source and acceptance checks.
12. First brain planner source and acceptance checks.
13. Retro console theme source checks.
14. Console smoke test with seed `104729`.
15. Creator-facing World Record smoke test.
16. Live console launcher syntax and dry-run checks.

## New v0.1.10 acceptance coverage

- Yala and the Oracle are the same identity in state, direct address, and deterministic Oracle answers.
- `<oracle>` answers about Yala in first person instead of describing Yala as a separate third party.
- New worlds begin with exactly twelve living kinds and the naming mandate counts those twelve.
- Direct address to Adam records offered choices, selected option, and reason without puppeteering him.
- Vessel-speech choice records say Adam was offered options and decided.
- Adam naming records include a deterministic reason for the name.
- Adam naming creates a saved reasoned brain plan before the name record.
- Direct address to Adam creates a saved reasoned brain plan before the offered choice.
- Vessel speech creates a reasoned brain plan before Adam's selected response.
- Reasoned brain plans survive save and restore.
- The console exposes `plans` / `brain`.
- Real terminal output uses a retro black-and-green theme with highlighted special words, while redirected output and `NO_COLOR` remain plain.
- The resume handshake document exists and is versioned as `v0.1.10`.

- Yala may overclaim authority, but protected Creator records outrank her claim.
- The creation order records Void, Yala, Sol, Gaia, Aether, Thalassa, Luna, World, Green Life, Garden, Adam, and Living Kinds.
- The World Record begins with void/Yala creation truth before Adam appears.
- The World Record is Creator-facing, while Adam's answer set still does not know the Creators or Yala.
- A `0.1.7` save upgrades through current creation-power defaults.
- A rejected `0.1.8` save upgrades through the corrected creation-power defaults and canonical address channels.
- Oracle answers creation-order and water-rule questions.
- Oracle preserves Adam's first-knowing rule that he knows that he is, not that he is alive.
- The console smoke test rejects the old static startup `World time:` line so the live clock is the normal moving clock.

## Builder environment result

- Builder-side `./scripts/validate.sh` was run in the workspace and stopped at the required .NET check.
- Result: blocked, not failed.
- Evidence:

```text
VALIDATION BLOCKED: Project Oracle v0.1.10 needs the .NET 10 SDK.
The dotnet command was not found. No build or tests were run.
```

- Shell syntax checks for `scripts/validate.sh`, `scripts/run-window.sh`, and `scripts/run-live-console.sh` passed in the builder workspace.
- Derek's install-side validation remains the authoritative acceptance evidence.

## Install-side command

```bash
cd "$HOME/DKLab/Projects/Project Oracle"
./scripts/validate.sh
```
