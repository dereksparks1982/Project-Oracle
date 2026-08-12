# Project Oracle Session Log v0.0.22

v0.0.20 was fully accepted, snapshotted, committed, tagged, pushed, and remotely verified before v0.0.22 work began.

Accepted base commit: `be0bf1f6aa18989856284b5766e323793d6f5f6b`.

## Why Brain Slice 5 is larger

Live v0.0.20 conversation showed that Yala could retrieve Gaia facts, remember a speaker identity claim, follow `why not?`, and expose knowledge gaps, but still had shallow temporal concepts, weak relationship learning, noisy unknown-word detection, limited short-follow-up context, and no autonomous inquiry.

The approved v0.0.22 scope deliberately takes a larger cognition slice: temporal reasoning, dialogue context, relationships, entity knowledge, belief confidence, goals, question generation, autonomous `ask-speaker`, and a bounded-agency policy.

## Fresh experiment

v0.0.22 starts `save_v4.json` schema 4 and a fresh `yala_soar_v0_0_22` memory directory. Earlier saves remain preserved. This prevents old conversation/contact history from being mistaken for fresh behavior and allows Derek to watch Time come into existence in a clean run.

## Bounded autonomy

The goal is strong in-world autonomy, including surprising choices and unsolicited questions, without any host escape surface. The allowed action set is explicit and host shell/process/network/file/code capabilities remain denied.

## Acceptance law

Install-side automated validation must pass, then the real application must launch for Derek's manual interrogation. No accepted snapshot, Git commit/tag, or GitHub push occurs before explicit manual PASS.


## Repair history before manual inspection

The initial v0.0.22 candidate carried Brain Slice 5 forward successfully enough to compile and reach 140/0 acceptance on Derek's machine, but packaging seams blocked installation. Repair 1 aligned the sealed v0.0.21 Repair 6 baseline hashes. Repair 2 restored the zero-epoch `OracleCalendar.cs` payload so export-manifest verification matched the candidate. Repair 3 hardened baseline recovery and rollback so known v0.0.20, v0.0.21 Repair 6, and recoverable v0.0.22 candidate bytes can be safely distinguished without trusting only a version string.

## v0.0.22 Brain Slice 5 additions

- Core language expands beyond 400 built-in concepts so ordinary vocabulary is not treated as a knowledge gap.
- Autonomous inquiry is deliberate: low-value dictionary questions are below the autonomous priority floor, and Yala waits for a speaker response before asking again.
- The live top-row world clock uses deterministic cursor save/restore plus a shared output gate.
- v0.0.22 starts schema 4 at `save_v4.json` with fresh `yala_soar_v0_0_22` long-term memory while preserving earlier save lines.
## Repair 4 live-interrogation findings

Manual v0.0.22 interrogation exposed a reachability cluster that automated coverage had not exercised: an `I am making...` predicate was being consumed as a claimed speaker name, `Gaia made Time?` failed to reach known temporal origin memory, `do you belive Wisdom is your mother` fell through despite the stored relationship claim, `what is florbnax` could not retrieve a just-learned speaker definition, and `who told you what florbnax means` treated `told` as unknown. Yala also became too quiet after dictionary questions were demoted. Repair 4 keeps unknown-word questions non-autonomous, adds purposeful speaker-understanding questions, and adds exact regressions for all of these live phrases. The acceptance inventory is 147 tests.
## Repair 5 acceptance finding

The complete Repair 4 run compiled cleanly and passed 146 of 147 acceptance tests. The only failure was `what is learned word retrieves the attributed speaker definition`. Topic classification was already correct (`word-meaning`), but `YalaReplyRealizer.WordMeaningTarget` had its own target extraction path and omitted the exact `what is <word>` form, so realization could not recover the learned lexeme even though it was stored correctly. Repair 5 aligns the realizer with the interpreter for that form. No other Brain Slice 5 behavior is changed.

