# Demon Killer World-Time Intake — Project Oracle v0.1.10

Date reviewed: 3 August 2026  
Purpose: preserve and adapt proven Demon Killer world-time work without coupling Project Oracle to Godot or Demon Killer fiction

## Time laws preserved

| Demon Killer law | Oracle v0.1.10 adaptation |
| --- | --- |
| Four world seconds per real second | Exact integer rate: one real millisecond becomes four world milliseconds |
| Four world days per real day | Four Garden days pass in one real day; each Garden day lasts six real hours |
| Saved last real Unix timestamp | Saved `last_real_unix_milliseconds` |
| Forward-only catch-up | Backwards host time never rewinds the Garden |
| Atomic save checkpoint | Temporary file then rename with last-good backup |

## v0.1.10 time status

The v0.1.9 live-time correction is preserved. v0.1.10 does not change the clock rate, epoch, event queue timing, save checkpoint, or offline catch-up laws. Adam's new direct-address choice scaffold records decisions at the current world milliseconds only; it does not alter time.

## Deliberately not copied

Godot `Node`, `Time`, `FileAccess`, networking, scene integration, Demon Killer names, DK prefixes, player-body decay, enemy respawn, resource regrowth, loot deadlines, and save-shrine flow remain excluded.
