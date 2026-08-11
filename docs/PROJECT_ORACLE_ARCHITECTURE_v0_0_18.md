# Project Oracle Architecture v0.0.18

## Layer model
```text
Derek / Oracle system authority
        |
Project Oracle runtime
  - world law / records / persistence / Master Key
  - cognition I/O boundary
        |
In-world state: Monad -> Wisdom -> Yala -> future lower creation
        |
Persistent Yala Soar 9.6.5 Brain Slice 2
```

Oracle is not stored as an in-world entity.

## Brain Slice 2 boundary
`OracleSimulation` owns one `YalaSoarMind` for the session. `SoarKernelHost` owns one official Soar 9.6.5 kernel/agent. Each decision creates a temporary world-input structure, runs Soar, reads a `yala-action`, then removes the temporary input while leaving the agent and its memories alive.

Soar receives location, current creation state, last action/result, drives, uncertainty, and structured contact cues. It never receives an Oracle identity fact.

Semantic memory and episodic memory are enabled. With a real save path they persist to SQLite under `yala_soar_v0_0_18/`. Project Oracle also persists structured contacts, beliefs/claims, episodes, and drives in `save_v2.json` so authority boundaries remain inspectable.

## Deliberation
When multiple operators remain plausible, Soar can enter a tie impasse/substate and create preferences that resolve the choice. This is used for ordinary cognition and statement handling rather than hard-wiring every choice in C#.

## Console surface
`ConsoleInputLine` owns the typed command buffer. `LiveConsoleSurface` may repaint the visible LIVE row only while the input buffer is empty. While typing, body status is frozen and terminal-title status may continue. This prevents the v0.0.17 fixed-row overrun.

## Save continuity
Schema remains save schema 2. v0.0.17 `save_v2` is accepted and normalized; v0.0.16 remains unsupported.

## Native launcher
Validation publishes `Project_Oracle_v0_0_18` as the root Linux apphost and the desktop launcher targets that executable.


## Repair 2 console ownership

The interactive prompt has exclusive ownership of terminal-body output while the user is entering a command. Idle simulation may advance world/cognition state but performs no console, cursor, LIVE-row, or dynamic-title writes. `LiveConsoleSurface` remains only as a terminal-silent compatibility shell and is not referenced by the interactive input path. `status` prints current state only on explicit request.
