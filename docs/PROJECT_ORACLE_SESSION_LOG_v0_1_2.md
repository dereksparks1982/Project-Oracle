# Project Oracle Session Log — v0.1.2 Candidate

## 2 August 2026 — v0.1.2 live console window

- Read the Project Oracle origin document, cumulative handoff, canon, roadmap, Demon Killer world-time intake, validation record, and canonical DK LAB Company Bible before repair work.
- Recorded Derek's accepted `v0.1.1` base: commit `ce58375`, tag `v0.1.1`, .NET SDK `10.0.110`, twenty-two acceptance checks passed.
- Added a separate live Garden console window launcher so Oracle input and output do not share Derek's working terminal.
- Added an internal live-console runner that sets the window title, protects against two concurrent Garden windows, holds the window open after exit or failure, and reports missing .NET clearly.
- Updated `run.sh` and the C# console loop so non-interactive input closure is reported plainly instead of silently returning to Bash after `oracle>`.
- Advanced Project Oracle package/version surfaces to v0.1.2 as the next unused numeric version.

Date: 1 August 2026

## Authority received

Derek authorised the first build and asked Codex to choose its scope, then create a long-range roadmap comparable in depth to the future implementation roadmap.

During the build Derek supplied Demon Killer's `scripts.zip` and established these additional laws:

- the world runs in real time at four world days per real day;
- one world day lasts six real hours;
- the world continues after closure through saved-state catch-up;
- lunar phases matter;
- solar state may matter because Adam may form his own beliefs or worship;
- the creation epoch is Year 1, Month 1, Day 1, 01:01:01.

## Work completed

- Read the accepted Project Oracle Concept Build 0 archive and living future implementation requirements register.
- Applied the already-read canonical external technical authority v0.1.39 rules to the build workflow.
- Reviewed the supplied 240-path Demon Killer script archive for server clock, save clock, offline catch-up, continue, backup, and deadline behaviour.
- Selected C# 14 and .NET 10 LTS for the prototype reference.
- Built the Core, Console, and AcceptanceTests projects.
- Added the persistent calendar, solar/lunar state, atomic save, backup recovery, Creator/World record separation, and intervention queue.
- Added canon, architecture, Demon Killer intake, validation, changelog, cumulative handoff, and long-range roadmap records.
- Updated the Oracle future-implementation requirements register with v0.1.2 reference evidence without changing future implementation syntax.

## Validation limitation

The builder workspace does not contain `dotnet`, a C# compiler, or MSBuild. The installed package therefore requires Derek's `.NET 10` validation to establish compile and runtime truth. No pass is claimed for phases that could not run.

A temporary C# Tree-sitter parser supplied through ast-grep successfully parsed all 15 C# source files with no error nodes. It was used only for builder validation, was not added to Project Oracle, and created no project dependency. This syntax pass does not replace .NET compilation.

## Next recommended scope

v0.1.3 should add the deterministic event queue and minimal offered-choice engine described in the cumulative handoff. It should not yet add memory, belief, Eve, Lilith, reproduction, expulsion, or civilisation.
