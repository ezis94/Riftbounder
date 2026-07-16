using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Events;

public sealed record SpellResolutionStartedEvent(
    ChainItemId ChainItemId,
    PlayerId ControllerId,
    Card Card,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record SpellResolutionCompletedEvent(
    ChainItemId ChainItemId,
    PlayerId ControllerId,
    Card Card,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record SpellResolutionFailedEvent(
    ChainItemId ChainItemId,
    PlayerId ControllerId,
    Card Card,
    string Reason,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);
