# Sprint 2 Hotfix 1

Fixes the second player's first-turn Channel count.

The original implementation incorrectly checked the global turn number. The
second player's first personal turn occurs on global turn 2, so the controller
now tracks turns started per player.

Also adds a regression test confirming that the second player channels only two
runes on later turns.

Suggested commit:

`AG-002-hotfix-1: track per-player first-turn channel bonus`
