# Project Oracle Session Log v0.0.19

## Accepted base
v0.0.18 was accepted after Repair 3 and closed out before v0.0.19 work.

- accepted snapshot: `/home/dereksparks1982/DKLab/Archive/Project_Oracle_v0_0_18_ACCEPTED_20260811_172300`
- accepted commit: `9c5f5c11ebde608a228c683099c68f5c7173133b`
- tag: `v0.0.18`
- GitHub main and dereferenced tag were verified at the same commit.

## Live observations driving Brain Slice 3
The v0.0.18 app showed that Yala could converse and remember but still answered some own-history questions with generic uncertainty, treated `tell me what you know` as a command, and restored obsolete male-only World Record wording from older save history.

## Approved v0.0.19 work
- normalize obsolete system-generated Yala history;
- add self-model and own-action memory;
- add foundational concept lexicon and lightweight grammar;
- add introspective action/contact/belief/knowledge summaries;
- add knowledge provenance and contradiction boundaries;
- store new word definitions first as speaker claims;
- create knowledge gaps for unknown concepts and connect them to curiosity;
- preserve the same `save_v2` world and native Soar long-term memory;
- preserve the v0.0.18 terminal-body hard isolation.

## Release status
v0.0.19 remains a candidate until install-side automated validation passes, the real application is manually inspected, and Derek explicitly says PASS.

## Repair 1 after first install-side failure
The first v0.0.19 candidate restored, built, and published successfully. The acceptance suite reached 41 passing tests before the native Soar runtime failed while trying to bind its default SML listener socket and exited with status 139. The installer correctly rolled the repository back to accepted v0.0.18.

Repair 1 changes the embedded Soar kernel startup from the default listener port to port `0`, Soar's listener-suppression mode. Project Oracle does not require remote SML connections, so no listener is needed. A regression gate now protects that embedding rule.

## Repair 2 after Repair 1 native crash
Repair 1 restored, built, and published successfully and confirmed that the unused Soar TCP listener was suppressed. The acceptance suite advanced through the speaker-claim provenance test, then the native process again exited with status 139 before the old-history normalization test could report a result. The installer correctly rolled the repository back to accepted v0.0.18.

Inspection showed that `OldWorldRecordCanonNormalises` and `ActionMemorySurvivesSaveRestore` kept their source `OracleSimulation` alive while constructing a restored simulation. Because each simulation owns an embedded native Soar kernel, those tests overlapped two kernels inside one process. Repair 2 introduces a `SnapshotAndDispose` test helper and requires the source simulation/kernel to be disposed before restoration starts. A dedicated lifetime-isolation acceptance regression and a pre-suite validation gate now protect that contract. Brain Slice 3 product behavior is unchanged.
