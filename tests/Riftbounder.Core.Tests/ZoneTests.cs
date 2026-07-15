using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;

namespace Riftbounder.Core.Tests;

public sealed class ZoneTests
{
    [Fact]
    public void AddToTop_AddsCardAsTopCard()
    {
        PlayerId owner = PlayerId.New();
        Zone zone = new(owner, ZoneKind.MainDeck, "Deck");
        Card first = Card.Create("first", owner);
        Card second = Card.Create("second", owner);

        zone.AddToTop(first);
        zone.AddToTop(second);

        Assert.Equal(2, zone.Count);
        Assert.Same(second, zone.PeekTop());
    }

    [Fact]
    public void AddToBottom_PreservesExistingTopCard()
    {
        PlayerId owner = PlayerId.New();
        Zone zone = new(owner, ZoneKind.MainDeck, "Deck");
        Card top = Card.Create("top", owner);
        Card bottom = Card.Create("bottom", owner);

        zone.AddToTop(top);
        zone.AddToBottom(bottom);

        Assert.Same(top, zone.PeekTop());
        Assert.Equal(bottom, zone.Cards[0]);
    }

    [Fact]
    public void AddToTop_RejectsDuplicateCardInstance()
    {
        PlayerId owner = PlayerId.New();
        Zone zone = new(owner, ZoneKind.Hand, "Hand");
        Card card = Card.Create("card", owner);
        zone.AddToTop(card);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => zone.AddToTop(card));

        Assert.Contains(card.Id.ToString(), error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Remove_MissingCard_ReturnsFalseWithoutMutation()
    {
        PlayerId owner = PlayerId.New();
        Zone zone = new(owner, ZoneKind.Hand, "Hand");
        Card existing = Card.Create("existing", owner);
        zone.AddToTop(existing);

        bool removed = zone.Remove(CardId.New(), out Card? card);

        Assert.False(removed);
        Assert.Null(card);
        Assert.Single(zone.Cards);
    }
}
