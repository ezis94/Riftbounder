
namespace Riftbounder.Core.Resources;
public readonly record struct PowerSpend
{
    public PowerSpend(PowerType powerType, int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        PowerType = powerType; Amount = amount;
    }
    public PowerType PowerType { get; }
    public int Amount { get; }
}
