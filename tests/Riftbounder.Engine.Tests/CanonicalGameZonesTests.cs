using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class CanonicalGameZonesTests
{
    [Fact]
    public void NewGame_RegistersCanonicalCardZones()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);

        Assert.Equal(18, game.CardZones.Count);

        Assert.All(
            new[]
            {
                first.MainDeck,
                first.Hand,
                first.Base,
                first.LegendZone,
                first.ChampionZone,
                first.Trash,
                first.Banishment,
                second.MainDeck,
                second.Hand,
                second.Base,
                second.LegendZone,
                second.ChampionZone,
                second.Trash,
                second.Banishment
            },
            zone => Assert.Contains(zone, game.CardZones));

        Assert.All(game.Board.Battlefields, zone => Assert.Contains(zone, game.CardZones));
        Assert.All(game.Board.FacedownZones, zone => Assert.Contains(zone, game.CardZones));
    }

    [Fact]
    public void TransferCard_CanMoveThroughBoardFacedownAndBanishment()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        Card card = CreateUnit(first.Id);

        game.RegisterCard(card, first.Hand);
        game.TransferCard(card.Id, first.Hand, first.Base);
        game.TransferCard(card.Id, first.Base, game.Board.GetBattlefield(1));
        game.TransferCard(
            card.Id,
            game.Board.GetBattlefield(1),
            game.Board.GetFacedownZone(1));
        game.TransferCard(
            card.Id,
            game.Board.GetFacedownZone(1),
            first.Banishment);

        Assert.Same(first.Banishment, game.FindZoneContaining(card.Id));
    }

    [Fact]
    public void PlayerCardZones_UseCanonicalRulesNames()
    {
        Player player = new(PlayerId.New(), "Player");

        Assert.Equal(
            new[]
            {
                ZoneKind.MainDeck,
                ZoneKind.Hand,
                ZoneKind.Base,
                ZoneKind.Legend,
                ZoneKind.Champion,
                ZoneKind.Trash,
                ZoneKind.Banishment
            },
            player.CardZones.Select(zone => zone.Kind).ToArray());
    }

    private static Card CreateUnit(PlayerId ownerId) =>
        Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            ownerId);
}
