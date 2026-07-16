
namespace Riftbounder.Core.Resources;
public sealed class ResourcePayment
{
    public ResourcePayment(int energy, IReadOnlyList<PowerSpend>? powerSpends = null)
    {
        if (energy < 0) throw new ArgumentOutOfRangeException(nameof(energy));
        Energy = energy;
        PowerSpends = powerSpends?.ToArray() ?? Array.Empty<PowerSpend>();
    }
    public int Energy { get; }
    public IReadOnlyList<PowerSpend> PowerSpends { get; }
}
