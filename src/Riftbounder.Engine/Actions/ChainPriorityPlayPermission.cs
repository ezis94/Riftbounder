using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Priority;

namespace Riftbounder.Engine.Actions;

public sealed class ChainPriorityPlayPermission : IPlayCardPermission
{
    private readonly ChainPriorityManager _priorityManager;

    public ChainPriorityPlayPermission(
        ChainPriorityManager priorityManager)
    {
        ArgumentNullException.ThrowIfNull(priorityManager);
        _priorityManager = priorityManager;
    }

    public bool CanPlay(PlayerId playerId, Card card)
    {
        ArgumentNullException.ThrowIfNull(card);
        return _priorityManager.PriorityPlayerId == playerId;
    }
}
