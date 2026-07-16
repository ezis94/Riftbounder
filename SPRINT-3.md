# Sprint 3 — Rune Deck and Channel Resolution

## Commit

`AG-003: implement rune zones and channel resolution`

## Implemented

- Strongly typed `RuneId`.
- Six Riftbound domains.
- Rune ready/exhausted state.
- Separate Rune Deck and Base rune zones.
- Rune registration and atomic rune transfers.
- Channel action:
  - takes runes from the top of the Rune Deck;
  - puts them into the Base;
  - enters them ready unless an effect specifies exhausted;
  - channels as many as possible when fewer remain.
- Awaken Phase readies exhausted runes.
- Channel Phase now resolves instead of emitting only a request.
- Typed `RuneChanneledEvent` and `RuneReadiedEvent`.
- Tests for normal, partial, and exhausted channeling.

## Rules mapped

- 108.5 — Rune Deck Zone
- 315.1 — Awaken Phase
- 315.3 — Channel Phase
- 415 — Ready
- 430 — Channel
- 480.7 / 481.7 — second player's first-turn extra Rune

Canonical rules:
`https://www.riftbound.one/rules/riftbound-core-rules.pdf`

## Deliberate boundary

Sprint 3 does not yet implement Rune Pools, Energy, Power, rune recycling as
Power costs, or Burn Out. Those are planned for Sprint 4.
