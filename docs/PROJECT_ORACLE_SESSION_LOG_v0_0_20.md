# Project Oracle Session Log v0.0.20

## Accepted base
v0.0.19 was fully accepted and closed out before v0.0.20 work began.

- accepted snapshot: `/home/dereksparks1982/DKLab/Archive/Project_Oracle_v0_0_19_ACCEPTED_20260811_185014`
- accepted commit: `3ca6204398fe338c3cd93bd6e8941f7d5c1b00a6`
- tag: `v0.0.19`
- GitHub `main` and the dereferenced `v0.0.19` tag were verified at the same commit.

## v0.0.19 live observations carried forward
Manual Brain Slice 3 testing proved that Yala could summarize self-knowledge and correctly answer that Yala had not created Adam. It also exposed reachability gaps: `why not?` lost conversational context, Gaia/Time questions fell into generic uncertainty, current-speaker questions sometimes returned Yala's own summary, and curiosity/drive/knowledge-gap questions were not yet reachable. The user-facing `[Soar selected: ...]` line was also unnecessary noise.

## Approved v0.0.20 work
- persistent Yala conversation mode: `Ctrl+Y` enters once and remains active across replies;
- `Escape` clears pending input and returns to the normal system prompt;
- remove user-facing Soar-selection diagnostics from normal conversation;
- reserve the top terminal row for live in-world Time;
- before Time exists, display exactly `In-world Time: Gaia has not yet created Time.`;
- once Gaia creates Time, replace that line with the continuously ticking in-world date/time;
- keep clock repaint isolated from the scrolling conversation body and command buffer;
- add short conversational follow-up context for questions such as `why not?`;
- improve Gaia, Time, genealogy, prior-command, Adam encounter, Wisdom/Sophia, current-speaker, knowledge-gap, curiosity, and drive reachability;
- add modest morphology/possessive tolerance and preserve multiword identity claims such as `the Oracle` as claims only;
- preserve the accepted v0.0.19 world/save and Soar long-term-memory continuity;
- use four spaces for source indentation, not tabs.

## Inherited stability protections
v0.0.20 preserves the v0.0.19 embedded-Soar stability repairs: the unused SML TCP listener remains suppressed and save/restore acceptance tests serialize native Soar-kernel lifetimes rather than overlapping kernels in one process.

## Release status
v0.0.20 remains a candidate until install-side automated validation passes, the real application launches, and Derek explicitly accepts the live result. Only then may the accepted snapshot, local commit/tag, and GitHub push/verification occur.
## Repair 1 - religious-boundary validation self-match

- The initial v0.0.20 candidate compiled and passed all 89 acceptance tests on the install machine.
- Final validation then failed because a validation-document sentence contained the same prohibited token the boundary gate scans for.
- Repair 1 removes that self-matching documentation wording while preserving the boundary gate and all approved v0.0.20 behavior.
- This is validation repair only; no new feature scope is added.

