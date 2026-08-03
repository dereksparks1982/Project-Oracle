# Project Oracle Changelog

## v0.1.2 — Separate Live Garden Console Window — Candidate — 2 August 2026

### Added

- Added `scripts/run-window.sh` to open Project Oracle in a separate live Garden console window.
- Added `scripts/run-live-console.sh` for the terminal-window child process, window-title setup, single-instance guard, and exit hold.
- The separate console window becomes the live Project Oracle display for startup, records, command output, warnings, errors, and exit status.
- The working terminal returns immediately after launching the Garden window.
- Added terminal detection for `gnome-terminal`, `ptyxis`, `kgx`, and `x-terminal-emulator`.
- Added a dry-run launcher validation path used by `scripts/validate.sh`.

### Repaired

- `scripts/run.sh` now reports non-interactive input clearly instead of letting the Oracle prompt appear and disappear silently.
- The console prints an explicit message if standard input closes while the live prompt is waiting.
- Version surfaces, README, validator, tests, changed/deleted-file records, validation record, and handoff now report `0.1.2`.

### Preserved

- Project Oracle v0.1.1 remains the accepted base.
- The v0.1.0 SDK-pin failure evidence remains preserved in the handoff history.

### Not added

- Autonomous decisions, memory, belief, AI/model calls, Eve, Lilith, reproduction, fruit effects, expulsion, exterior world, civilisation, saves, database, networking, or graphics.

### Baseline

Accepted Project Oracle v0.1.1 commit/tag.

### Acceptance

Not accepted until Derek installs, validates, and explicitly accepts the build.
