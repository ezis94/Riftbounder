using Riftbounder.Core.Cards;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;

namespace Riftbounder.Engine.Actions;

public interface IPlayCostResolver
{
    ResourceCost GetTotalCost(
        Card card,
        IReadOnlyList<TargetSnapshot> targets);
}
