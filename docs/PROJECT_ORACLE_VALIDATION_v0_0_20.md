# Project Oracle Validation v0.0.20

## Required base
Accepted v0.0.19 commit `3ca6204398fe338c3cd93bd6e8941f7d5c1b00a6`, tag `v0.0.19`, clean `main`.

## Required automated gates
- .NET 10 restore/build with warnings as errors.
- native root Linux publish and ELF check for `Project_Oracle_v0_0_20`.
- complete acceptance suite, all tests passing.
- real Soar 9.6.5 runtime and production load.
- one persistent Yala Soar session across multiple decisions.
- embedded Soar kernel suppresses the unused SML TCP listener.
- save/restore acceptance tests isolate native Soar-kernel lifetimes.
- semantic memory store/retrieval proof.
- episodic memory advancement proof and SQLite smoke files.
- impasse/substate deliberation proof.
- Brain Slice 3 self-model, concept lexicon, language roles, knowledge provenance, action memory, knowledge gaps, and learned-definition claim gates.
- hidden-Oracle boundary remains intact.
- Gaia-created Time and pre-Time clock law remain intact.
- v0.0.17, v0.0.18, and v0.0.19 `save_v2` acceptance/normalization; v0.0.16 rejection.
- persistent Yala mode and Escape exit.
- no user-facing `[Soar selected: ...]` diagnostics in normal conversation.
- exact pre-Time top-row wording: `In-world Time: Gaia has not yet created Time.`
- live clock transition and advancement after Gaia creates Time.
- live clock top-row/body isolation and command-buffer protection.
- conversational follow-up context and Brain Slice 3 reachability regressions.
- current README/authority alignment.
- four-space C#/Soar source indentation with no tabs.
- current-scope religious boundary passes without self-matching documentation text.
- export-manifest integrity.
- desktop and root-executable identity.

## Packaging-environment evidence
The packaging environment can verify payload hashes, source inventories, shell syntax, source-formatting gates, authority/canon gates, export-manifest integrity, and Soar source/runtime presence. This environment does not provide the .NET 10 SDK, so C# restore/build/publish and the full native acceptance suite remain mandatory install-side gates on Derek's machine and must not be reported as having run here.

## Native memory continuity
The smoke test expects semantic and episodic SQLite databases under `yala_soar_v0_0_18` by design. v0.0.20 reuses that directory to continue the same Yala long-term memory established by the accepted line.

## Manual acceptance gate
After automated FINAL PASS, launch the real application for Derek's inspection. Verify that `Ctrl+Y` enters persistent Yala mode, `Escape` exits it, repeated Yala prompts require no repeated prefix, Soar-selection chatter is absent, and the top in-world clock continues updating without overwriting typed text.

Also use a separate fresh-world test save so the top row can be observed as `In-world Time: Gaia has not yet created Time.` before Gaia creates Time and as a ticking date/time immediately after Gaia creates it. Preserve the existing continuing save.

No accepted snapshot, commit, tag, or push occurs until Derek explicitly says PASS.
