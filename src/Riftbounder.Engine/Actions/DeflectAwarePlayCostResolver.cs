using Riftbounder.Core.Cards;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Actions;

public sealed class DeflectAwarePlayCostResolver :
    IPlayCostResolver
{
    private readonly BoardState _boardState;

    public DeflectAwarePlayCostResolver(
        BoardState boardState)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        _boardState = boardState;
    }

    public ResourceCost GetTotalCost(
        Card card,
        IReadOnlyList<TargetSnapshot> targets)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(targets);

        List<PowerRequirement> requirements =
            card.Definition.Cost.PowerRequirements.ToList();

        foreach (TargetSnapshot target in targets)
        {
            if (_boardState.TryGet(
                    target.CardId,
                    out BoardCardState? state)
                && state is { HasDeflect: true })
            {
                requirements.Add(
                    PowerRequirement.Any(1));
            }
        }

        return new ResourceCost(
            card.Definition.Cost.Energy,
            requirements);
    }
}
