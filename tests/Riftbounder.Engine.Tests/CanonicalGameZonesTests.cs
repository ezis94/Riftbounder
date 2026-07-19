using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class CanonicalGameZonesTests
{
    [Fact]
    public void NewGame_RegistersEveryCanonicalCardZone()
    {
        Player first = new(
            PlayerId.New(),
            "First");
        Player second = new(
            PlayerId.New(),
            "Second");
        Game game = new(first, second);

        Assert.Equal(14, game.CardZones.Count);

        Assert.Contains(first.MainDeck, game.CardZones);
        Assert.Contains(first.Hand, game.CardZones);
        Assert.Contains(first.Base, game.CardZones);
        Assert.Contains(first.Hidden, game.CardZones);
        Assert.Contains(first.Trash, game.CardZones);
        Assert.Contains(first.Banish, game.CardZones);

        Assert.Contains(second.MainDeck, game.CardZones);
        Assert.Contains(second.Hand, game.CardZones);
        Assert.Contains(second.Base, game.CardZones);
        Assert.Contains(second.Hidden, game.CardZones);
        Assert.Contains(second.Trash, game.CardZones);
        Assert.Contains(second.Banish, game.CardZones);

        Assert.All(
            game.Board.Battlefields,
            battlefield =>
                Assert.Contains(
                    battlefield,
                    game.CardZones));
    }

    [Fact]
    public void TransferCard_CanMoveThroughBaseBattlefieldHiddenAndBanish()
    {
        Player first = new(
            PlayerId.New(),
            "First");
        Player second = new(
            PlayerId.New(),
            "Second");
        Game game = new(first, second);

        Card card = Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            first.Id);

        game.RegisterCard(card, first.Hand);
        game.TransferCard(
            card.Id,
            first.Hand,
            first.Base);
        game.TransferCard(
            card.Id,
            first.Base,
            game.Board.GetBattlefield(1));
        game.TransferCard(
            card.Id,
            game.Board.GetBattlefield(1),
            first.Hidden);
        game.TransferCard(
            card.Id,
            first.Hidden,
            first.Banish);

        Assert.Same(
            first.Banish,
            game.FindZoneContaining(card.Id));
    }

    [Fact]
    public void PlayerCardZones_AreOwnedByThatPlayer()
    {
        Player player = new(
            PlayerId.New(),
            "Player");

        Assert.All(
            player.CardZones,
            zone => Assert.Equal(
                player.Id,
                zone.OwnerId));

        Assert.Equal(
            [
                ZoneKind.MainDeck,
                ZoneKind.Hand,
                ZoneKind.Base,
                ZoneKind.Hidden,
                ZoneKind.Trash,
                ZoneKind.Banish
            ],
            player.CardZones
                .Select(zone => zone.Kind)
                .ToArray());
    }
}
