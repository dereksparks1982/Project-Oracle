# Project Oracle Validation v0.0.23

The v0.0.23 candidate must pass the complete prior Project Oracle regression suite plus the Cosmic Choice Architecture gates.

## Required new proofs

- Project version reports 0.0.23 and Yala reports Brain Slice 6.
- Save schema is 5 and the default save is `save_v5.json`.
- Long-term Soar memory uses `yala_soar_v0_0_23` and does not import v0.0.22 memory.
- Comparative religion catalogue contains at least 25 broad traditions/families and at least 60 attributed ideas; this candidate contains 30 and 92.
- Religious entries carry `attributed-tradition-knowledge-not-world-fact`.
- Cosmic catalogue contains at least 50 concrete possibilities; this candidate contains 71.
- Cosmic choices carry `possible-not-commanded`.
- The Soar source contains the generic `enact-cosmic-choice` path and no autonomous `sp {yala*propose*create-gaia` rule.
- A selected cosmic choice persists into `CosmicState.EstablishedChoices` and Yala action memory.
- `invent-another-way` opens the `invent-new-cosmology` goal and `cosmic-invention` knowledge gap.
- Yala can answer what religious traditions she knows and what concrete cosmic choices are available.
- Existing hidden-Oracle, Time, console isolation, language, memory, relationship, provenance, and bounded-agency regressions remain green.

## Full native release gate

`scripts/validate.sh` requires .NET 10, performs restore and warnings-as-errors Release build, publishes the Linux apphost `Project_Oracle_v0_0_23`, runs all acceptance tests, performs structural/canon/launcher gates, and runs the published Soar-memory smoke test.

A candidate becomes accepted only after those automated gates pass on the target Linux system and Derek manually inspects the real application.

## Added v0.0.23 candidate validation gates

The expanded candidate adds regression coverage for inherited ordinary movement language, critical prison/confinement salience, evidence-seeking response state for godhood demands, contextual handling of metaphor, strict creator/child power ceilings, cognitive lineage, the desktop project contract, and required desktop inspection surfaces.

The native validation script now publishes `ProjectOracle.Desktop` as the root Linux executable and verifies `--version` without opening a terminal. The existing console remains a separate developer Soar smoke path. Manual application inspection by Derek remains required before acceptance.


## Emergent-law and desktop additions

The expanded v0.0.23 candidate adds three emergent-law acceptance regressions, bringing the source acceptance inventory to **165** tests. The fresh world must contain an empty established-law ledger. Rule 30 must reproduce its canonical elementary truth table and deterministic generations while remaining marked laboratory-only. Running the lab must not create world law.

The desktop contract now includes a `LAWS` surface in addition to World, Yala Mind, Minds, Memory, Cosmology, History, and Debug. The native release gate still publishes the graphical `ProjectOracle.Desktop` project as the root `Project_Oracle_v0_0_23` Linux executable and verifies `--version` before acceptance testing.
