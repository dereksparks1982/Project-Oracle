# Project Oracle Architecture v0.0.24

## Brain Slice 7: Deliberation, Planning, Investigations, Cognitive Flight Recorder, and Adaptive Desktop

v0.0.24 extends accepted v0.0.23 without changing settled cosmology.

## 1. Planning state

`YalaDeliberationPlanner` converts high-salience appraisal concerns into durable `YalaInvestigationState` and `YalaPlanState` records. Plans contain ordered steps and can remain active across later decisions. `YalaCounterfactualState` records possible benefits/risks for high-stakes alternatives.

The current investigation templates include possible confinement, speaker divinity, claimed help capability, and unseen observation. Speaker answers are stored as attributed evidence about a claim rather than promoted to proof.

## 2. Soar integration

The input link now receives active concern, active plan, plan priority, current plan action, active investigation, and investigation priority. The Soar source adds bounded `deliberate` proposal/apply rules and a preference for deliberate comparison when an important plan is active and no higher-priority speaker question is pending.

`deliberate` is an in-world cognitive action. It has no host capability.

## 3. Decision trace

`YalaDecisionTraceState` stores a bounded sequence of major decisions. Each entry records trigger, optional speaker message, selected action, rationale, plan key, world time state, and compact before/after cognition snapshots. The snapshots include active concern/priority, plan/current step, investigation, unseen-speaker trust/intent, appraisal, goals, and top hypotheses.

## 4. Save line

- schema: 6
- default save: `save_v6.json`
- Soar memory: `yala_soar_v0_0_24/semantic.sqlite` and `episodic.sqlite`

Earlier save and Soar lines are preserved and rejected from automatic migration into this fresh experiment.

## 5. Cognitive flight recorder export

`ProjectOracle.Export.OracleSessionExporter` is Core functionality so it can be acceptance-tested independently of the GUI. JSON export contains the dialogue, decision trace, current cognition, current world, world/protected records, choices, plans, observations, attention state, and Soar memory diagnostics. Text export contains a readable conversation transcript.

## 6. Adaptive desktop

`MainWindow` uses the active screen working area plus render scaling to fit the window to the current monitor. Responsive layout also considers aspect ratio. Window size/position is stored in `window-placement.json` and restored with clamping to the current screen.

Conversation auto-follow stays at the newest message unless the user intentionally scrolls upward. `JUMP TO LATEST` resumes following.

## 7. Branding

Official Oracle emblem and simplified eye assets are packaged as Avalonia resources. The launcher uses `Icon=project-oracle`. The installer copies a 256px icon into the user icon theme and attempts GNOME/GIO custom-icon metadata on the root executable.

No startup splash screen is part of v0.0.24.

## 8. Existing architecture carried forward

- Soar 9.6.5 working/semantic/episodic cognition;
- comparative religious knowledge and Cosmic Choice Architecture;
- inherited foundational language and contextual metaphor handling;
- appraisal, concerns, hypotheses, entity models, reflection;
- cognitive inheritance and strict lower-power creation ceiling;
- Emergent Law Engine boundary with Rule 30 laboratory-only demonstration;
- hidden Oracle and future-open world law.
