# Project Oracle Architecture v0.0.22

## Brain Slice 5 flow

```text
terminal / world / autonomous tick
        |
        v
language interpreter + dialogue context
        |
        +--> entity / relationship / temporal resolution
        |
        v
Yala perception frame
        |
        v
persistent Soar 9.6.5 agent
working memory + semantic memory + episodic memory
        |
        v
operator preferences / impasse / substate deliberation
        |
        v
bounded action: observe | reflect | wait | create-gaia |
                command-gaia-time | respond | ask-speaker
        |
        v
agency policy + Project Oracle world-law resolution
        |
        v
world state + temporal events + beliefs + dialogue + questions + goals
```

## Persistence

v0.0.22 intentionally starts schema 4 at `save_v4.json`. Old save_v2 worlds remain untouched. Native Soar long-term memory starts fresh under `yala_soar_v0_0_22` so prior speaker claims do not contaminate the fresh experiment.

## Temporal architecture

`before-time` events have sequence/causal position but no world date. `origin-of-time` is Gaia's creation of Time. `dated` events can use the live world clock. Current-time answers are generated from current world state, not stale conversational snapshots.

## Agency boundary

Soar never receives arbitrary host capabilities as candidate in-world operators. `YalaAgencyPolicy` rejects any action outside the explicit simulation action set. Network, shell/process execution, host-file mutation, code mutation, and hidden Oracle knowledge are absent from the allowed interface.

## Console isolation

The top world-time row is a dedicated surface. The conversation body remains protected. Persistent Yala mode is console state, not repeated user text. Autonomous questions are dequeued only when the editable input line is empty.

## v0.0.22 Brain Slice 5 additions

- Core language expands beyond 400 built-in concepts so ordinary vocabulary is not treated as a knowledge gap.
- Autonomous inquiry is deliberate: low-value dictionary questions are below the autonomous priority floor, and Yala waits for a speaker response before asking again.
- The live top-row world clock uses deterministic cursor save/restore plus a shared output gate.
- v0.0.22 starts schema 4 at `save_v4.json` with fresh `yala_soar_v0_0_22` long-term memory while preserving earlier save lines.
