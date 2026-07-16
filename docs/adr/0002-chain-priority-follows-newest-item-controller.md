# ADR 0002: Chain priority follows the newest item controller

- Status: Accepted
- Date: 2026-07-16

## Context

Riftbound's FEPR process assigns Priority to the controller of the newest
finalized Chain item. Players then pass in turn order. When every player passes
without adding an item, only the newest item resolves. If items remain, the
controller of the new newest item receives Priority.

An earlier project outline described generic consecutive passes on an empty
Chain. That would conflate Chain Priority with the surrounding Open State and
is not faithful to rules 335–340.

## Decision

`ChainPriorityManager` is active only while the Chain contains an item.

- `Begin()` assigns Priority to the newest item's controller.
- A pass transfers Priority to the next player in turn order.
- Adding a new item clears pass history and assigns Priority to that item's
  controller.
- When all players pass, exactly one item resolves.
- If another item remains, its controller receives Priority.
- If the Chain empties, Priority is cleared and an `IChainFlowObserver` is
  notified so the surrounding turn or Showdown flow can continue.

## Consequences

- Priority logic remains separate from Main Phase progression and Showdown
  Focus.
- Multiplayer pass order is supported without special two-player logic.
- AG-007 can add played actions as clients of this manager without changing
  FEPR pass semantics.
