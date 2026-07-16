using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Events;

public sealed record SpellCardDrawnEvent(
    ChainItemId ChainItemId,
    PlayerId PlayerId,
    Card Card,
    int DrawIndex,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record SpellEmptyDeckDrawAttemptedEvent(
    ChainItemId ChainItemId,
    PlayerId PlayerId,
    int DrawIndex,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);
