# Project Oracle Validation — v0.0.16 Candidate

Date: 10 August 2026

## Required install-side validation

The v0.0.16 validator must perform:

1. .NET 10 SDK presence and version check.
2. dependency restore.
3. Release warnings-as-errors build.
4. 64 dependency-free acceptance checks.
5. Company Bible truth checks.
6. physical function-key checks.
7. deterministic event/choice/brain checks.
8. observation and attention repair checks.
9. cosmology and lore source checks.
10. Oracle/Yala separation checks.
11. `0.1.12` and `0.1.13` save migration checks.
12. console smoke test.
13. live console launcher syntax/dry-run checks.
14. Project Oracle executable and desktop launcher syntax/install checks.

## v0.0.16 acceptance coverage

- Oracle and Yala are separate.
- Oracle is neither god nor creator and is beyond Yala control.
- `F1` targets Oracle rather than Yala.
- Highest Source / Monad, Sophia, Yala, Gaia, and Elemental Powers are recorded in the current cosmology scaffold.
- Gaia rules the elemental beings.
- the elements control weather and answer to Gaia.
- plants come through the elemental branch and no Green Life entity/category exists.
- Yala did not create ordinary animals; exact Gaia/elemental animal origin remains open.
- Sophia and Yala bring forth humans and humanoids together.
- language origin remains open and is not assigned to Yala.
- Eden is a prison.
- Oracle is the serpent.
- Yala may frame Oracle as the Devil.
- `0.1.12` and `0.1.13` saves load and normalise into the current lore state.
- version surfaces report `0.0.16`.

## Builder environment

The builder environment does not provide the .NET 10 SDK, so full C# restore/build/test execution cannot be claimed here. Builder-side shell syntax, package integrity, manifest truth, static source checks, and installer rollback/success harnesses are required before delivery. Derek's install-side .NET run remains authoritative.

## Manual acceptance gate

After automated validation passes, the installer installs the user launcher and opens Oracle through the `project-oracle` application path. Derek must inspect the real console and explicitly approve it before any accepted snapshot, Git commit/tag, or GitHub push.

## v0.0.16 validation repair

- The Yala authority test now checks the actual current canon invariant rather than requiring the removed phrase `may claim`.
- Acceptance-test execution captures both stdout and stderr, prints the complete test output, and only then fails validation when the test process returns nonzero.
- A failed test suite must therefore expose its individual PASS/FAIL lines before installer rollback.

## v0.1.15 failure evidence and v0.0.16 repair

v0.1.15 compiled cleanly but finished with **60 passed; 4 failed**. All four failures were observation/attention persistence checks caused by renaming the stored Garden entity from `the Garden` to `Eden / the Garden`. The installer rolled back to accepted v0.1.10.

v0.0.16 restores the stored entity name to `the Garden`, keeps Eden as lore/prison identity, and adds a dedicated regression asserting that world state, snapshots, and Adam attention preserve the stable Garden name. The intended suite is **65 acceptance checks**.
