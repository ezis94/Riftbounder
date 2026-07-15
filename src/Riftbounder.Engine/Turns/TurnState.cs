using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Turns;

public sealed class TurnState
{
    internal TurnState()
    {
    }

    public int TurnNumber { get; internal set; }

    public PlayerId? TurnPlayerId { get; internal set; }

    public PlayerId? PriorityPlayerId { get; internal set; }

    public TurnPhase Phase { get; internal set; } = TurnPhase.NotStarted;

    public bool HasStarted => Phase is not TurnPhase.NotStarted;
}
