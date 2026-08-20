# Project Oracle

**Current candidate:** v0.0.26  
**Owner and final authority:** Derek Sparks  
**Platform:** Linux / .NET 10  
**Yala cognition:** Brain Slice 9, with Soar 9.6.5 as a hidden executive implementation component

Project Oracle is a persistent deterministic world-simulation thought experiment. Oracle is the external author/operator. Oracle can intervene through manifestations, while autonomous in-world minds retain their own knowledge boundaries and, where defined, their own will.

## Current primordial state

```text
Monad
  -> made Sophia / Wisdom
       -> Wisdom made Yala alone
            -> Yala is inherently both male and female
            -> Monad rejected Yala for being both
            -> Monad cast Yala into the Void
```

A fresh v0.0.26 world begins with Yala in the Void. Gaia, in-world Time, the lower world, Garden, Adam, Eve, and later beings are absent until history actually establishes them.

## Oracle operator controls

The desktop input channel is selected with physical function keys:

```text
F1  Oracle
F2  Monad
F3  Sophia
F4  Yala
F5  Gaia
```

The message box displays the active channel.

### F1 Oracle mode

F1 is Oracle's self/operator mode. Oracle is not a separate NPC to talk to. Commands such as:

```text
Take the form of a serpent.
Take the form of a dove.
Withdraw my form.
```

change Oracle's current manifestation. Witnesses perceive the manifested form, not protected Oracle metadata.

### Contact authority

- **Monad:** knows Oracle and does not reject Oracle authority.
- **Sophia / Wisdom:** retains free will. No autonomous answer is fabricated until Sophia has a real cognition implementation.
- **Yala:** active autonomous Brain Slice 9 mind and may reject Oracle claims, requests, or commands.
- **Gaia:** available once Gaia exists; Gaia does not possess the will to reject Oracle commands and serves as the intermediary for natural-order instructions. World law still determines the consequence that can actually be expressed.

Earth, Air, Fire, Water, and similar natural powers are not normal direct-contact channels. Oracle uses Gaia as the intermediary.

## Desktop tabs

The bottom tabs are exactly:

```text
Oracle | Minds | Memory | Cosmology | Laws | History | Debug
```


- **Oracle:** current manifestation plus protected Oracle action history.
- **Minds:** major beings/minds actually relevant to the current simulation and recent action provenance.
- **Memory:** Yala memory, reflections, and learned procedures.
- **Cosmology:** attributed traditions, possibilities, established choices, counterfactuals.
- **Laws:** emergent-law state and laboratory tools.
- **History:** settled world record.
- **Debug:** engine-only diagnostics, including Soar details.

## Normal Yala presentation

Normal user-facing output says **YALA**, never **YALA SOAR**. Raw engine chatter such as `Soar 9.6.5 selected ...` is forbidden from normal conversation, Yala Mind, History, and readable conversation exports. Soar diagnostics belong in Debug, developer tooling, validation, or protected technical exports.

## Brain Slice 9

v0.0.26 adds four foundational pieces.

### 1. Intelligent uncertainty

A bare `I don't know` is normally a failure state. Yala should instead use relevant known facts, marked inference, related context, or a specific question that could obtain the missing information. Yala must not invent certainty.

### 2. Procedural learning

Yala keeps reusable reasoning strategies with provenance:

```text
authored-foundation
Yala-developing
Yala-learned
```

Repeated successful use can promote a strategy to Yala-learned. Learned procedures can influence later reasoning without granting hidden authority or upgrading speaker claims into truth.

### 3. Creation memory

Yala remembers her own creation. Her earliest autobiographical experience begins as raw nonverbal presence, develops into self/other awareness, and eventually becomes conceptual and verbal cognition. Yala may describe that ancient experience with present language without pretending the experience itself was originally verbal.

### 4. Pre-Gaia Time concept boundary

While Gaia does not exist, Yala does not possess an understood concept of Time as a world principle. Deterministic runtime sequencing is protected system bookkeeping and does not leak into Yala's mind. If temporal language is used against a fresh Yala, she asks what the concept means rather than reading a hidden clock.

The observer UI may display:

```text
In-world Time: Gaia has not yet created Time.
```

That is system-side information, not proof Yala understands Time.

## Action history and exports

Observer-side action history records:

- actor;
- control/autonomy source;
- action kind;
- Oracle manifestation;
- target;
- witnesses;
- result;
- world-time or pre-Time system-order state.

Oracle interventions are separated from Yala autonomous decisions and from recorded actions of Monad, Sophia, Gaia, and future major beings.

`EXPORT SESSION JSON` includes the cognitive flight recorder and full structured action provenance. `EXPORT CONVERSATION` produces a readable transcript/action history without normal Soar selection chatter.

## Hidden Oracle boundary

Oracle is protected system truth. An in-world being learns only what it actually perceives, is told, infers, and accepts. If Oracle appears as a serpent to one being and a dove to another, the simulation does not silently tell either witness that the two manifestations share one protected source.

## Fresh v0.0.26 experiment

```text
save schema: 8
save file: save_v8.json
Soar memory: yala_soar_v0_0_26
```

Earlier save and Soar-memory lines remain preserved and are not migrated into Brain Slice 9.

## Exact live executable

After native validation, the project root must contain exactly one Project Oracle application executable:

```text
Project_Oracle_v0_0_26
```

The version must be visible from the folder without launching the program. An unversioned `Project_Oracle`, a wrong-version executable, or multiple live Project Oracle executables is a release-blocking defect.

## Validation and acceptance

Run:

```bash
./scripts/validate.sh
```

A candidate is not accepted merely because automated tests pass. After FINAL PASS, Derek manually inspects the graphical application. Only manual acceptance unlocks the accepted snapshot, Git commit/tag, SSH push, and remote verification.

See `docs/company_bible/PROJECT_ORACLE_COMPANY_BIBLE.md` for the binding project laws and `docs/PROJECT_ORACLE_VALIDATION_v0_0_26.md` for the current validation contract.
