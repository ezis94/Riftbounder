
using Riftbounder.Core.Resources;
namespace Riftbounder.Engine.Resources;

public sealed class RunePool
{
    private readonly Dictionary<PowerType, int> _power = [];
    public int Energy { get; private set; }
    public IReadOnlyDictionary<PowerType, int> Power => _power;
    public void AddEnergy(int amount) { if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount)); Energy += amount; }
    public void AddPower(PowerType type, int amount) { if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount)); _power[type] = GetPower(type) + amount; }
    public int GetPower(PowerType type) => _power.GetValueOrDefault(type);
    public bool CanPay(ResourceCost cost, ResourcePayment payment)
    {
        ArgumentNullException.ThrowIfNull(cost); ArgumentNullException.ThrowIfNull(payment);
        if (payment.Energy != cost.Energy || Energy < payment.Energy) return false;
        var pool = new Dictionary<PowerType, int>(_power);
        var reqs = cost.PowerRequirements
            .SelectMany(requirement =>
                Enumerable.Repeat(requirement.Domain, requirement.Amount))
            .OrderBy(domain => domain is null ? 1 : 0)
            .ToList();
        foreach (var spend in payment.PowerSpends)
        {
            if (spend.Amount == 0) continue;
            if (!pool.TryGetValue(spend.PowerType, out var available) || available < spend.Amount) return false;
            pool[spend.PowerType] = available - spend.Amount;
            for (int i = 0; i < spend.Amount; i++)
            {
                int idx = reqs.FindIndex(required => required is null || spend.PowerType.IsUniversal || spend.PowerType.Domain == required);
                if (idx < 0) return false;
                reqs.RemoveAt(idx);
            }
        }
        return reqs.Count == 0;
    }
    public void Pay(ResourceCost cost, ResourcePayment payment)
    {
        if (!CanPay(cost, payment)) throw new InvalidOperationException("The supplied resource payment does not legally pay the cost.");
        Energy -= payment.Energy;
        foreach (var spend in payment.PowerSpends) if (spend.Amount > 0) _power[spend.PowerType] -= spend.Amount;
    }
    public void Refund(ResourcePayment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        Energy += payment.Energy;

        foreach (PowerSpend spend in payment.PowerSpends)
        {
            if (spend.Amount == 0)
            {
                continue;
            }

            _power[spend.PowerType] = GetPower(spend.PowerType) + spend.Amount;
        }
    }

    public bool Empty()
    {
        bool changed = Energy > 0 || _power.Values.Any(v => v > 0); Energy = 0; _power.Clear(); return changed;
    }
}
