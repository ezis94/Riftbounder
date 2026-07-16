
namespace Riftbounder.Core.Resources;

public sealed class ResourceCost
{
    public ResourceCost(int energy, IReadOnlyList<PowerRequirement>? powerRequirements = null)
    {
        if (energy < 0) throw new ArgumentOutOfRangeException(nameof(energy));
        Energy = energy;
        PowerRequirements = powerRequirements?.ToArray() ?? Array.Empty<PowerRequirement>();
    }
    public int Energy { get; }
    public IReadOnlyList<PowerRequirement> PowerRequirements { get; }
    public static ResourceCost EnergyOnly(int energy) => new(energy);
}
