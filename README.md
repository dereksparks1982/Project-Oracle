# Project Oracle

**Current build:** v0.0.18 candidate  
**Owner and final authority:** Derek Sparks  
**Platform:** Linux / .NET 10  
**Yala cognition:** Soar 9.6.5, Brain Slice 2

Project Oracle is a persistent autonomous-world simulation. **Oracle is not an in-world character.** Oracle is the outside/system-level author represented by Project Oracle itself: the code, simulation machinery, world-law resolver, records, and Master Key authority that make the simulated reality possible.

## Current cosmology

The settled in-world foundation is:

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Yala is inherently both male and female
            -> Monad rejected Yala for being both rather than exclusively one or the other
            -> Monad cast Yala into the Void
```

The primordial in-world being is called **Monad**. Do not call Monad Creator or Omega in active Project Oracle canon.

Yala may later claim the title **Creator** inside the world. That is Yala's claim or title and does not rewrite the settled fact that Wisdom made Yala.

A fresh world still begins with Yala in the Void. Gaia, in-world Time, Terra, Aether, Sol, Thalassa, Luna, the Garden, Adam, Eve, and later living kinds are absent until autonomous history actually establishes them.

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

Natural powers perform their own domains. Aether governs air and wind. Oracle provides the simulation capability that makes beings, actions, laws, and consequences possible; Oracle does not secretly perform every natural power's job.

## Yala Soar Brain Slice 2

v0.0.18 keeps the real supplied **Soar 9.6.5** runtime and expands Yala from the first operator-selection proof into a continuing cognitive session.

```text
Project Oracle supplies only Yala's available perception
-> one persistent Soar Yala agent remains alive for the session
-> Soar working memory represents the current situation
-> Yala's drives and current uncertainty influence candidate operators
-> Soar may resolve undecided choices through an impasse/substate
-> Yala attempts the selected action
-> Project Oracle resolves reality through world law
-> result, contacts, beliefs, claims, and episodes become part of Yala's continuing state
```

Brain Slice 2 adds:

- a persistent Soar agent instead of creating a new Soar mind for every thought;
- native Soar **semantic memory** for durable knowledge/contact recall;
- native Soar **episodic memory** for continuing decision episodes;
- structured Project Oracle contact, belief/claim, episode, and drive state;
- primitive goals/motives expressed through curiosity, caution, authority, companionship, comfort, and uncertainty;
- Soar impasse/substate deliberation when several operators remain plausible;
- richer conversation: introductions, remembered contacts, known facts, uncertainty, claims, commands, and clarification;
- strict separation between world truth, Yala's knowledge, Yala's beliefs, and what an unseen speaker merely claims.

Yala is deliberately **not** given an `Oracle exists` fact. When an unseen speaker says, for example, `I am Derek`, Yala may remember that an unseen source **claimed** the name Derek. That does not tell Yala the source is Oracle or establish the claim as world truth.

## Protected console input

v0.0.18 Repair 2 removes asynchronous LIVE rendering from the interactive console body entirely. There is no reserved LIVE row, no background cursor repositioning, and no dynamic status/title repaint while a command is being entered. The prompt owns the typing area exclusively.

Yala's autonomous simulation work can continue while Project Oracle waits for input, but that idle work is terminal-silent. Type `status` when you want a body-readable status report.

## Save continuity

v0.0.18 **continues the v0.0.17 `save_v2.json` world line**. It does not reset the world again.

A v0.0.17 save is accepted and normalized into the v0.0.18 cognition model. The earlier v0.0.16 Garden-era `save_v1.json` remains rejected and untouched.

Yala's corrected nature is canonical during normalization: old v0.0.17 `male`-only state is normalized to **male and female**, while world history, decisions, contacts, and other valid `save_v2` state continue.

Native Soar long-term databases are stored beside the active save in:

```text
yala_soar_v0_0_18/semantic.sqlite
yala_soar_v0_0_18/episodic.sqlite
```

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

After v0.0.18 is installed and validated, the main project directory contains the generated Linux executable:

```text
Project_Oracle_v0_0_18
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

### v0.0.18 Repair 3 validation note
Repair 3 preserves the Repair 2 console implementation and corrects the final stale acceptance assertion: asynchronous LIVE status is forbidden from the interactive terminal body whether the command buffer is empty or contains typed text.
