using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Game;
using Riftbounder.Engine.Results;

namespace Riftbounder.Engine.Tests;

public sealed class GameTests
{
    [Fact]
    public void Constructor_RejectsDuplicatePlayerIds()
    {
        PlayerId id = PlayerId.New();
        Player first = new(id, "First");
        Player second = new(id, "Second");

        Assert.Throws<ArgumentException>(() => new Game(first, second));
    }

    [Fact]
    public void RegisterCard_PlacesCardInExactlyOneZone()
    {
        (Game game, Player first, _) = CreateGame();
        Card card = Card.Create("test-card", first.Id);

        game.RegisterCard(card, first.MainDeck);

        Assert.Same(first.MainDeck, game.FindZoneContaining(card.Id));
        Assert.Contains(card, first.MainDeck.Cards);
        Assert.DoesNotContain(card, first.Hand.Cards);
    }

    [Fact]
    public void RegisterCard_RejectsCardAlreadyRegistered()
    {
        (Game game, Player first, _) = CreateGame();
        Card card = Card.Create("test-card", first.Id);
        game.RegisterCard(card, first.MainDeck);

        Assert.Throws<InvalidOperationException>(() => game.RegisterCard(card, first.Hand));
    }

    [Fact]
    public void TransferCard_MovesCardAtomicallyBetweenZones()
    {
        (Game game, Player first, _) = CreateGame();
        Card card = Card.Create("test-card", first.Id);
        game.RegisterCard(card, first.MainDeck);

        game.TransferCard(card.Id, first.MainDeck, first.Hand);

        Assert.Empty(first.MainDeck.Cards);
        Assert.Single(first.Hand.Cards);
        Assert.Same(first.Hand, game.FindZoneContaining(card.Id));
    }

    [Fact]
    public void TransferCard_RejectsIncorrectSourceWithoutMutation()
    {
        (Game game, Player first, _) = CreateGame();
        Card card = Card.Create("test-card", first.Id);
        game.RegisterCard(card, first.MainDeck);

        Assert.Throws<InvalidOperationException>(() =>
            game.TransferCard(card.Id, first.Hand, first.Trash));

        Assert.Single(first.MainDeck.Cards);
        Assert.Empty(first.Hand.Cards);
        Assert.Empty(first.Trash.Cards);
    }

    [Fact]
    public void DrawCard_MovesTopCardFromDeckToHand()
    {
        (Game game, Player first, _) = CreateGame();
        Card bottom = Card.Create("bottom", first.Id);
        Card top = Card.Create("top", first.Id);
        game.RegisterCard(bottom, first.MainDeck);
        game.RegisterCard(top, first.MainDeck);

        DrawResult result = game.DrawCard(first.Id);

        Assert.True(result.Succeeded);
        Assert.Same(top, result.Card);
        Assert.Single(first.MainDeck.Cards);
        Assert.Same(bottom, first.MainDeck.PeekTop());
        Assert.Single(first.Hand.Cards);
        Assert.Same(top, first.Hand.PeekTop());
    }

    [Fact]
    public void DrawCard_EmptyDeck_ReturnsExplicitFailureWithoutMutation()
    {
        (Game game, Player first, _) = CreateGame();

        DrawResult result = game.DrawCard(first.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Card);
        Assert.Empty(first.MainDeck.Cards);
        Assert.Empty(first.Hand.Cards);
    }

    private static (Game Game, Player First, Player Second) CreateGame()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        return (new Game(first, second), first, second);
    }
}
