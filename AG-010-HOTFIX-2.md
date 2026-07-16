# AG-010 Hotfix 2

Corrects the Deflect regression test fixture.

The test supplied one target, but the default test instruction referenced
target indexes `[0, 1]`. That correctly produced an engine failure for missing
target index 1 before Deflect eligibility could be asserted.

The test now uses target indexes `[0]`.

Suggested commit:

`AG-010-hotfix-2: correct single-target Deflect test fixture`
