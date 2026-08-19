# Project Oracle Validation v0.0.24

The v0.0.24 candidate must pass the entire accepted v0.0.23 regression suite plus Brain Slice 7, export, adaptive-desktop, and branding gates. The current automated inventory is **180 acceptance tests**.

## Required identity

- version `0.0.24`;
- `Yala Soar Brain Slice 7`;
- schema 6;
- default `save_v6.json`;
- Soar memory directory `yala_soar_v0_0_24`;
- root Linux executable `Project_Oracle_v0_0_24`.

## New cognition proofs

- fresh planning/investigation/counterfactual/trace state starts empty;
- prison contact creates a durable investigation and four-step plan;
- extraordinary god/help claims create inspectable counterfactuals;
- speaker answers enter investigation evidence without becoming proof;
- decision trace contains before/after cognitive snapshots;
- planning state and trace survive save/restore;
- Soar receives active plan/investigation signals and supports bounded `deliberate`;
- accepted v0.0.23 schema-5 saves are preserved and rejected from migration.

## Export proofs

- JSON export is real executable Core behavior, not only GUI labels;
- JSON contains the full ledger-backed conversation timeline, world state, current cognition, and cognitive flight recorder;
- readable TXT export contains the same full `You:` / `Yala:` timeline, including autonomous Yala questions rather than only the bounded recent dialogue-memory window.

## Desktop proofs

- auto-follow and intentional-scroll protection exist;
- active screen working area, render scaling, and aspect ratio are used;
- window placement storage/restoration and screen clamping exist;
- `FIT TO SCREEN` and `JUMP TO LATEST` are present;
- Oracle branding assets are packaged;
- launcher uses `Icon=project-oracle` and `Terminal=false`;
- installer copies icon and applies executable custom-icon metadata where supported.

## Full native gate

Validation uses single-node/disabled-parallel .NET restore/build/publish on the target Linux machine to avoid the known environment's multiprocess CLR/MSBuild failure. It publishes a self-contained Linux x64 apphost to the project root, verifies ELF/version identity, runs all acceptance tests, validates Soar native dependencies and memory smoke, validates docs/manifest/formatting, and leaves manual Derek inspection as the final acceptance gate.
