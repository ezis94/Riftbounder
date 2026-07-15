# Sprint 2 — Turn Flow and Event Journal

## Commit

`AG-002: implement turn phases and typed event journal`

## Implemented

- Typed, append-only event journal.
- Official turn phase sequence:
  - Awaken
  - Beginning
  - Channel
  - Draw
  - Main
  - Ending
- Turn-player priority during Neutral Open Main Phase.
- First player skips the first Draw Phase draw.
- Second player requests three runes during their first Channel Phase.
- Actual Main Deck draw resolution through the Sprint 1 `Game` object.
- Explicit empty-deck draw event.
- Turn rotation between two players.

## Deliberate boundary

Rune cards and rune zones are not implemented yet. Sprint 2 emits a
`ChannelRunesRequestedEvent`; Sprint 3 will resolve that request against a Rune Deck.

The Chain, Reactions, FEPR, Showdowns, and Combat are also intentionally deferred.

## Rules mapped

- 312 — Priority
- 314–317 — Turn phases
- 315.3 — Channel Phase
- 315.4 — Draw Phase
- 316 — Main Phase
- 480.7 / 481.7 — first-turn adjustments

Canonical rules:
`https://www.riftbound.one/rules/riftbound-core-rules.pdf`
