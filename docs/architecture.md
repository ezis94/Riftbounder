# Architecture

## Current boundary

`Riftbounder.Core` contains stable domain primitives. It does not know how a game is played.

`Riftbounder.Engine` owns state transitions and invariant enforcement. Sprint 1 exposes only card transfer and draw operations.

## Invariants

1. A card instance may exist in at most one registered zone.
2. A zone belongs to exactly one player.
3. A card transfer is atomic: either the card leaves the source and enters the destination, or the source is restored.
4. Drawing from an empty main deck is represented explicitly and does not mutate state.
5. Identifiers are strongly typed to prevent accidental interchange of player and card IDs.

## Deliberate omissions

The following are not part of Sprint 1:

- turn structure;
- runes and energy;
- battlefields and base;
- actions and reactions;
- event or trigger resolution;
- card-definition loading;
- Riftbound card implementations.

These omissions keep the first commit small and testable.
