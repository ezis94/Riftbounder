# Rules Coverage

Canonical source: Riftbound Core Rules, updated 2026-03-30.

| Rules | Area | Status | Sprint |
|---|---|---|---|
| 106–110 | Basic zones | Partial | 1 |
| 312 | Priority | Partial: Neutral Open Main Phase | 2 |
| 314–317 | Turn phases | Implemented foundation | 2 |
| 315.3 | Channel Phase | Implemented, including partial channel | 3 |
| 315.4 | Draw Phase | Implemented | 2 |
| 316 | Main Phase | Implemented foundation | 2 |
| 327–340 | Chain / FEPR | Not implemented | — |
| 415 | Ready | Implemented for runes | 3 |
| 430 | Channel | Implemented | 3 |
| 431 | Burn Out | Not implemented | — |
| 454–465 | Combat and scoring | Not implemented | — |
| 480.7 / 481.7 | First-turn adjustments | Implemented | 2 |

| 131 | Costs | Foundation implemented | 4 |
| 135.2.e.5 | Any-domain / Universal Power | Implemented | 4 |
| 160–167 | Rune Pools | Implemented | 4 |
| 163.2 | Basic Rune abilities | Atomic operations | 4 |
| 166 | Pool emptying | Implemented | 4 |
| 357.1 | Resource payment | Foundation implemented | 4 |
| 416 | Recycle | Runes implemented | 4 |
| 429 | Add | Implemented | 4 |

| 327–335 | Chain storage and LIFO resolution primitive | Partial | AG-005 |

| 333.1 | Initial Chain Priority | Implemented | AG-006 |
| 337.1.c.3 | Priority to newest item controller | Implemented | AG-006 |
| 338.1.c | Pass Priority | Implemented | AG-006 |
| 339 | All-player pass detection | Implemented | AG-006 |
| 340 | Resolve newest item and continue/close | Implemented foundation | AG-006 |

| 350–354 | Start playing a spell from Hand | Partial: Hand only | AG-007 |
| 356–357 | Printed Energy/Power cost payment | Implemented foundation | AG-007 |
| 358 | Play legality | Partial: ownership, source, permission, payment | AG-007 |
| 359.1 / 359.3 | Finalized spell Chain item | Implemented foundation | AG-007 |

| 108.2.b | Resolved spells enter owner Trash | Implemented | AG-008 |
| 359.3.c–d | Finalized spell resolution lifecycle | Implemented foundation | AG-008 |
| 359.3.e.10–11 | No-effect and partial spell resolution | Executor contract defined | AG-008 |

| Target selection | Immutable finalized target snapshots | Implemented foundation | AG-009 |
| Target resolution | Recheck kind/location, not Deflect | Implemented foundation | AG-009 |
| Deflect | Adds 1 any-domain Power per chosen Deflect target | Implemented foundation | AG-009 |

| Partial resolution | Resolve instructions using remaining eligible targets | Implemented foundation | AG-010 |
| Invalid targets | Skip invalid targets; spell still completes | Implemented foundation | AG-010 |
