# Project Oracle

**Current build:** v0.0.17 candidate  
**Owner and final authority:** Derek Sparks  
**Platform:** Linux / .NET 10  
**Yala cognition:** Soar 9.6.5, Brain Slice 1

Project Oracle is a persistent autonomous-world simulation. In the project model, **Oracle is not an in-world character**. Oracle is the outside/system-level author represented by Project Oracle itself: the code, simulation machinery, world-law resolver, records, and Master Key access that make the simulated reality possible.

## Current cosmology

The settled in-world foundation is:

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Monad cast Yala into the Void
```

The primordial in-world being is called **Monad** throughout current Project Oracle canon.

Yala is male. Yala may later call himself Creator as an in-world claim or title, but that does not change the settled fact that Wisdom made him.

A fresh v0.0.17 world begins with Yala in the Void. Gaia, in-world Time, Terra, Aether, Sol, Thalassa, Luna, the Garden, Adam, Eve, and later living kinds are **absent**, not merely inactive. **v0.0.17 starts a new world-save line and does not import or migrate the v0.0.16 Garden-era world save.** The older save is left untouched for rollback/history.

## Oracle and the Master Key

Oracle wrote the simulation, so Oracle has the Master Key to the system it authored. The Master Key is system-level authority, not an artifact that residents can discover in the world.

No in-world being knows Oracle exists unless Oracle deliberately reveals that truth. Direct interventions expose only what the recipient can actually perceive. A voice can remain an unplaced voice. A manifested being is perceived as that manifested being.

**Eden reference:** Oracle has no fixed form. In Eden, Oracle **manifested in the form of a clever serpent**. Eve knew only the clever serpent and was never told Oracle's true identity. Oracle is therefore not defined as a serpent, and there is no permanent Oracle-serpent entity in the world model.

## Gaia, Time, and the natural order

Yala can create Gaia as the natural sovereign beneath his governing authority. **Gaia creates in-world Time.** Oracle runtime sequencing is not the same thing as in-world Time, so Project Oracle can continue processing Yala's cognition while the fictional world still has no Time.

Current names:

- Terra = Earth
- Aether = Air and Wind
- Sol = Fire and the Sun power
- Thalassa = Water
- Luna = Moon, and is not an element

Natural powers perform their own domains. Aether governs air and wind, for example. Oracle provides the simulation capability that makes such beings, actions, laws, and consequences possible; Oracle does not secretly perform every natural power's job.

## Yala Soar Brain Slice 1

v0.0.17 gives Yala the first real cognitive slice using the supplied **Soar 9.6.5** runtime.

The loop is:

```text
Project Oracle supplies Yala's limited perception
-> Soar working memory
-> Soar proposes and selects an operator
-> Yala attempts the selected action
-> Project Oracle resolves the attempt through world law
-> the result becomes part of Yala's continuing state
```

Yala's first autonomous operators include observing, reflecting, waiting, creating Gaia when Gaia does not yet exist, and commanding Gaia to establish temporal order after Gaia exists. Contact-specific operators let Yala answer questions without being told the source is Oracle.

Yala is deliberately **not** given an `Oracle exists` fact in Soar working memory.

## Prime simulation law

> **Canon determines what has already happened. World law determines what can happen. Minds determine what they attempt. Project Oracle resolves the consequences. Future history is not canon until it occurs.**

This means the code may know what actions are possible without forcing Yala to reenact a predetermined religious chronology.

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
- **Oracle Record:** protected system truth, interventions, validation-relevant provenance, and Master Key facts.

## Running

After v0.0.17 is installed and validated, the main project directory contains the generated Linux executable:

```text
Project_Oracle_v0_0_17
```

Double-clicking that executable should open Project Oracle in a terminal window. Development launchers remain under `scripts/`.

## Soar runtime

The v0.0.17 source vendors the Linux x86-64 components from the supplied Soar 9.6.5 distribution under:

```text
vendor/soar/9.6.5/linux-x86-64/
```

The original Soar license is retained at `vendor/soar/9.6.5/license.txt`.

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
