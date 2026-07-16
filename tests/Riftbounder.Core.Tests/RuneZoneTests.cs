using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;

namespace Riftbounder.Core.Tests;

public sealed class RuneZoneTests
{
    [Fact]
    public void PeekTop_ReturnsLastAddedRune()
    {
        PlayerId ownerId = PlayerId.New();
        RuneZone zone = new(ownerId, "Rune Deck");
        Rune bottom = Rune.Create(ownerId, Domain.Mind);
        Rune top = Rune.Create(ownerId, Domain.Order);

        zone.AddToTop(bottom);
        zone.AddToTop(top);

        Assert.Same(top, zone.PeekTop());
    }

    [Fact]
    public void Remove_ReturnsAndRemovesRune()
    {
        PlayerId ownerId = PlayerId.New();
        RuneZone zone = new(ownerId, "Rune Deck");
        Rune rune = Rune.Create(ownerId, Domain.Calm);
        zone.AddToTop(rune);

        bool removed = zone.Remove(rune.Id, out Rune? result);

        Assert.True(removed);
        Assert.Same(rune, result);
        Assert.Empty(zone.Runes);
    }

    [Fact]
    public void AddingDuplicateRune_Throws()
    {
        PlayerId ownerId = PlayerId.New();
        RuneZone zone = new(ownerId, "Rune Deck");
        Rune rune = Rune.Create(ownerId, Domain.Chaos);
        zone.AddToTop(rune);

        Assert.Throws<InvalidOperationException>(() => zone.AddToTop(rune));
    }
}
