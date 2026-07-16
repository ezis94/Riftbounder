# ADR 0001: Chain resolution is transactional

- Status: Accepted
- Date: 2026-07-16

## Context

A Chain item may fail to resolve because the engine encounters an invalid or
unsupported operation. Removing that item before its resolution succeeds would
lose the exact game state needed for diagnosis or recovery.

## Decision

`ChainResolver.ResolveTop()` peeks at the top item, asks an
`IChainItemResolver` to resolve it, and removes it only when the returned result
is successful.

`ResolveAll()` stops on the first failed item and leaves that item, plus every
item below it, on the Chain.

## Consequences

- Successful resolution is atomic from the Chain container's perspective.
- Failures are inspectable and retryable.
- Rules-level outcomes that successfully resolve but have no effect must still
  return success. `Failed` is reserved for engine-level inability to complete
  resolution.
