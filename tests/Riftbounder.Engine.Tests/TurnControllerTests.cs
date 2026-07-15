using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
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
