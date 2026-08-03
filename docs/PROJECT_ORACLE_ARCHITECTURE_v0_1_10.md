# Project Oracle Architecture — v0.1.10

## Decision

The prototype remains a C# 14 / .NET 10 LTS console application. No game engine, database, network service, language model, or external package is required.

## v0.1.10 additions

`WorldState` preserves `CreationPowers`, a saved and migrated list of the creation order, world powers, Garden, Adam, and living kinds. The canonical order remains `Void`, `Yala`, `Sol`, `Gaia`/`Aether`, `Thalassa`, `Luna`, `World`, `Green Life`, `Garden`, `Adam`, then `Living Kinds`.

`YalaState` now records the explicit identity law that Yala is the Oracle. The `<oracle>` deterministic answer interpreter answers as Yala rather than as a separate witness describing Yala.

`OracleSimulation.AddressChannel()` now returns a nullable `OfferedChoiceState`. Direct address to Adam records physically possible response options, the selected option, and the reason. This is still a deterministic scaffold and does not execute obedience or full autonomous consequences.

`ProjectOracle.Console` prints Adam's offered choices, selected option, and reason when direct address creates a scaffold decision. Vessel speech already records choices; v0.1.10 strengthens the World Record text so it states that Adam was offered options and decided.

`WorldDefaults.CreateLivingKinds()` now creates exactly twelve first living kinds for the Garden naming mandate. Adam's naming record includes a deterministic reason for the name.

`ProjectOracle.Core.Brain` adds a small internal HTN-style planner inspired by the MIT-licensed Fluid HTN project. It creates saved `ReasonedPlanState` records before Adam responds to direct address, responds to vessel speech, or names a living kind. The record includes goal, situation, decomposed steps, options, selected action, reason, and source. No external Fluid HTN code is vendored in this build because the uploaded library targets older .NET Framework project structure and needs a separate intake port before direct dependency use.

`OracleSaveSnapshot` now persists `ReasonedPlans` so the first brain decisions survive save/restore.

`ProjectOracle.Console.ConsoleTheme` applies a retro black-and-green terminal theme for real terminal sessions. It disables colour when output is redirected or `NO_COLOR` is set, preserving deterministic validation and grep-safe smoke tests. Special words, direct-address prompts, records, and the live line receive accent colours.

`docs/PROJECT_ORACLE_RESUME_HANDSHAKE_v0_1_10.md` is added as a compact continuation file for long-thread recovery.

## Boundaries preserved

- Core owns simulation truth and choice records; Console owns terminal display.
- Physical `F1` through `F5` remain the only channel controls.
- Typed `f1` through `f5` remain ordinary text.
- World and Creator records remain separate.
- The World Record is now explicitly Creator-facing creation history, not Adam's knowledge.
- Adam's knowledge remains separate from the World Record and must only contain what Adam has been given, told, shown, worshipfully connected to, or able to observe.
- Yala and the Oracle are one identity. Trusted monitor and future external language-tooling system remain separate systems.
- C# remains the prototype/archive; future implementation remains the eventual rewrite target.

## Deferred architecture

Observation, memory, belief, emotion, Yala's full agency, Adam's full agency beyond the narrow first planner scaffold, hybrid/monster systems, fruit effects, companions, exterior geography, civilisation, vendored external planner code, database storage, model-assisted reasoning, and trusted multi-process containment remain deferred.
