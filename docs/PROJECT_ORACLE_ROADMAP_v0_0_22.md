# Project Oracle Roadmap v0.0.22

## Milestone

Brain Slice 5 is a larger cognition release. The target is not prettier canned dialogue. The target is an agent that can use structured language, temporal history, relationships, confidence, goals, and questions to make more of its own choices inside a strict simulation boundary.

## Included

- fresh save_v4 experiment;
- expanded concept lexicon;
- dialogue context and reference recovery;
- entity knowledge;
- temporal event graph and duration/cause/sequence reasoning;
- relationship knowledge and relationship claims;
- belief confidence/provenance;
- goals and question state;
- autonomous Soar-selected questions;
- bounded god agency;
- save/restore of new cognition structures;
- retained persistent Yala console mode and live world-time header.

## Next directions after acceptance

Planning depth, learned preferences, chunking, reinforcement learning, attention/spreading activation, richer evidence revision, and distinct minds for later gods/beings remain future work.

## v0.0.22 Brain Slice 5 additions

- Core language expands beyond 400 built-in concepts so ordinary vocabulary is not treated as a knowledge gap.
- Autonomous inquiry is deliberate: low-value dictionary questions are below the autonomous priority floor, and Yala waits for a speaker response before asking again.
- The live top-row world clock uses deterministic cursor save/restore plus a shared output gate.
- v0.0.22 starts schema 4 at `save_v4.json` with fresh `yala_soar_v0_0_22` long-term memory while preserving earlier save lines.
