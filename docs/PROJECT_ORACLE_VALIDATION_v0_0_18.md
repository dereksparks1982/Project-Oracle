# Project Oracle Validation v0.0.18

## Required automated gates
- .NET 10 restore/build with warnings as errors.
- native root Linux publish and ELF check.
- full acceptance suite, all tests passing.
- real Soar 9.6.5 runtime/production load.
- one persistent Yala Soar session across multiple decisions.
- semantic memory store/retrieval proof.
- episodic memory advancement proof and SQLite smoke files.
- impasse/substate deliberation proof.
- protected console-input buffer tests and visible-status typing guard.
- Yala is both male and female; Monad rejection reason is exact.
- hidden-Oracle boundary remains intact.
- Gaia-created Time and pre-Time clock law remain intact.
- v0.0.17 `save_v2` acceptance/normalization and v0.0.16 rejection.
- README/current authorities align with v0.0.18.
- root `Project_Oracle_v0_0_18` and desktop launcher identity pass.
- export manifest hashes pass.

## Acceptance gate
Automated FINAL PASS must be followed by launching the real application for Derek's manual inspection. No snapshot, commit, tag, or push occurs until Derek explicitly says PASS.

## Builder-environment evidence
The packaging environment does not provide the .NET 10 SDK, so C# build/publish/full acceptance remain mandatory install-side gates. The supplied Soar 9.6.5 Linux runtime is directly exercised during packaging to load Brain Slice 2 productions, prove operator output, impasse/substate behavior, semantic-memory commands, and episodic-memory configuration.


## Repair 2 console regression

Validation forbids `LiveConsoleSurface`, `surface.Refresh`, cursor positioning, and LIVE-row painting from `Program.cs`; requires the compatibility shell to be terminal-silent; requires the prompt to own the input body; and requires an acceptance regression proving the interactive source path contains no LIVE surface refresh.

## Repair 3 validation correction

The console-body isolation test now requires `LiveConsoleSurface.MayPaintVisibleStatus(...)` to return `false` for both an empty input buffer and an active input buffer. This matches the Repair 2 law that asynchronous LIVE status never paints in the interactive terminal body.
