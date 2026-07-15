using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Turns;

namespace Riftbounder.Engine.Events;

public sealed record TurnStartedEvent(
    int TurnNumber,
    PlayerId TurnPlayerId,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record TurnPhaseChangedEvent(
    int TurnNumber,
    PlayerId TurnPlayerId,
    TurnPhase PreviousPhase,
    TurnPhase CurrentPhase,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record PriorityChangedEvent(
    PlayerId? PreviousPlayerId,
    PlayerId? CurrentPlayerId,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record ChannelRunesRequestedEvent(
    int TurnNumber,
    PlayerId PlayerId,
    int RuneCount,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record CardDrawnEvent(
    int TurnNumber,
    PlayerId PlayerId,
    Card Card,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);

public sealed record EmptyDeckDrawAttemptedEvent(
    int TurnNumber,
    PlayerId PlayerId,
    DateTimeOffset OccurredAt)
    : GameEvent(OccurredAt);
