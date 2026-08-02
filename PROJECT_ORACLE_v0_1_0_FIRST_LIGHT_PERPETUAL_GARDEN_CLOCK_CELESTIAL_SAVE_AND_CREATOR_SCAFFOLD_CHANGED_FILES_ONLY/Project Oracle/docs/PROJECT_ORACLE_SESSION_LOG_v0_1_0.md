# Project Oracle Session Log — v0.1.0 Candidate

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
- Updated the Oracle future-implementation requirements register with v0.1.0 reference evidence without changing future implementation syntax.

## Validation limitation

The builder workspace does not contain `dotnet`, a C# compiler, or MSBuild. The installed package therefore requires Derek's `.NET 10` validation to establish compile and runtime truth. No pass is claimed for phases that could not run.

A temporary C# Tree-sitter parser supplied through ast-grep successfully parsed all 15 C# source files with no error nodes. It was used only for builder validation, was not added to Project Oracle, and created no project dependency. This syntax pass does not replace .NET compilation.

## Next recommended scope

v0.1.1 should add the deterministic event queue and minimal offered-choice engine described in the cumulative handoff. It should not yet add memory, belief, Eve, Lilith, reproduction, expulsion, or civilisation.
