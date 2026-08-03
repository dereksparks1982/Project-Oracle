# Demon Killer World-Time Intake — Project Oracle v0.1.2

Date reviewed: 1 August 2026  
Source supplied by Derek: `scripts.zip`  
Purpose: preserve and adapt months of proven Demon Killer world-time work without coupling Project Oracle to Godot or Demon Killer fiction

## Scripts examined

- `scripts/server/DKPersistentWorldServer.gd`
- `scripts/save/DKWorldClockOfflineCatchUpContract.gd`
- World-clock and atomic-save sections of `scripts/save/DKSaveManager.gd`
- Continue/catch-up integration in `scripts/tests/DKSeamlessCellStreamingController.gd`
- Related resource, enemy, and body deadline references found across the archive

The archive contained 240 paths and approximately 1.28 MB of scripts. The time and save systems were traced across server, save, continue, world, and test layers rather than inferred from one constant.

## Demon Killer laws carried into Oracle

| Demon Killer law | Oracle v0.1.2 adaptation |
| --- | --- |
| `WORLD_SECONDS_PER_REAL_SECOND = 4.0` | Exact integer rate: one real millisecond becomes four world milliseconds |
| `WORLD_DAYS_PER_REAL_DAY = 4.0` | Four Garden days pass in one real day; each Garden day lasts six real hours |
| Saved last real Unix timestamp | Saved `last_real_unix_milliseconds` |
| Startup catch-up uses `max(now - saved, 0)` | Forward-only reconciliation; backwards host time never rewinds the Garden |
| Saving is a checkpoint, not a universe freeze | Oracle continues from elapsed real time after closure |
| Same-slot write-back after catch-up | One official save is updated immediately after load/catch-up |
| Temporary file then rename | Atomic JSON checkpoint commit |
| Preserve a valid previous primary | Last-good backup retained; a corrupt primary cannot overwrite it |
| Catch-up lanes are explicit | v0.1.2 applies only world-clock catch-up; future events gain versioned lanes |

## Oracle-specific additions

- Exact creation epoch: Year 1, Month 1, Day 1, 01:01:01.
- Twelve numbered months and a 365-day calendar contract.
- Dawn, day, dusk, and night derived from world time.
- Eight deterministic lunar phases over a 29.530588-day synodic cycle.
- Creator Record evidence for live advance, offline catch-up, and backwards-clock detection.
- Save schema and project-version validation.

## Deliberately not copied

- Godot `Node`, `Time`, `FileAccess`, networking, and scene integration.
- Demon Killer's 9600 BCE start, September date, 17:30 start time, ancient-body impact countdown, and kingdom placeholders.
- Player-body decay, enemy respawn, resource regrowth, loot-bag deadlines, region state, and save-shrine flow.
- Demon Killer names or `DK` prefixes in Project Oracle code.

Those exclusions protect both projects. Later Oracle ecology, death, and event systems may reuse the deadline pattern only through explicit Oracle builds.
