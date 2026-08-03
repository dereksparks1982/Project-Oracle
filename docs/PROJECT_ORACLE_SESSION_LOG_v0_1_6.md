# Project Oracle Session Log — v0.1.6 Candidate

## 2 August 2026 — Physical function keys and save migration chain repair

- Derek reported that the accepted `v0.1.5` console still rejected saves written as version `0.1.4`.
- Derek also corrected the direct-address control again: `F1`, `F2`, `F3`, `F4`, and `F5` mean the physical function keys, not typed character commands followed by Enter.
- Read the required Project Oracle origin DOCX, cumulative handoff, Project Oracle Company Bible, canon, roadmap, world-time intake, architecture, validation record, and future implementation requirements register before this build.
- Implemented interactive physical function-key capture with `Console.ReadKey`.
- Removed typed `f1`, `f2`, `f3`, `f4`, and `f5` as channel-switch shortcuts.
- Added explicit `0.1.4` and `0.1.5` save-version support.
- Updated README, handoff, canon, roadmap, architecture, validation, changelog, Company Bible, scripts, and version surfaces to `v0.1.6`.
- Added acceptance checks for `0.1.4` save upgrade and physical function-key mapping, raising the acceptance count to thirty-one.

## Validation limitation

The builder workspace does not contain `dotnet`, a C# compiler, or MSBuild. The installed package therefore requires Derek's `.NET 10` validation to establish compile and runtime truth. No pass is claimed for phases that could not run.

No local compile, C# parse, restore, or runtime pass is claimed from the builder workspace.

## Next recommended scope

v0.1.7 should be proposed only after Derek manually confirms `v0.1.6`. The next likely feature scope is the deterministic event queue, but it should not be built without a fresh Company Bible read and explicit approval.
