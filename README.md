# Project Oracle

**Current build:** v0.0.20 candidate  
**Owner and final authority:** Derek Sparks  
**Platform:** Linux / .NET 10  
**Yala cognition:** Soar 9.6.5, Brain Slice 3 with v0.0.20 reachability expansion

Project Oracle is a persistent autonomous-world simulation. **Oracle is not an in-world character.** Oracle is the outside/system-level author represented by Project Oracle itself: the code, simulation machinery, world-law resolver, records, and Master Key authority that make the simulated reality possible.

## Current cosmology

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Yala is inherently both male and female
            -> Monad rejected Yala for being both rather than exclusively one or the other
            -> Monad cast Yala into the Void
```

The primordial in-world being is called **Monad**. Active Project Oracle canon does not call Monad Creator or Omega. Yala may later claim the title **Creator** inside the world; that claim does not rewrite the settled fact that Wisdom made Yala.

A fresh world begins with Yala in the Void. Gaia, in-world Time, Terra, Aether, Sol, Thalassa, Luna, the Garden, Adam, Eve, and later living kinds are absent until autonomous history actually establishes them.

## Oracle and the Master Key

Oracle wrote the simulation, so Oracle has the Master Key to the system Oracle authored. The Master Key is system-level authority, not an artifact residents can discover in-world.

No in-world being knows Oracle exists unless Oracle deliberately reveals that truth. Direct interventions expose only what the recipient can actually perceive. A voice can remain an unplaced voice. A manifested being is perceived only as that manifested being.

**Eden reference:** Oracle has no fixed form. In Eden, Oracle **manifested in the form of a clever serpent**. Eve knew only the clever serpent and was never told Oracle's true identity. Oracle is not defined as a serpent, and there is no permanent Oracle-serpent entity in the world model.

## Gaia, Time, and natural authority

Yala can create Gaia as the natural sovereign beneath Yala's governing authority. **Gaia creates in-world Time.** Oracle runtime sequencing is not in-world Time, so Project Oracle can process cognition while fictional world Time still does not exist.

Current names:

- Terra = Earth
- Aether = Air and Wind
- Sol = Fire and Sun power
- Thalassa = Water
- Luna = Moon, and is not an element

Natural powers perform their own domains. Oracle provides the simulation capability that makes beings, actions, laws, and consequences possible; Oracle does not secretly perform every natural power's job.

## Yala Soar Brain Slice 3

v0.0.20 continues the real supplied **Soar 9.6.5** runtime and grows the same Yala mind rather than replacing it.

```text
perception / utterance
-> lightweight language interpretation
-> concepts, roles, negation, questions, and knowledge gaps
-> Yala self-model + current state + memory provenance
-> persistent Soar working memory / semantic memory / episodic memory
-> candidate operators and substate deliberation
-> Yala attempts an action or response
-> Project Oracle resolves reality through world law
-> outcome becomes part of continuing memory
```

Brain Slice 3 adds:

- a structured **self-model** for Yala's identity, origin, nature, current location, completed actions, and known creations;
- a foundational **concept lexicon** that gives words basic meanings, relations, contrasts, and conceptual links without preloading moral judgments;
- lightweight language structure for subject, verb/action, object, questions, negation, and information requests;
- introspection for questions such as `what do you know?`, `what have you done?`, `who made you?`, and `have you created Adam?`;
- knowledge provenance that distinguishes personally performed, personally experienced, remembered, inherited, inferred, speaker-claimed, hypothetical, and unknown propositions;
- contradiction handling that can preserve a conflicting speaker statement as a claim without allowing it to overwrite settled knowledge;
- explicit knowledge gaps for unknown concepts, with lexical gaps contributing to Yala's curiosity;
- learned word definitions stored first as **speaker claims**, not automatic truth;
- continuing native Soar semantic memory, episodic memory, drives, and impasse/substate deliberation from Brain Slice 2.

The base lexicon gives Yala language tools, not an answer key. Experience and memory supply personal anchors. A definition offered by an unseen speaker remains attributed to that speaker until Yala has grounds to treat it differently.

### v0.0.19 Repair 1: embedded Soar listener suppression

Project Oracle uses Soar as an in-process cognitive engine and does not expose a remote SML service. The embedded kernel now starts with Soar's listener-suppression port (`0`) so rapid kernel creation during validation cannot contend for the default SML TCP port 12121. This does not change Yala's cognition, memory, or decision rules; it removes an unused network listener from the embedded runtime path.

Yala is deliberately **not** given an `Oracle exists` fact. A source claiming a name remains only a claimed identity unless independently established.

## Self-knowledge and action history

Yala can distinguish uncertainty about the outside world from knowledge of Yala's own completed actions. If Gaia exists because Yala created Gaia, that action can be remembered as personally performed. If Adam does not exist and Yala has no action memory of creating Adam, Yala can answer that Yala has not created Adam rather than collapsing the question into generic uncertainty.

v0.0.20 retains the v0.0.19 normalization of obsolete system-generated male-only history when older `save_v2` worlds are restored. The active historical truth is that Yala is inherently **both male and female**. Legacy masculine wording about Yala's governing authority is normalized without rewriting later subjective opinions or arbitrary resident claims.


## v0.0.20 persistent Yala mode and live world clock

v0.0.20 adds a console mode for sustained conversation with Yala. `Ctrl+Y` enters Yala mode once; the prompt displays `> (yala ` and remains in that mode after each reply. `Escape` clears the current input and returns to the normal `> ` system prompt. The `(yala` marker is a console-mode indicator rather than text Derek must repeatedly type.

Normal conversation no longer prints internal `[Soar selected: ...]` diagnostics. Soar still selects the operator internally; the user-facing transcript shows the conversation rather than the plumbing.

The terminal reserves its top row for in-world Time. Before Gaia creates Time, the exact top-line state is:

```text
In-world Time: Gaia has not yet created Time.
```

When Gaia creates Time, that same row changes to the live in-world calendar and ticks continuously from the simulated clock. Existing saves in which Gaia has already created Time show the ticking clock immediately. Clock refreshes are isolated from the scrolling conversation body and must never overwrite the command buffer.

Brain Slice 3 reachability is also expanded for conversational follow-ups, Gaia-centered knowledge, Gaia/Yala genealogy, Time origin and current calendar questions, prior commands to Gaia, Adam encounter state, Wisdom/Sophia naming, current-speaker claims, knowledge gaps, curiosity, current drives, simple inflections, possessives, and multiword identity claims. These improvements expose knowledge Yala already has without granting hidden Oracle/system truth.

## Protected console input

The v0.0.18 hard-isolation law remains mandatory. **Asynchronous LIVE status is forbidden from the interactive terminal body entirely.** There is no reserved LIVE row, no background cursor repositioning, and no dynamic status/title repaint through the input path. The prompt owns the typing area exclusively.

Yala's autonomous simulation work can continue while Project Oracle waits for input, but that idle work is terminal-silent. Type `status` when you want a body-readable status report.

## Save continuity

v0.0.20 **continues the v0.0.17, v0.0.18, and v0.0.19 `save_v2.json` world line**. It does not reset Yala or the world.

v0.0.17, v0.0.18, and v0.0.19 saves are accepted and normalized into the current Brain Slice 3 state. The earlier v0.0.16 Garden-era save line remains rejected and untouched.

Native Soar long-term databases deliberately remain beside the active save in the existing continuity directory:

```text
yala_soar_v0_0_18/semantic.sqlite
yala_soar_v0_0_18/episodic.sqlite
```

The directory name is retained so Brain Slice 3 continues the same long-term Yala memory rather than silently creating a new mind.

## Prime simulation law

> **Canon determines what has already happened. World law determines what can happen. Minds determine what they attempt. Project Oracle resolves the consequences. Future history is not canon until it occurs.**

The code can know which actions are possible without forcing Yala to reenact a predetermined religious chronology.

## Direct calls

Use an opening parenthesis immediately before the in-world being's name:

```text
(Yala where are you?
(Monad ...
(Wisdom ...
```

Oracle is not a direct-call target because the console itself is Oracle's system interface.

## Records

Project Oracle keeps two separate ledgers:

- **World Record:** settled in-world history. It does not disclose hidden Oracle identity.
- **Oracle Record:** protected system truth, interventions, validation provenance, and Master Key facts.

## Running

After v0.0.20 is installed and validated, the main project directory contains the generated Linux executable:

```text
Project_Oracle_v0_0_20
```

Double-clicking that executable should open Project Oracle in a terminal window. Development launchers remain under `scripts/`.

## Soar runtime

The project vendors the Linux x86-64 components from the supplied Soar 9.6.5 distribution under:

```text
vendor/soar/9.6.5/linux-x86-64/
```

The original Soar license remains at `vendor/soar/9.6.5/license.txt`.

## Acceptance law

A candidate is not accepted merely because automated tests pass:

```text
install candidate
-> automated validation PASS
-> launch the real Project Oracle application
-> Derek manually inspects it
-> Derek explicitly says PASS
-> accepted snapshot
-> local Git commit/tag
-> remote push/verification
```

No accepted snapshot, commit, tag, or push belongs before manual inspection.
