using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class DrawCardsInstructionHandlerTests
{
    [Fact]
    public void Execute_DrawsRequestedCardsForSpellController()
    {
        TestContext context = CreateContext();
        Card bottom = context.RegisterDeckCard("bottom");
        Card top = context.RegisterDeckCard("top");
        DrawCardsInstructionHandler handler = context.CreateHandler();

        InstructionExecutionResult result = handler.Execute(
            context.CreateInstructionContext(amount: 2));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.AffectedTargetCount);
        Assert.Empty(context.First.MainDeck.Cards);
        Assert.Collection(
            context.First.Hand.Cards,
            card => Assert.Same(top, card),
            card => Assert.Same(bottom, card));
    }

    [Fact]
    public void Execute_UsesControllerRatherThanCardOwner()
    {
        TestContext context = CreateContext();
        Card controllerCard = Card.Create(
            "controller-card",
            context.Second.Id);
        context.Game.RegisterCard(
            controllerCard,
            context.Second.MainDeck);
        DrawCardsInstructionHandler handler = context.CreateHandler();

        InstructionExecutionResult result = handler.Execute(
            context.CreateInstructionContext(
                amount: 1,
                spellOwnerId: context.First.Id,
                controllerId: context.Second.Id));

        Assert.True(result.Succeeded);
        Assert.Single(context.Second.Hand.Cards);
        Assert.Empty(context.First.Hand.Cards);
    }

    [Fact]
    public void Execute_EmptyDeck_SucceedsWithNoDraw()
    {
        TestContext context = CreateContext();
        DrawCardsInstructionHandler handler = context.CreateHandler();

        InstructionExecutionResult result = handler.Execute(
            context.CreateInstructionContext(amount: 2));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.AffectedTargetCount);
        Assert.Empty(context.First.Hand.Cards);
        SpellEmptyDeckDrawAttemptedEvent emptyEvent = Assert.Single(
            context.Journal.Events
                .OfType<SpellEmptyDeckDrawAttemptedEvent>());
        Assert.Equal(1, emptyEvent.DrawIndex);
    }

    [Fact]
    public void Execute_DeckRunsOut_PreservesCardsAlreadyDrawn()
    {
        TestContext context = CreateContext();
        Card onlyCard = context.RegisterDeckCard("only");
        DrawCardsInstructionHandler handler = context.CreateHandler();

        InstructionExecutionResult result = handler.Execute(
            context.CreateInstructionContext(amount: 3));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.AffectedTargetCount);
        Assert.Single(context.First.Hand.Cards);
        Assert.Same(onlyCard, context.First.Hand.Cards[0]);
        Assert.Single(
            context.Journal.Events
                .OfType<SpellCardDrawnEvent>());
        SpellEmptyDeckDrawAttemptedEvent emptyEvent = Assert.Single(
            context.Journal.Events
                .OfType<SpellEmptyDeckDrawAttemptedEvent>());
        Assert.Equal(2, emptyEvent.DrawIndex);
    }

    [Fact]
    public void Execute_ZeroAmount_SucceedsWithoutMutation()
    {
        TestContext context = CreateContext();
        context.RegisterDeckCard("card");
        DrawCardsInstructionHandler handler = context.CreateHandler();

        InstructionExecutionResult result = handler.Execute(
            context.CreateInstructionContext(amount: 0));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.AffectedTargetCount);
        Assert.Single(context.First.MainDeck.Cards);
        Assert.Empty(context.Journal.Events);
    }

    [Fact]
    public void Execute_RecordsDrawEventsInDrawOrder()
    {
        TestContext context = CreateContext();
        Card bottom = context.RegisterDeckCard("bottom");
        Card top = context.RegisterDeckCard("top");
        DrawCardsInstructionHandler handler = context.CreateHandler();

        handler.Execute(
            context.CreateInstructionContext(amount: 2));

        Assert.Collection(
            context.Journal.Events,
            gameEvent =>
            {
                SpellCardDrawnEvent drawn =
                    Assert.IsType<SpellCardDrawnEvent>(gameEvent);
                Assert.Same(top, drawn.Card);
                Assert.Equal(1, drawn.DrawIndex);
            },
            gameEvent =>
            {
                SpellCardDrawnEvent drawn =
                    Assert.IsType<SpellCardDrawnEvent>(gameEvent);
                Assert.Same(bottom, drawn.Card);
                Assert.Equal(2, drawn.DrawIndex);
            });
    }

    private static TestContext CreateContext()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        return new TestContext(
            new Game(first, second),
            first,
            second,
            new EventJournal());
    }

    private sealed class TestContext
    {
        public TestContext(
            Game game,
            Player first,
            Player second,
            EventJournal journal)
        {
            Game = game;
            First = first;
            Second = second;
            Journal = journal;
        }

        public Game Game { get; }

        public Player First { get; }

        public Player Second { get; }

        public EventJournal Journal { get; }

        public Card RegisterDeckCard(string id)
        {
            Card card = Card.Create(id, First.Id);
            Game.RegisterCard(card, First.MainDeck);
            return card;
        }

        public DrawCardsInstructionHandler CreateHandler() =>
            new(
                Game,
                Journal,
                new FixedTimeProvider());

        public InstructionExecutionContext CreateInstructionContext(
            int amount,
            PlayerId? spellOwnerId = null,
            PlayerId? controllerId = null)
        {
            PlayerId owner = spellOwnerId ?? First.Id;
            PlayerId controller = controllerId ?? First.Id;

            Card spell = Card.Create(
                new CardDefinition(
                    "draw-spell",
                    "Draw Spell",
                    CardType.Spell,
                    ResourceCost.EnergyOnly(0),
                    [
                        new SpellInstructionDefinition(
                            DrawCardsInstructionHandler.Id,
                            [],
                            amount)
                    ]),
                owner);

            PlayCardChainItem item = PlayCardChainItem.Create(
                controller,
                spell,
                new ResourcePayment(0));

            return new InstructionExecutionContext(
                item,
                spell.Definition.Instructions[0],
                []);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            DateTimeOffset.UnixEpoch;
    }
}
