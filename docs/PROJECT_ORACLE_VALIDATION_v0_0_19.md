# Project Oracle Validation v0.0.19

## Required automated gates
- .NET 10 restore/build with warnings as errors.
- native root Linux publish and ELF check.
- complete acceptance suite, all tests passing.
- real Soar 9.6.5 runtime and production load.
- one persistent Yala Soar session across multiple decisions.
- embedded Soar kernel suppresses the unused SML TCP listener rather than binding default port 12121.
- semantic memory store/retrieval proof.
- episodic memory advancement proof and SQLite smoke files.
- impasse/substate deliberation proof.
- Brain Slice 3 self-model, concept lexicon, language roles, knowledge provenance, action memory, knowledge gaps, and learned-definition claim gates.
- own-action answers and introspection regressions.
- obsolete male-only World Record normalization regression.
- hidden-Oracle boundary remains intact.
- Gaia-created Time and pre-Time clock law remain intact.
- v0.0.17 and v0.0.18 `save_v2` acceptance/normalization; v0.0.16 rejection.
- asynchronous LIVE status remains forbidden from the interactive console body.
- current README/authority alignment.
- root `Project_Oracle_v0_0_19` and desktop launcher identity.
- export manifest hashes.
- no unapproved religious genealogy is introduced by Brain Slice 3.

## Acceptance gate
Automated FINAL PASS must be followed by launching the real application for Derek's manual inspection. No accepted snapshot, commit, tag, or push occurs until Derek explicitly says PASS.

## Packaging-environment evidence
The supplied Soar 9.6.5 Linux runtime can be exercised directly during packaging to source the Brain Slice 3 productions. The packaging environment may not provide the .NET 10 SDK; when unavailable, C# restore/build/publish/full acceptance remain mandatory install-side gates and must not be reported as having run locally.

## Native memory continuity
The smoke test expects semantic and episodic SQLite databases under `yala_soar_v0_0_18` by design. v0.0.19 reuses that directory to continue the same Yala long-term memory.

## Repair 1 failure addressed
The first v0.0.19 install reached 41 passing acceptance tests, then Soar reported `Error binding the listener socket to its port number` and the native process exited with status 139. Repair 1 starts the in-process Soar kernel with listener port `0`, which Soar defines as listener suppression, because Project Oracle does not use remote SML connections. The complete suite remains authoritative on the target machine.

## Repair 2 failure addressed
Repair 1 proved that listener suppression fixed the earlier TCP-port collision, then the acceptance process exited with status 139 immediately after the speaker-provenance test. Inspection found that the next two save/restore regressions each created a restored `OracleSimulation` while the source `OracleSimulation` and its native Soar kernel were still alive in the same process. Repair 2 serializes those kernel lifetimes: the source simulation creates its snapshot and is disposed before `OracleSimulation.Restore(...)` constructs the replacement simulation. A dedicated regression and pre-suite source gate protect this rule. No Brain Slice 3 behavior is changed by this repair.
