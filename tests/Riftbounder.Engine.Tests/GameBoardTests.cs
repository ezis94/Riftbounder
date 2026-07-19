using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class GameBoardTests
{
    [Fact]
    public void Constructor_CreatesExactlyTwoSharedBattlefields()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        GameBoard board = new(first, second);

        Assert.Equal(2, board.Battlefields.Count);
        Assert.All(
            board.Battlefields,
            battlefield =>
            {
                Assert.Equal(ZoneKind.Battlefield, battlefield.Kind);
                Assert.True(battlefield.IsShared);
            });
        Assert.NotEqual(board.Battlefields[0].Id, board.Battlefields[1].Id);
    }

    [Fact]
    public void Constructor_UsesEachPlayersCanonicalBase()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        GameBoard board = new(first, second);

        Assert.Same(first.Base, board.GetBase(first.Id));
        Assert.Same(second.Base, board.GetBase(second.Id));
        Assert.Equal(2, board.Bases.Count);
    }

    [Fact]
    public void EachBattlefield_HasOneAssociatedFacedownZone()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        GameBoard board = new(first, second);

        Assert.Equal(2, board.FacedownZones.Count);

        for (int number = 1; number <= 2; number++)
        {
            Zone battlefield = board.GetBattlefield(number);
            Zone facedown = board.GetFacedownZone(battlefield.Id);

            Assert.Equal(ZoneKind.Facedown, facedown.Kind);
            Assert.Equal(1, facedown.MaximumOccupancy);
            Assert.DoesNotContain(facedown, board.Locations);
        }
    }

    [Fact]
    public void Locations_ContainsTwoBasesAndTwoBattlefieldsOnly()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        GameBoard board = new(first, second);

        Assert.Equal(4, board.Locations.Count);
        Assert.Equal(2, board.Locations.Count(zone => zone.Kind == ZoneKind.Base));
        Assert.Equal(2, board.Locations.Count(zone => zone.Kind == ZoneKind.Battlefield));
    }
}
