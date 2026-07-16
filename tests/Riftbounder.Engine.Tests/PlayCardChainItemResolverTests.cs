using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class PlayCardChainItemResolverTests
{
    [Fact]
    public void Resolve_SuccessfulSpell_MovesCardToOwnersTrash()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));

        ChainResolutionResult result =
            context.Resolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.Single(context.First.Trash.Cards);
        Assert.Same(spell, context.First.Trash.Cards[0]);
        Assert.Same(
            context.First.Trash,
            context.Game.FindZoneContaining(spell.Id));
    }

    [Fact]
    public void Resolve_SuccessfulSpell_UsesCardsOwnerTrash()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.Second.Id,
            spell,
            new ResourcePayment(0));

        ChainResolutionResult result =
            context.Resolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.Single(context.First.Trash.Cards);
        Assert.Empty(context.Second.Trash.Cards);
    }

    [Fact]
    public void Resolve_ExecutesEffectBeforeMovingSpellToTrash()
    {
        TestContext context = CreateContext(
            new InspectingExecutor());
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));
        InspectingExecutor executor =
            Assert.IsType<InspectingExecutor>(context.Executor);
        executor.Game = context.Game;

        ChainResolutionResult result =
            context.Resolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.Null(executor.ZoneDuringExecution);
        Assert.Same(
            context.First.Trash,
            context.Game.FindZoneContaining(spell.Id));
    }

    [Fact]
    public void Resolve_FailedExecution_LeavesSpellOutsideTrash()
    {
        TestContext context = CreateContext(
            new FailingExecutor("Unsupported instruction."));
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));

        ChainResolutionResult result =
            context.Resolver.Resolve(item);

        Assert.False(result.Succeeded);
        Assert.Equal(
            "Unsupported instruction.",
            result.FailureReason);
        Assert.Empty(context.First.Trash.Cards);
        Assert.Null(context.Game.FindZoneContaining(spell.Id));
    }

    [Fact]
    public void Resolve_RecordsStartedAndCompletedEvents()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));

        context.Resolver.Resolve(item);

        Assert.Collection(
            context.Journal.Events,
            gameEvent =>
            {
                SpellResolutionStartedEvent started =
                    Assert.IsType<SpellResolutionStartedEvent>(
                        gameEvent);
                Assert.Equal(item.Id, started.ChainItemId);
                Assert.Same(spell, started.Card);
            },
            gameEvent =>
            {
                SpellResolutionCompletedEvent completed =
                    Assert.IsType<SpellResolutionCompletedEvent>(
                        gameEvent);
                Assert.Equal(item.Id, completed.ChainItemId);
                Assert.Same(spell, completed.Card);
            });
    }

    [Fact]
    public void Resolve_FailedExecution_RecordsFailureEvent()
    {
        TestContext context = CreateContext(
            new FailingExecutor("Failure."));
        Card spell = CreateSpell(context.First.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));

        context.Resolver.Resolve(item);

        Assert.Collection(
            context.Journal.Events,
            gameEvent =>
                Assert.IsType<SpellResolutionStartedEvent>(
                    gameEvent),
            gameEvent =>
            {
                SpellResolutionFailedEvent failed =
                    Assert.IsType<SpellResolutionFailedEvent>(
                        gameEvent);
                Assert.Equal("Failure.", failed.Reason);
            });
    }

    [Fact]
    public void Resolve_UnsupportedItem_ReturnsFailure()
    {
        TestContext context = CreateContext();
        TestChainItem item = new(
            ChainItemId.New(),
            context.First.Id,
            "Unsupported");

        ChainResolutionResult result =
            context.Resolver.Resolve(item);

        Assert.False(result.Succeeded);
        Assert.Contains(
            nameof(TestChainItem),
            result.FailureReason,
            StringComparison.Ordinal);
        Assert.Empty(context.Journal.Events);
    }

    [Fact]
    public void Resolve_SpellAlreadyInRegisteredZone_Throws()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(context.First.Id);
        context.Game.RegisterCard(spell, context.First.Hand);
        PlayCardChainItem item = PlayCardChainItem.Create(
            context.First.Id,
            spell,
            new ResourcePayment(0));

        Assert.Throws<InvalidOperationException>(() =>
            context.Resolver.Resolve(item));
    }

    private static TestContext CreateContext(
        ISpellEffectExecutor? executor = null)
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        EventJournal journal = new();
        ISpellEffectExecutor effectiveExecutor =
            executor ?? new NoOpSpellEffectExecutor();
        PlayCardChainItemResolver resolver = new(
            game,
            effectiveExecutor,
            journal,
            new FixedTimeProvider());

        return new TestContext(
            game,
            first,
            second,
            journal,
            effectiveExecutor,
            resolver);
    }

    private static Card CreateSpell(PlayerId ownerId) =>
        Card.Create(
            new CardDefinition(
                "test-spell",
                "Test Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0)),
            ownerId);

    private sealed record TestContext(
        Game Game,
        Player First,
        Player Second,
        EventJournal Journal,
        ISpellEffectExecutor Executor,
        PlayCardChainItemResolver Resolver);

    private sealed record TestChainItem(
        ChainItemId Id,
        PlayerId ControllerId,
        string Description)
        : IChainItem;

    private sealed class InspectingExecutor :
        ISpellEffectExecutor
    {
        public Game? Game { get; set; }

        public object? ZoneDuringExecution { get; private set; }

        public SpellExecutionResult Execute(
            PlayCardChainItem spell)
        {
            ZoneDuringExecution =
                Game?.FindZoneContaining(spell.Card.Id);
            return SpellExecutionResult.Success();
        }
    }

    private sealed class FailingExecutor :
        ISpellEffectExecutor
    {
        private readonly string _reason;

        public FailingExecutor(string reason)
        {
            _reason = reason;
        }

        public SpellExecutionResult Execute(
            PlayCardChainItem spell) =>
            SpellExecutionResult.Failure(_reason);
    }

    private sealed class FixedTimeProvider :
        TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            DateTimeOffset.UnixEpoch;
    }
}
