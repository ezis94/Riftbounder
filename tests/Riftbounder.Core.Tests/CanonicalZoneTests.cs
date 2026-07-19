using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Zones;

namespace Riftbounder.Core.Tests;

public sealed class CanonicalZoneTests
{
    [Fact]
    public void Zones_HaveStableDistinctIdentity()
    {
        PlayerId owner = PlayerId.New();
        Zone hand = new(owner, ZoneKind.Hand, "Hand");
        Zone trash = new(owner, ZoneKind.Trash, "Trash");

        Assert.NotEqual(hand.Id, trash.Id);
        Assert.Equal(owner, hand.OwnerId);
        Assert.False(hand.IsShared);
    }

    [Fact]
    public void SharedZone_HasNoOwner()
    {
        Zone battlefield = new(
            ZoneId.New(),
            ownerId: null,
            ZoneKind.Battlefield,
            "Battlefield");

        Assert.Null(battlefield.OwnerId);
        Assert.True(battlefield.IsShared);
    }

    [Fact]
    public void ZoneKind_UsesRulesTerminology()
    {
        Assert.True(Enum.IsDefined(ZoneKind.Banishment));
        Assert.True(Enum.IsDefined(ZoneKind.Facedown));
        Assert.True(Enum.IsDefined(ZoneKind.Champion));
        Assert.True(Enum.IsDefined(ZoneKind.Legend));
        Assert.False(Enum.GetNames<ZoneKind>().Contains("Hidden"));
    }

    [Fact]
    public void CapacityLimitedZone_RejectsCardBeyondCapacity()
    {
        PlayerId owner = PlayerId.New();
        Zone zone = new(
            owner,
            ZoneKind.Champion,
            "Champion Zone",
            maximumOccupancy: 1);
        Card first = CreateCard(owner, "first");
        Card second = CreateCard(owner, "second");

        zone.AddToTop(first);

        Assert.Throws<InvalidOperationException>(() =>
            zone.AddToTop(second));
    }

    private static Card CreateCard(PlayerId owner, string id) =>
        Card.Create(
            new CardDefinition(
                id,
                id,
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            owner);
}
