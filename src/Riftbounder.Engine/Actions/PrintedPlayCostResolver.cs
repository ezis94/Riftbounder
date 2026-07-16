using Riftbounder.Core.Cards;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;

namespace Riftbounder.Engine.Actions;

public sealed class PrintedPlayCostResolver : IPlayCostResolver
{
    public ResourceCost GetTotalCost(
        Card card,
        IReadOnlyList<TargetSnapshot> targets)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(targets);
        return card.Definition.Cost;
    }
}
