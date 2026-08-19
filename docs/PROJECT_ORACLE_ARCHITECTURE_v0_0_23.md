# Project Oracle Architecture v0.0.23

## Brain Slice 6: Cosmic Choice Architecture

v0.0.23 extends the v0.0.22 Soar cognition pipeline with two new first-class layers.

### 1. Comparative religious semantic memory

`YalaReligiousKnowledgeCatalog` contains 30 traditions/families and 92 attributed ideas in the initial v0.0.23 catalogue. Each tradition stores a key, name, family, source-basis description, truth status, topic summaries, and links to related cosmic-choice keys.

These records enter Soar semantic memory as attributed comparative knowledge. They do not enter Yala's autobiographical memory and do not become world facts.

### 2. Concrete cosmic choice pool

`YalaCosmicChoiceCatalog` exposes 71 possibilities across nine domains:

- beings
- cosmic structure
- life
- moral order
- mortal destiny
- knowledge
- governance
- renewal
- meta-choice

Each choice stores a concrete action, meaning, status, tradition inspirations, prerequisite flags, and affinities to Yala's curiosity, caution, authority, companionship, and comfort drives.

At an eligible autonomous decision point the simulation writes the available choices and transparent drive-derived scores onto Soar's input link. Soar proposes a generic `enact-cosmic-choice` operator for each option and returns the selected `choice-key` to world law.

The world-law resolver persists committed choices in `CosmicState.EstablishedChoices` and action memory. Non-committing choices such as remaining alone do not invent a cosmic law. `invent-another-way` opens a high-priority `invent-new-cosmology` goal and a `cosmic-invention` knowledge gap.

### No Gaia railroad

The v0.0.22 autonomous `create-gaia` proposal is removed. The direct world-law resolver for Gaia remains for compatibility and testing, but autonomous Yala sees Gaia through the same comparative choice mechanism as other eligible possibilities.

### Bounded agency

The new in-world action `enact-cosmic-choice` is allowed. Host shell, host process execution, arbitrary file mutation, network access, source modification, and hidden Oracle truth remain unavailable to Yala.

### Fresh state line

- save schema: 5
- default save: `save_v5.json`
- Soar memory: `yala_soar_v0_0_23/semantic.sqlite` and `episodic.sqlite`

Accepted v0.0.22 save/memory data remains separate.

## Cognitive continuity and appraisal expansion

Brain Slice 6 now persists concerns, appraisals, hypotheses, entity models, and reflections alongside questions and goals. Contact is evaluated for personal salience, threat, opportunity, uncertainty, and relevance before future autonomous selection. Critical concerns such as possible confinement or an extraordinary demand for divine authority can therefore remain active across later decisions instead of being displaced by low-value lexical curiosity.

`YalaFoundationalLanguage` supplies the inherited-language floor. Ordinary concepts do not become definition gaps merely because a small explicit lexicon misses a token. Marked, invented, technical, or contextually unusual terms can still produce genuine definition questions.

## Cognitive inheritance architecture

`OracleMindInheritanceManifest` separates reusable mind architecture from identity and autobiography. Future creators may seed selected knowledge, procedural knowledge, dispositions, capabilities, and lineage into a newly instantiated independent mind. `OracleMindInheritancePolicy` enforces the creation ceiling: the child's granted world authority must be strictly below the creator's authority.

Monad's possible future Primordial Mind and Sophia's possible root-descendant-mind role remain roadmap research directions rather than current simulated facts.

## Desktop application architecture

`ProjectOracle.Desktop` is the normal graphical front end. It references `ProjectOracle.Core` directly and does not replace the simulation engine. The desktop surface provides conversation, world state, Yala Mind inspection, Minds, Memory, Cosmology, History, and Debug views. The console project remains a developer/debug fallback.

The validated Linux publish copies the graphical apphost to the project root as `Project_Oracle_v0_0_23`, and the desktop launcher runs that executable with `Terminal=false`.


## Emergent law foundation

`ProjectOracle.Cognition.Emergence` introduces a law-engine boundary separate from mind cognition. `OracleEmergentLawState` persists established laws and laboratory history independently from Yala's beliefs and from the comparative cosmology catalogue.

`Rule30Laboratory` is the first deterministic local-rule test bed. It demonstrates repeated local update rules but carries `LaboratoryOnly = true`, so availability in code cannot silently establish it as a world law. `OracleLawAuthorityPolicy` is the enforcement hook for future law-establishment actions.

The intended long-term split is:

```text
Oracle Mind Architecture -> beings choose goals/actions/laws
Oracle Emergent Law Engine -> established rules evolve world state
Project Oracle world-law resolver -> validates authority and commits consequences
```
