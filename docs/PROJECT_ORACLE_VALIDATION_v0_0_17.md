# Project Oracle Validation v0.0.17

## Required automated gates

- .NET 10 SDK available and project builds with warnings as errors.
- Soar 9.6.5 Linux x86-64 native dependencies resolve.
- Yala Soar productions load.
- Soar receives input and returns a legal `yala-action` output command.
- Fresh world contains no in-world Oracle entity/direct-call target.
- Fresh Void contains no Garden, Adam, Adam spark, living kinds, Gaia, Time, Terra, Aether, Sol, Thalassa, or Luna.
- Monad -> Wisdom -> Yala genealogy is exact.
- Gaia is the creator of in-world Time.
- Pre-Time runtime does not advance world milliseconds.
- Yala does not know Oracle exists.
- Direct Yala contact reports an unknown/unplaced source.
- Serpent wording says Oracle manifested in the form of a clever serpent; no fixed-serpent identity wording.
- v0.0.17 uses a new `save_v2.json` default and rejects v0.0.16 saves instead of migrating them.
- Yala cognition survives save/restore.
- root `Project_Oracle_v0_0_17` is generated and is an ELF executable.
- desktop launcher points at the root executable.
- current README and authority docs agree with v0.0.17 canon.

## Acceptance gate

Automated FINAL PASS must be followed by launching the real application for Derek's manual inspection. No snapshot, commit, tag, or push occurs until Derek explicitly says PASS.

## Builder-environment evidence

The packaging environment did not contain the .NET 10 SDK, so C# restore/build/publish and the complete C# acceptance executable are intentionally target-machine gates in `scripts/validate.sh` rather than falsely reported as having run here.

The supplied Soar 9.6.5 Linux runtime was exercised directly through its supplied SML interface during packaging. The Yala production set loaded successfully; fresh-Void runs selected only `observe`, `reflect`, `wait`, or `create-gaia`; post-Gaia runs selected only `observe`, `reflect`, `wait`, or `command-gaia-time`; and all four direct-contact intents returned the expected `yala-action` response command. This is evidence for the Soar production slice, not a substitute for the required .NET 10 install-side validation.
