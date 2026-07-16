
using Riftbounder.Core.Runes;
namespace Riftbounder.Core.Resources;
public readonly record struct PowerType
{
    private PowerType(Domain? domain) => Domain = domain;
    public Domain? Domain { get; }
    public bool IsUniversal => Domain is null;
    public static PowerType ForDomain(Domain domain) => new(domain);
    public static PowerType Universal { get; } = new(null);
    public override string ToString() => IsUniversal ? "Universal" : Domain!.Value.ToString();
}
