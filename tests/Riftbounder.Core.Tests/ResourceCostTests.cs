
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
namespace Riftbounder.Core.Tests;

public sealed class ResourceCostTests
{
    [Fact] public void Constructor_RejectsNegativeEnergy() => Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceCost(-1));
    [Fact] public void AnyPowerRequirement_HasNoDomain() { var r = PowerRequirement.Any(2); Assert.True(r.AcceptsAnyDomain); Assert.Null(r.Domain); }
    [Fact] public void SpecificPowerRequirement_StoresDomain() { var r = PowerRequirement.Of(Domain.Mind, 1); Assert.Equal(Domain.Mind, r.Domain); }
}
