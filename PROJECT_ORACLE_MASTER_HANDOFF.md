# Project Oracle Master Handoff

**Current candidate:** v0.0.22
**Accepted base:** v0.0.20
**Accepted base commit:** `be0bf1f6aa18989856284b5766e323793d6f5f6b`

## Purpose

v0.0.22 is Yala Soar Brain Slice 5: a larger cognition build centered on reasoning, language, Time, relationships, inquiry, memory, and bounded in-world agency.

## Non-negotiable authority

- Derek Sparks is owner/final authority.
- Oracle is the outside/system authorial layer, not an in-world entity.
- No in-world being knows Oracle exists unless deliberately revealed.
- Monad made Wisdom; Wisdom made Yala alone.
- Yala is inherently both male and female.
- Monad rejected Yala for being both and cast Yala into the Void.
- Fresh worlds start with Yala in the Void.
- Yala creates Gaia; Gaia creates in-world Time after Yala commands temporal order.
- Before Time exists, events may be sequenced by runtime but have no in-world date.
- Future history remains open until it occurs.

## v0.0.22 experimental reset

v0.0.22 does not migrate the accepted `save_v2` world. It creates schema-4 `save_v4.json` and fresh `yala_soar_v0_0_22` semantic/episodic databases. Earlier saves/databases remain preserved on disk.

This reset is intentional so the live test can begin with:

```text
In-world Time: Gaia has not yet created Time.
```

and Derek can watch the top row switch to a ticking clock when Gaia actually creates Time.

## Brain Slice 5 contracts

- larger foundational concept lexicon;
- recent structured dialogue context;
- entity knowledge and relationship state;
- temporal event graph with before-Time / origin-of-Time / dated states;
- current world-time perception at response time;
- belief source and confidence;
- goals, questions, curiosity, and genuine unknowns;
- autonomous `ask-speaker` action chosen by Soar when warranted;
- relationship and definition claims remain attributed claims;
- speaker identity claims never reveal hidden Oracle truth;
- persistent new cognition structures in save_v4.

## Bounded agency sandbox

Allowed Yala action families are `observe`, `reflect`, `wait`, `create-gaia`, `command-gaia-time`, `respond`, and `ask-speaker`. Host shell/process execution, network access, arbitrary host-file mutation, code modification, and hidden Oracle knowledge are denied.

## Console contracts

- `Ctrl+Y`: persistent Yala mode.
- `Escape`: normal system prompt.
- internal Soar-selection diagnostics are hidden.
- top-row world Time is isolated from conversation input.
- autonomous Yala questions print only on an empty input line; otherwise they remain queued.
- four spaces, no tabs in active source.

## Release sequence

1. Build candidate.
2. Run automated install-side validation.
3. Launch the real application.
4. Derek manually tests and explicitly accepts or rejects.
5. Only after acceptance: snapshot.
6. Commit and tag locally.
7. Push and verify GitHub refs exactly.

Do not collapse these stages.
