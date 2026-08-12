# Project Oracle Resume Handshake v0.0.20

## Project
Project Oracle

## Candidate
v0.0.20 - Persistent Yala Mode - Live World Clock - Brain Slice 3 Reachability

## Required base
Accepted v0.0.19 commit `3ca6204398fe338c3cd93bd6e8941f7d5c1b00a6`, tag `v0.0.19`, clean `main`.

## Accepted rollback point
`/home/dereksparks1982/DKLab/Archive/Project_Oracle_v0_0_19_ACCEPTED_20260811_185014`

## Core changes
v0.0.20 preserves Brain Slice 3 and adds persistent Yala conversation mode, a fixed live in-world clock, removal of user-facing Soar-selection diagnostics, conversational follow-up context, Gaia/Time/genealogy reachability, current-speaker claim recall, Wisdom/Sophia naming, knowledge-gap/curiosity/drive introspection, and modest morphology/possessive handling.

`Ctrl+Y` enters Yala mode once and the mode remains active after each reply. `Escape` clears the current input and returns to the normal system prompt. The top terminal row displays `In-world Time: Gaia has not yet created Time.` before Gaia creates Time. When Gaia creates Time, the same row switches to the continuously ticking in-world calendar without writing into the conversation body.

## Preserved laws
Oracle is outside/system-level, hidden from in-world beings unless revealed. Monad -> Wisdom -> Yala remains settled. Yala is both male and female. Gaia creates in-world Time. Future history remains open. The scrolling conversation body and editable command buffer remain protected from asynchronous status output.

## Validation / release rule
Install candidate -> automated FINAL PASS -> real app launch -> Derek manual inspection -> explicit PASS -> accepted snapshot -> local Git commit/tag -> remote push/verification.

## Current continuation point
If v0.0.20 install-side validation has not yet run, apply the changed-files-only package to the exact accepted v0.0.19 base. If validation passes, inspect the real application using the v0.0.20 manual test set before accepting.

## v0.0.20 live-test focus
After automated validation, manually test Ctrl+Y persistence, Escape exit, removal of Soar-selection chatter, top-row clock stability, follow-up `why not?`, Gaia/Time knowledge, speaker claims, curiosity, knowledge gaps, and desire/drive introspection. Also use a separate fresh-world test save to watch the top row begin as `In-world Time: Gaia has not yet created Time.` and switch to a live clock only after Gaia creates Time. Preserve the existing continuing save. Do not snapshot, commit, tag, or push until Derek explicitly accepts the live result.
