using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Turns;

namespace Riftbounder.Engine.Tests;

public sealed class TurnControllerTests
{
    [Fact]
    public void Start_InitializesFirstPlayersAwakenPhase()
    {
        TestContext context = CreateContext();

        context.Controller.Start();

        Assert.Equal(1, context.Controller.State.TurnNumber);
        Assert.Equal(context.First.Id, context.Controller.State.TurnPlayerId);
        Assert.Equal(TurnPhase.Awaken, context.Controller.State.Phase);
        Assert.Null(context.Controller.State.PriorityPlayerId);
    }

    [Fact]
    public void Start_CannotBeCalledTwice()
    {
        TestContext context = CreateContext();
        context.Controller.Start();

        Assert.Throws<InvalidOperationException>(() => context.Controller.Start());
    }

    [Fact]
    public void AdvanceToMainPhase_ProcessesOfficialStartOfTurnOrder()
    {
        TestContext context = CreateContext();
        context.Controller.Start();

        context.Controller.AdvanceToMainPhase();

        Assert.Equal(TurnPhase.Main, context.Controller.State.Phase);
        Assert.Equal(context.First.Id, context.Controller.State.PriorityPlayerId);

        TurnPhase[] phases = context.Journal.Events
            .OfType<TurnPhaseChangedEvent>()
            .Select(gameEvent => gameEvent.CurrentPhase)
            .ToArray();

        Assert.Equal(
            [
                TurnPhase.Awaken,
                TurnPhase.Beginning,
                TurnPhase.Channel,
                TurnPhase.Draw,
                TurnPhase.Main
            ],
            phases);
    }

    [Fact]
    public void FirstPlayer_SkipsFirstTurnDraw()
    {
        TestContext context = CreateContext();
        Card card = Card.Create("top-card", context.First.Id);
        context.Game.RegisterCard(card, context.First.MainDeck);

        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();

        Assert.Empty(context.First.Hand.Cards);
        Assert.Single(context.First.MainDeck.Cards);
        Assert.Empty(context.Journal.Events.OfType<CardDrawnEvent>());
    }

    [Fact]
    public void SecondPlayer_DrawsOnFirstTurn()
    {
        TestContext context = CreateContext();
        Card card = Card.Create("top-card", context.Second.Id);
        context.Game.RegisterCard(card, context.Second.MainDeck);

        StartSecondPlayersTurn(context);
        context.Controller.AdvanceToMainPhase();

        CardDrawnEvent drawEvent = Assert.Single(
            context.Journal.Events.OfType<CardDrawnEvent>());

        Assert.Equal(context.Second.Id, drawEvent.PlayerId);
        Assert.Same(card, drawEvent.Card);
        Assert.Single(context.Second.Hand.Cards);
    }

    [Fact]
    public void FirstPlayer_RequestsTwoRunesDuringFirstChannelPhase()
    {
        TestContext context = CreateContext();

        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();

        ChannelRunesRequestedEvent channelEvent = Assert.Single(
            context.Journal.Events.OfType<ChannelRunesRequestedEvent>());

        Assert.Equal(context.First.Id, channelEvent.PlayerId);
        Assert.Equal(2, channelEvent.RuneCount);
    }

    [Fact]
    public void SecondPlayer_RequestsThreeRunesDuringFirstChannelPhase()
    {
        TestContext context = CreateContext();

        StartSecondPlayersTurn(context);
        context.Controller.AdvanceToMainPhase();

        ChannelRunesRequestedEvent channelEvent = context.Journal.Events
            .OfType<ChannelRunesRequestedEvent>()
            .Last();

        Assert.Equal(context.Second.Id, channelEvent.PlayerId);
        Assert.Equal(3, channelEvent.RuneCount);
    }


