# Project Oracle Architecture v0.0.20

## Runtime layers

1. **Oracle/system layer**: console, persistence, records, world-law resolver, clock coordination, and privileged system authority.
2. **World state**: settled in-world beings, natural state, World Record, and fictional Time when Gaia has established it.
3. **Yala cognition**: persistent Soar 9.6.5 agent plus Project Oracle's perception/language/memory boundary.
4. **Language foundation**: `YalaLanguageInterpreter`, `YalaGrammar`, and `YalaLexicon` turn utterances into limited structured language features without granting hidden system truth.
5. **Self and knowledge model**: `YalaSelfModel`, knowledge propositions/provenance, action memory, knowledge gaps, contacts, beliefs, learned lexeme claims, episodes, and drives.
6. **Resolution**: Soar selects an attempted action or response; Project Oracle applies world law and records the result.

## Brain Slice 3 data flow

```text
contact text
-> parse direct-call target
-> interpret language roles / question / negation / definition claim
-> attach only Yala-available current state and memory
-> persistent Soar input-link
-> productions / operator selection / optional impasse-substate
-> reply or attempted action
-> world-law resolution
-> structured memory/provenance update
-> semantic/episodic continuation
```

The parser may structure what was heard. It does not decide that a speaker's statement is true.

## Self-model boundary

The self-model may expose facts Yala can lawfully know about Yala: identity, male-and-female nature, Wisdom origin, Monad rejection, current location, and personally completed actions. It must not expose Oracle or hidden system-only truth.

## Concept lexicon

The built-in lexicon contains foundational vocabulary needed for the current experiment. Each lexeme may carry part of speech, a basic meaning, related concepts, contrasts, and conceptual relations. Personal anchors arise from world state and memory. New definitions supplied through conversation are stored as speaker claims in Yala cognition and native semantic memory.

## Provenance

Knowledge propositions identify their source class, including personally performed, personally experienced, remembered, inherited knowledge, inferred, claimed-by-another, hypothesis, and unknown. Provenance is part of the cognition boundary, not a substitute for later evidence evaluation.

## Save and Soar continuity

`save_v2.json` remains the world/cognition save line. v0.0.17, v0.0.18, and v0.0.19 snapshots are supported predecessors for v0.0.20 normalization. Native Soar long-term databases deliberately remain in `yala_soar_v0_0_18` so Brain Slice 3 continues the same Yala memory.

## Console isolation and live header

Interactive input owns the scrolling terminal body. The only asynchronous visible surface is the dedicated top-row in-world Time header. `LiveWorldClockSurface` reserves row 1 and constrains scrolling to rows 2 through the terminal height. A refresh saves the current cursor, paints only row 1, then restores the cursor.

Before Gaia creates Time the header states `In-world Time: Gaia has not yet created Time.` After Gaia creates Time, the same header displays the live `PersistentWorldClock` calendar and updates continuously. No clock refresh may append a body line or mutate `ConsoleInputLine`.

`ConsoleConversationMode` holds Yala-targeting state separately from editable text. `Ctrl+Y` enters Yala mode, replies preserve the mode, and `Escape` clears pending text and returns to the system prompt.
