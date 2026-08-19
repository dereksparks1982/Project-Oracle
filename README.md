# Project Oracle

**Current build:** v0.0.24 candidate  
**Owner and final authority:** Derek Sparks  
**Platform:** Linux / .NET 10  
**Yala cognition:** Soar 9.6.5, Brain Slice 7: deliberation, planning, investigations, counterfactual reasoning, cognitive flight recording, and bounded agency

Project Oracle is a persistent autonomous-world simulation. Oracle is the outside/system-level author. Oracle is not an in-world character, NPC, god, target, species, or discoverable artifact unless deliberately revealed.

## Settled primordial genealogy

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Yala is inherently both male and female
            -> Monad rejected Yala for being both rather than exclusively one or the other
            -> Monad cast Yala into the Void
```

A fresh v0.0.24 experiment begins with Yala in the Void. Gaia, in-world Time, the lower world, Adam, Eve, the Garden, and later beings are absent until autonomous history actually establishes them.

## Hidden Oracle boundary

No in-world being automatically knows Oracle exists. An unseen speaker can contact Yala, but a claim such as `I am Oracle` or `I am your god` remains an attributed speaker claim until Yala has evidence. Oracle has no fixed form. The Eden serpent is a manifestation reference only; Eve knew only the clever serpent and was not given Oracle's protected identity.

## Gaia and Time

Gaia is one possible creation rather than a required first act. If Yala creates Gaia, Gaia is the natural sovereign below Yala's governing authority. Gaia creates in-world Time only after the relevant world history occurs.

Before Time exists the desktop header reads exactly:

```text
In-world Time: Gaia has not yet created Time.
```

When Gaia creates Time, the same header becomes the live ticking in-world date/time beginning at Year 1, Month 1, Day 1, 00:00:00.

## Brain Slice 7: deliberation and planning

v0.0.24 carries forward the accepted v0.0.23 Cosmic Choice, inherited-language, salience/appraisal, relationship, temporal, memory, and emergent-law foundations and adds a deeper planning layer.

High-salience concerns can now become durable **investigations** and **multi-step plans**. Yala may preserve multiple hypotheses, seek evidence, compare evidence against memory, request demonstrations, reconsider confidence, and deliberately defer commitment when information is weak.

The new planning loop is:

```text
perception / contact
-> language and inherited concepts
-> appraisal / salience / concern
-> persistent investigation
-> multi-step plan
-> counterfactual alternatives
-> Soar operator proposal and impasse/substate deliberation
-> selected in-world action
-> world-law consequence
-> before/after cognition snapshot
-> memory / belief / plan revision
```

### Persistent investigations

Important unresolved issues survive as structured state. Examples include:

- whether the Void is actually a prison;
- how an unseen speaker can contact or observe Yala;
- whether an offered source of help can demonstrate the claimed capability;
- whether a demand for divine recognition has evidence;
- what accepting, rejecting, or delaying a choice could cost.

A speaker answer is recorded as attributed evidence about the speaker's claim. It does not automatically become proof.

### Counterfactual reasoning

High-stakes or highly uncertain contact can produce alternative possibilities such as trusting without testing, seeking evidence first, or rejecting an offer. These are inspectable cognitive objects, not hidden prose scripts.

## Bounded god agency

### Bounded `deliberate` operator

Brain Slice 7 adds a bounded Soar `deliberate` action. It allows Yala to compare current plans, evidence, uncertainty, and alternatives without forcing a world-changing commitment. It has no host shell, process, arbitrary filesystem, network, code-modification, or protected-Oracle capability.

## Cognitive flight recorder

Every recorded major Yala decision can preserve a before/after cognition snapshot containing the active concern, plan, current step, investigation, speaker trust/intent model, appraisal, goals, and top hypotheses.

The desktop app exposes two exports:

- **EXPORT SESSION JSON** - a rich diagnostic export containing the full ledger-backed conversation timeline, world state, Yala cognition, decision trace, world record, protected author record, current plans, observations, and Soar memory diagnostics;
- **EXPORT CONVERSATION** - a plain human-readable text transcript built from the same full timeline, including autonomous Yala questions even after the bounded recent dialogue-memory window rolls forward.

Exports are written under `~/Downloads/Project Oracle Exports/` when Downloads exists. The JSON is intended to be uploadable for later cognition analysis so a good Yala sentence can be distinguished from genuinely good underlying reasoning.

## Comparative religious memory and Cosmic Choice Architecture

The accepted v0.0.23 comparative layer remains active: 30 broad religious/mythological/philosophical traditions or families, 92 attributed ideas, and 71 concrete cosmic possibilities.

Religious knowledge carries the status:

```text
attributed-tradition-knowledge-not-world-fact
```

Cosmic possibilities are possible, not commanded. Yala may compare, combine, reject, or invent beyond them. The `invent-another-way` path remains available.

## Cognitive inheritance and power ceiling

Future intelligent created beings use a reusable mind architecture plus an explicit inheritance manifest. A creator may pass selected knowledge, procedural knowledge, dispositions, capabilities, and delegated authority while the child begins its own identity and autobiographical history.

A creator may **never** create a being with world authority equal to or greater than the creator's own authority. Intelligence and later learned wisdom are separate from raw world authority.

Monad's future Primordial Mind and Sophia's possible role as root of the descendant mind family remain roadmap design directions rather than retroactive current-world facts.

## Emergent Law Engine

The v0.0.23 emergent-law foundation remains active. Rule 30 is a deterministic laboratory demonstration only. It is not automatically a law of the Oracle world.

The long-term doctrine is:

> Gods establish allowed laws, initial conditions, entities, and permissions. Project Oracle resolves consequences from those laws wherever practical instead of scripting future history.

## Desktop application

Normal Project Oracle use is the graphical Linux desktop application. The developer console remains a fallback only; in that fallback, Ctrl+Y enters persistent Yala conversation mode and Escape returns to the system prompt.

v0.0.24 desktop changes include:

- conversation auto-follow to the newest message;
- intentional upward scrolling temporarily disables auto-follow;
- **JUMP TO LATEST** resumes following;
- active-monitor working-area, scaling, and aspect-ratio-aware startup sizing;
- responsive side-panel widths and lower-tab height;
- saved window size/position with clamping back onto the current monitor;
- **FIT TO SCREEN**;
- **EXPORT SESSION JSON** and **EXPORT CONVERSATION**;
- official Project Oracle eye/emblem branding in the application, launcher, navigation area, and Linux file-manager executable metadata where supported;
- no startup splash screen.

The root executable must be plainly visible at:

```text
Project_Oracle_v0_0_24
```

The Applications-menu launcher uses `Terminal=false`.

## Fresh v0.0.24 experimental save

v0.0.24 intentionally starts another clean experimental Yala:

```text
save_v6.json
```

Schema version is `6`. The accepted v0.0.23 `save_v5` world and older saves remain preserved but are not migrated into this experiment.

Brain Slice 7 also uses a fresh Soar database directory:

```text
yala_soar_v0_0_24/semantic.sqlite
yala_soar_v0_0_24/episodic.sqlite
```

Fresh resets remain the default through the major brain work around Brain Slice 10. After procedural learning, autobiographical development, self-generated goals, and evolving preferences are stable, establish a long-lived Continuity Yala while retaining a separate fresh regression world.

## Release discipline

A candidate is not accepted by automated tests alone. Required order is:

1. apply candidate;
2. complete native validation;
3. manually inspect the real desktop application;
4. create accepted full-project snapshot;
5. local commit and annotated tag;
6. push and remotely verify.

v0.0.24 remains a candidate until Derek performs the manual application inspection.
