
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
using Riftbounder.Engine.Resources;
namespace Riftbounder.Engine.Tests;

public sealed class RunePoolTests
{
    [Fact] public void MatchingDomainPayment_Succeeds() { var p = new RunePool(); var m = PowerType.ForDomain(Domain.Mind); p.AddEnergy(2); p.AddPower(m, 1); var c = new ResourceCost(2, [PowerRequirement.Of(Domain.Mind, 1)]); var pay = new ResourcePayment(2, [new PowerSpend(m, 1)]); Assert.True(p.CanPay(c, pay)); p.Pay(c, pay); Assert.Equal(0, p.Energy); Assert.Equal(0, p.GetPower(m)); }
    [Fact] public void WrongDomainPayment_Fails() { var p = new RunePool(); var o = PowerType.ForDomain(Domain.Order); p.AddPower(o, 1); Assert.False(p.CanPay(new ResourceCost(0, [PowerRequirement.Of(Domain.Mind, 1)]), new ResourcePayment(0, [new PowerSpend(o, 1)]))); }
    [Fact] public void UniversalPower_PaysSpecificRequirement() { var p = new RunePool(); p.AddPower(PowerType.Universal, 1); Assert.True(p.CanPay(new ResourceCost(0, [PowerRequirement.Of(Domain.Order, 1)]), new ResourcePayment(0, [new PowerSpend(PowerType.Universal, 1)]))); }
    [Fact] public void DomainPower_PaysAnyRequirement() { var p = new RunePool(); var c = PowerType.ForDomain(Domain.Calm); p.AddPower(c, 1); Assert.True(p.CanPay(new ResourceCost(0, [PowerRequirement.Any(1)]), new ResourcePayment(0, [new PowerSpend(c, 1)]))); }
    [Fact] public void Empty_RemovesAllResources() { var p = new RunePool(); p.AddEnergy(2); p.AddPower(PowerType.ForDomain(Domain.Chaos), 1); Assert.True(p.Empty()); Assert.Equal(0, p.Energy); Assert.Empty(p.Power); }
}
