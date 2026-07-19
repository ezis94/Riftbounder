using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;

namespace Riftbounder.Core.Tests;

public sealed class CanonicalZoneTests
{
    [Fact]
    public void Zones_HaveStableDistinctIdentity()
    {
        PlayerId owner = PlayerId.New();
        Zone hand = new(
            owner,
            ZoneKind.Hand,
            "Hand");
        Zone trash = new(
            owner,
            ZoneKind.Trash,
            "Trash");

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
    public void ZoneKind_IncludesCanonicalNonBoardZones()
    {
        Assert.True(
            Enum.IsDefined(ZoneKind.Banish));
        Assert.True(
            Enum.IsDefined(ZoneKind.Hidden));
        Assert.True(
            Enum.IsDefined(ZoneKind.Chain));
    }
}
