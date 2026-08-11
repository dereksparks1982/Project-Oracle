# Project Oracle Session Log v0.0.18

- v0.0.17 accepted, committed, tagged, and remotely verified at `0e328af4ab0b79c8f9e32199e93d3fecabfc977a`.
- Live testing exposed console LIVE-row/input collision and overly shallow Yala conversation.
- v0.0.18 approved as Protected Console Input + Yala Soar Brain Slice 2.
- Scope expanded to persistent Soar session, semantic memory, episodic memory, structured contacts/beliefs/episodes/drives, and impasse/substate deliberation.
- Canon corrected: Yala is both male and female. Monad rejected Yala for being both rather than exclusively one or the other and cast Yala into the Void.
- v0.0.18 continues the v0.0.17 save_v2 world rather than resetting again.


## Repair 2 - terminal body hard isolation

A live screenshot showed the first v0.0.18 console still writing LIVE rows into the typing area. Review also found the Repair 1 acceptance expectations were contradictory, so that repair could not be relied on. Repair 2 removes the LIVE renderer from the interactive path altogether. Idle Yala simulation may continue, but it cannot write status, move the cursor, or repaint dynamic title/body state while input is pending.

## Repair 3 - console isolation acceptance assertion repair

Repair 2 compiled and reached 46 passes with one acceptance failure. The implementation already hard-disabled terminal-body LIVE painting, but `LiveStatusTypingGuard` still expected an empty input buffer to permit visible LIVE body output. Repair 3 corrects that stale assertion to require `false` for both empty and active input buffers. The console implementation remains the Repair 2 hard-isolation design.
