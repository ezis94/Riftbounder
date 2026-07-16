using Riftbounder.Core.Resources;

namespace Riftbounder.Engine.Targets;

public static class TargetCostModifier
{
    public static ResourceCost Apply(
        ResourceCost printedCost,
        IReadOnlyList<TargetSelectionResult> selections)
    {
        ArgumentNullException.ThrowIfNull(printedCost);
        ArgumentNullException.ThrowIfNull(selections);

        List<PowerRequirement> requirements =
            printedCost.PowerRequirements.ToList();

        foreach (TargetSelectionResult selection in selections)
        {
            if (!selection.Succeeded)
            {
                throw new InvalidOperationException(
                    "Cannot apply target costs from a failed selection.");
            }

            if (selection.AddedCost is PowerRequirement added)
            {
                requirements.Add(added);
            }
        }

        return new ResourceCost(
            printedCost.Energy,
            requirements);
    }
}
