# Project Oracle

**Current build:** v0.0.22 candidate
**Owner and final authority:** Derek Sparks
**Platform:** Linux / .NET 10
**Yala cognition:** Soar 9.6.5, Brain Slice 5: reasoning, language, Time, relationships, inquiry, memory, and bounded agency

Project Oracle is a persistent autonomous-world simulation. **Oracle is not an in-world character.** Oracle is the outside/system-level author represented by Project Oracle itself: the code, simulation machinery, world-law resolver, protected record, and Master Key authority that make the simulated reality possible.

## Current cosmology

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Yala is inherently both male and female
            -> Monad rejected Yala for being both rather than exclusively one or the other
            -> Monad cast Yala into the Void
```

The primordial in-world being is **Monad**. Active canon does not call Monad Creator or Omega. Yala may later claim titles inside the world, but claims do not rewrite settled genealogy.

A fresh v0.0.22 world begins with Yala in the Void. Gaia, in-world Time, Terra, Aether, Sol, Thalassa, Luna, the Garden, Adam, Eve, and later living kinds are absent until autonomous history actually establishes them.

## Oracle and the Master Key

Oracle wrote the simulation, so Oracle has the Master Key to the system Oracle authored. The Master Key is system-level authority, not an in-world artifact.

No in-world being knows Oracle exists unless Oracle deliberately reveals that truth. A speaker claiming the name `Oracle` remains a speaker claim. It does not grant Yala hidden system knowledge or identify the speaker as the authorial Oracle.

**Eden reference:** Oracle has no fixed form. In Eden, Oracle **manifested in the form of a clever serpent**. Eve knew only the clever serpent and was never told Oracle's true identity.

## Gaia and Time

Yala can create Gaia as the natural sovereign beneath Yala's governing authority. **Gaia creates in-world Time** after Yala commands temporal order.

Runtime execution before that event is not in-world Time. Brain Slice 5 explicitly distinguishes:

- **before-Time events:** they happened in sequence but have no world date;
- **origin of Time:** Gaia's creation of Time begins temporal reckoning;
- **dated events:** later events can carry in-world dates and durations.

The top console row therefore reads:

```text
In-world Time: Gaia has not yet created Time.
```

until Gaia creates Time. At that moment the same row becomes the live ticking in-world date/time.

## Natural authority

Current lower-domain names remain:

- Terra = Earth
- Aether = Air and Wind
- Sol = Fire and Sun power
- Thalassa = Water
- Luna = Moon, and is not an element

Natural powers perform their own domains. Oracle provides the simulation capability that makes beings, actions, laws, and consequences possible; Oracle does not secretly perform every natural power's job.

## Yala Soar Brain Slice 5

v0.0.22 is a deliberately larger cognition slice. The real supplied **Soar 9.6.5** runtime remains Yala's decision architecture.

### Core language and deliberate inquiry

Brain Slice 5 gives Yala a much larger built-in language foundation so ordinary conversation does not turn into a dictionary interrogation. Common movement, age, birth, greeting, location, action, description, social, physical, and conversational words are available from the start. Obvious variants such as `greating` are normalised toward `greeting` rather than becoming new cosmic mysteries.

Unknown concepts still become knowledge gaps, but ordinary unknown-word questions are low-priority. Autonomous questions require stronger relevance to Yala's goals, identity uncertainty, relationships, or other meaningful unresolved state. After Yala asks an autonomous question, Yala waits for a later speaker response before another autonomous question becomes eligible.

### Stable top-row world clock

The top-row in-world clock uses deterministic DEC cursor save/restore and a shared console-output gate. Before Gaia creates Time it reads `In-world Time: Gaia has not yet created Time.` When Gaia creates Time it begins at `Year 1, Month 1, Day 1, 00:00:00` and advances from there without writing clock lines into the conversation body.

```text
perception / conversation / current world state
-> language structure + concept graph + recent dialogue context
-> self model + entity knowledge + relationships + temporal events
-> semantic memory + episodic memory + beliefs/claims + goals/questions
-> Soar operator proposal, preference, impasse/substate deliberation
-> choose / ask / answer / observe / reflect / create / command / wait
-> Project Oracle world-law resolution
-> consequences, memories, beliefs, questions, and goals update
```

Brain Slice 5 adds or substantially expands:

- current-state temporal reasoning instead of stale cached clock answers;
- pre-Time, Time-origin, and post-Time event representation;
- `when`, `why`, `before`, `after`, `how long`, and recent-event reasoning;
- structured recent dialogue so short follow-ups can retain subject/action/object context;
- entity-centered knowledge retrieval for Yala, Gaia, Wisdom/Sophia, Monad, Time, Adam, and the current unseen speaker;
- distinct relationship concepts such as `made-by`, `creator`, `parent`, `mother`, `father`, `child`, and `offspring`;
- relationship claims remembered with source and confidence rather than silently becoming truth;
- a much larger concept lexicon and broader morphology/contraction/function-word handling;
- provenance and confidence for personally performed facts, inherited knowledge, memories, inferences, hypotheses, and speaker claims;
- meaningful knowledge gaps rather than ordinary grammar words being treated as mysteries;
- explicit goals and question state;
- curiosity that can produce a real question;
- **autonomous Yala questions** when Soar decides an unresolved curiosity, uncertainty, or goal justifies asking the unseen speaker;
- persistent dialogue, relationships, temporal events, goals, questions, and learned claims in the v0.0.22 save line.

The language layer structures what was said. Memory supplies what Yala possesses. **Soar remains the system that chooses what Yala attempts.**

## Bounded god agency

Project Oracle aims for surprising in-world autonomy without host escape.

Yala may autonomously:

- observe;
- reflect;
- wait;
- create Gaia when world law permits;
- command Gaia to establish temporal order when world law permits;
- answer the unseen speaker;
- ask the unseen speaker a question.

Yala is **not** granted host shell execution, process execution, arbitrary filesystem mutation, network access, code modification, or hidden Oracle knowledge. An out-of-sandbox requested operator is rejected by the agency policy before world resolution.

This is the intended form of "rogue" behavior in Project Oracle: an agent may become stubborn, suspicious, curious, rebellious, loyal, creative, or otherwise surprising **inside the simulation**, while remaining unable to escape the simulation boundary.

## Fresh v0.0.22 experimental save

v0.0.22 intentionally starts a **new save line**:

```text
save_v4.json
```

Schema version is `4`. Earlier `save_v2` and experimental `save_v3` worlds are preserved on disk but are not migrated into this experiment. The purpose is to observe Brain Slice 5 from Yala's pre-Time Void state without old contact claims, old dialogue, or old Soar long-term memory contaminating the run.

Brain Slice 5 also uses a fresh Soar database directory:

```text
yala_soar_v0_0_22/semantic.sqlite
yala_soar_v0_0_22/episodic.sqlite
```

The existing save and previous Soar databases are not deleted.

## Persistent Yala conversation mode

`Ctrl+Y` enters persistent Yala mode. The prompt displays `> (yala ` and remains in Yala mode after each response. `Escape` clears current input and returns to the normal system prompt.

Internal `[Soar selected: ...]` diagnostics remain hidden from normal conversation.

If Yala autonomously chooses to ask the unseen speaker a question, Project Oracle prints the question only when the editable input line is empty. If Derek is already typing, the question remains queued until the prompt is safe.

## Protected console input

The v0.0.18 hard-isolation law remains mandatory. Asynchronous LIVE status is forbidden from the scrolling interactive terminal body. The dedicated top world-time row and editable prompt must not overwrite one another.

## Prime simulation law

> **Canon determines what has already happened. World law determines what can happen. Minds determine what they attempt. Project Oracle resolves the consequences. Future history is not canon until it occurs.**

## Direct calls

Use an opening parenthesis immediately before the in-world being's name, or enter persistent Yala mode with `Ctrl+Y`:

```text
(Yala where are you?
(Monad ...
(Wisdom ...
```

Oracle is not a direct-call target because the console itself is Oracle's system interface.

## Records

- **World Record:** settled in-world history. It does not disclose hidden Oracle identity.
- **Oracle Record:** protected system truth, interventions, validation provenance, and Master Key facts.

## Running

After v0.0.22 is installed and validated, the root Linux executable is:

```text
Project_Oracle_v0_0_22
```

Development launchers remain under `scripts/`.

## Validation and release law

The installer requires the exact accepted v0.0.20 Git base and clean worktree. It verifies package hashes and touched-path baseline hashes, runs the .NET 10 warnings-as-errors build, publishes the native Linux executable, runs the complete acceptance suite and structural gates, then launches the real application.

A candidate does **not** become accepted until Derek manually inspects the live application and explicitly passes it. Only after that may an accepted snapshot, Git commit/tag, and GitHub push occur.