    [Fact]
    public void SecondPlayer_RequestsTwoRunesOnLaterTurns()
    {
        TestContext context = CreateContext();

        // First player's first turn.
        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();
        context.Controller.EndMainPhase();
        context.Controller.CompleteEndingPhase();

        // Second player's first turn.
        context.Controller.AdvanceToMainPhase();
        context.Controller.EndMainPhase();
        context.Controller.CompleteEndingPhase();

        // First player's second turn.
        context.Controller.AdvanceToMainPhase();
        context.Controller.EndMainPhase();
        context.Controller.CompleteEndingPhase();

        // Second player's second turn.
        context.Controller.AdvanceToMainPhase();

        ChannelRunesRequestedEvent channelEvent = context.Journal.Events
            .OfType<ChannelRunesRequestedEvent>()
            .Last();

        Assert.Equal(context.Second.Id, channelEvent.PlayerId);
        Assert.Equal(2, channelEvent.RuneCount);
    }

[Fact]
public void ChannelPhase_MovesRequestedRunesToBaseReady()
{
    TestContext context = CreateContext();
    Rune first = Rune.Create(context.First.Id, Domain.Mind);
    Rune second = Rune.Create(context.First.Id, Domain.Order);
    context.Game.RegisterRune(first, context.First.RuneDeck);
    context.Game.RegisterRune(second, context.First.RuneDeck);

    context.Controller.Start();
    context.Controller.AdvanceToMainPhase();

    Assert.Empty(context.First.RuneDeck.Runes);
    Assert.Equal(2, context.First.RunesInBase.Count);
    Assert.All(context.First.RunesInBase.Runes, rune => Assert.True(rune.IsReady));
    Assert.Equal(
        2,
        context.Journal.Events.OfType<RuneChanneledEvent>().Count());
}

[Fact]
public void ChannelPhase_WithTooFewRunes_ChannelsAsManyAsPossible()
{
    TestContext context = CreateContext();
    Rune rune = Rune.Create(context.First.Id, Domain.Mind);
    context.Game.RegisterRune(rune, context.First.RuneDeck);

    context.Controller.Start();
    context.Controller.AdvanceToMainPhase();

    Assert.Empty(context.First.RuneDeck.Runes);
    Assert.Single(context.First.RunesInBase.Runes);
    Assert.Single(context.Journal.Events.OfType<RuneChanneledEvent>());
}

[Fact]
public void AwakenPhase_ReadiesExhaustedRunes()
{
    TestContext context = CreateContext();
    Rune rune = Rune.Create(context.First.Id, Domain.Mind);
    context.Game.RegisterRune(rune, context.First.RuneDeck);
    context.Game.ChannelRunes(context.First.Id, 1);
    rune.Exhaust();

    context.Controller.Start();
    context.Controller.AdvanceStartOfTurn();

    RuneReadiedEvent readiedEvent = Assert.Single(
        context.Journal.Events.OfType<RuneReadiedEvent>());

    Assert.Same(rune, readiedEvent.Rune);
    Assert.True(rune.IsReady);
}


[Fact]
public void EndOfDrawPhase_EmptiesAllPlayersRunePools()
{
    TestContext context=CreateContext(); context.Game.AddEnergy(context.First.Id,2); context.Game.AddPower(context.Second.Id,PowerType.ForDomain(Domain.Order),1);
    context.Controller.Start(); context.Controller.AdvanceToMainPhase();
    Assert.Equal(0,context.First.RunePool.Energy); Assert.Empty(context.Second.RunePool.Power); Assert.Equal(2,context.Journal.Events.OfType<RunePoolEmptiedEvent>().Count());
}

[Fact]
public void EndOfTurn_EmptiesResourcesAddedDuringMainPhase()
{
    TestContext context=CreateContext(); context.Controller.Start(); context.Controller.AdvanceToMainPhase(); context.Game.AddEnergy(context.First.Id,2);
    context.Controller.EndMainPhase(); context.Controller.CompleteEndingPhase();
    Assert.Equal(0,context.First.RunePool.Energy); Assert.Equal("EndOfTurn",context.Journal.Events.OfType<RunePoolEmptiedEvent>().Last().Checkpoint);
}

    [Fact]
    public void EndMainPhase_RemovesPriorityAndEntersEnding()
    {
        TestContext context = CreateContext();
        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();

        context.Controller.EndMainPhase();

        Assert.Equal(TurnPhase.Ending, context.Controller.State.Phase);
        Assert.Null(context.Controller.State.PriorityPlayerId);
    }

    [Fact]
    public void CompleteEndingPhase_AdvancesTurnPlayerAndTurnNumber()
    {
        TestContext context = CreateContext();
        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();
        context.Controller.EndMainPhase();

        context.Controller.CompleteEndingPhase();

        Assert.Equal(2, context.Controller.State.TurnNumber);
        Assert.Equal(context.Second.Id, context.Controller.State.TurnPlayerId);
        Assert.Equal(TurnPhase.Awaken, context.Controller.State.Phase);
    }

    [Fact]
    public void EmptyDeckDraw_IsRecordedExplicitly()
    {
        TestContext context = CreateContext();

        StartSecondPlayersTurn(context);
        context.Controller.AdvanceToMainPhase();

        EmptyDeckDrawAttemptedEvent drawEvent = Assert.Single(
            context.Journal.Events.OfType<EmptyDeckDrawAttemptedEvent>());

        Assert.Equal(context.Second.Id, drawEvent.PlayerId);
    }

    [Fact]
    public void EndMainPhase_RejectsWrongPhase()
    {
        TestContext context = CreateContext();
        context.Controller.Start();

        Assert.Throws<InvalidOperationException>(() =>
            context.Controller.EndMainPhase());
    }

    private static void StartSecondPlayersTurn(TestContext context)
    {
        context.Controller.Start();
        context.Controller.AdvanceToMainPhase();
        context.Controller.EndMainPhase();
        context.Controller.CompleteEndingPhase();
    }

    private static TestContext CreateContext()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        EventJournal journal = new();
        TurnController controller = new(
            game,
            [first.Id, second.Id],
            journal,
            new FixedTimeProvider());

        return new TestContext(game, first, second, journal, controller);
    }

    private sealed record TestContext(
        Game Game,
        Player First,
        Player Second,
        EventJournal Journal,
        TurnController Controller);

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch;
    }
}
