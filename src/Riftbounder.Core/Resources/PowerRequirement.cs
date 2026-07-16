
using Riftbounder.Core.Runes;
namespace Riftbounder.Core.Resources;
public readonly record struct PowerRequirement
{
    public PowerRequirement(int amount, Domain? domain)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount; Domain = domain;
    }
    public int Amount { get; }
    public Domain? Domain { get; }
    public bool AcceptsAnyDomain => Domain is null;
    public static PowerRequirement Any(int amount) => new(amount, null);
    public static PowerRequirement Of(Domain domain, int amount) => new(amount, domain);
}
