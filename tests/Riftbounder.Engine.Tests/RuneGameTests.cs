using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class RuneGameTests
{
    [Fact]
    public void RegisterRune_AddsRuneToRegisteredRuneZone()
    {
        TestContext context = CreateContext();
        Rune rune = Rune.Create(context.First.Id, Domain.Mind);

        context.Game.RegisterRune(rune, context.First.RuneDeck);

        Assert.Same(
            context.First.RuneDeck,
            context.Game.FindRuneZoneContaining(rune.Id));
    }

    [Fact]
    public void RegisterRune_RejectsDuplicateRegistration()
    {
        TestContext context = CreateContext();
        Rune rune = Rune.Create(context.First.Id, Domain.Mind);
        context.Game.RegisterRune(rune, context.First.RuneDeck);

        Assert.Throws<InvalidOperationException>(() =>
            context.Game.RegisterRune(rune, context.First.RunesInBase));
    }

    [Fact]
    public void ChannelRunes_MovesTopRunesToBaseReady()
    {
        TestContext context = CreateContext();
        Rune first = Rune.Create(context.First.Id, Domain.Mind);
        Rune second = Rune.Create(context.First.Id, Domain.Order);
        context.Game.RegisterRune(first, context.First.RuneDeck);
        context.Game.RegisterRune(second, context.First.RuneDeck);

        var result = context.Game.ChannelRunes(context.First.Id, 2);

        Assert.Equal(2, result.ChanneledCount);
        Assert.True(result.WasFullySatisfied);
        Assert.Empty(context.First.RuneDeck.Runes);
        Assert.Equal(2, context.First.RunesInBase.Count);
        Assert.All(result.ChanneledRunes, rune => Assert.True(rune.IsReady));
        Assert.Collection(
            result.ChanneledRunes,
            rune => Assert.Same(second, rune),
            rune => Assert.Same(first, rune));
    }

    [Fact]
    public void ChannelRunes_WhenDeckHasFewerRunes_ChannelsAsManyAsPossible()
    {
        TestContext context = CreateContext();
        Rune rune = Rune.Create(context.First.Id, Domain.Mind);
        context.Game.RegisterRune(rune, context.First.RuneDeck);

        var result = context.Game.ChannelRunes(context.First.Id, 2);

        Assert.Equal(1, result.ChanneledCount);
        Assert.False(result.WasFullySatisfied);
        Assert.Same(rune, result.ChanneledRunes[0]);
    }

    [Fact]
    public void ChannelRunes_CanEnterExhaustedWhenEffectSpecifies()
    {
        TestContext context = CreateContext();
        Rune rune = Rune.Create(context.First.Id, Domain.Order);
        context.Game.RegisterRune(rune, context.First.RuneDeck);

        var result = context.Game.ChannelRunes(
            context.First.Id,
            1,
            enterExhausted: true);

        Assert.False(result.ChanneledRunes[0].IsReady);
    }

    [Fact]
    public void ReadyAllRunes_ReadiesOnlyExhaustedRunesInBase()
    {
        TestContext context = CreateContext();
        Rune readyRune = Rune.Create(context.First.Id, Domain.Mind);
        Rune exhaustedRune = Rune.Create(context.First.Id, Domain.Order);
        context.Game.RegisterRune(readyRune, context.First.RuneDeck);
        context.Game.RegisterRune(exhaustedRune, context.First.RuneDeck);
        context.Game.ChannelRunes(context.First.Id, 2);
        exhaustedRune.Exhaust();

        IReadOnlyList<Rune> readied = context.Game.ReadyAllRunes(context.First.Id);

        Assert.Single(readied);
        Assert.Same(exhaustedRune, readied[0]);
        Assert.True(exhaustedRune.IsReady);
        Assert.True(readyRune.IsReady);
    }

    private static TestContext CreateContext()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        return new TestContext(new Game(first, second), first, second);
    }

    private sealed record TestContext(
        Game Game,
        Player First,
        Player Second);
}
