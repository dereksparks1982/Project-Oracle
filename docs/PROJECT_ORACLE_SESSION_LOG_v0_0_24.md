# Project Oracle Session Log v0.0.24

## Accepted base

v0.0.23 was accepted after 165/165 tests, desktop manual inspection, accepted snapshot, commit `a2e38bc7ba68c1540b9e51dc3038369e055c0c4f`, annotated tag `v0.0.23`, and verified GitHub push.

## v0.0.24 requested direction

Derek requested a larger next brain slice centered on deliberation and planning, plus a way to export sessions for later analysis. The JSON export was explicitly reframed as a cognitive flight recorder so later analysis can inspect what was happening inside Yala's cognition rather than only reading her visible reply.

GUI observations from the accepted desktop app also produced these requirements:

- conversation must follow the newest message automatically;
- intentional upward scrolling must not be overridden;
- provide `JUMP TO LATEST`;
- the app must auto-detect the monitor working area/scaling/aspect ratio rather than opening larger than a 1366x768 display;
- remember/clamp window placement and provide `FIT TO SCREEN`;
- use the approved Oracle eye/emblem as app/launcher/navigation branding;
- no startup splash screen.

The build continues the fresh-save policy with schema 6 and `yala_soar_v0_0_24`.

## Manual-test and evidence-routing closeout additions

The v0.0.24 acceptance run reached 179/180. The remaining failure showed that a speaker answer could be routed only to the literal subject of the spoken question and miss the higher-level active investigation that caused the question. The candidate now preserves the answer as attributed evidence in the best literal investigation and the active motivating plan/investigation, without promoting the answer to proof.

The candidate also begins the living cognitive-manual system with a Yala Manual and shared Oracle Mind Architecture Manual. The Yala manual contains Derek's `substrate of existence` test statement and the approved thorough post-PASS manual interrogation battery. Future major independent cognitive entities receive their own manuals when implemented.
