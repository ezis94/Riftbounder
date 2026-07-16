using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Priority;

namespace Riftbounder.Engine.Tests;

public sealed class ChainPriorityManagerTests
{
    [Fact]
    public void Begin_AssignsPriorityToControllerOfNewestChainItem()
    {
        TestContext context = CreateContext();
        TestChainItem first = context.Push(context.First, "first");
        TestChainItem newest = context.Push(context.Second, "newest");

        context.Manager.Begin();

        Assert.True(context.Manager.IsActive);
        Assert.Equal(newest.ControllerId, context.Manager.PriorityPlayerId);
        Assert.NotEqual(first.ControllerId, context.Manager.PriorityPlayerId);
    }

    [Fact]
    public void Begin_EmptyChain_Throws()
    {
        TestContext context = CreateContext();

        Assert.Throws<InvalidOperationException>(() =>
            context.Manager.Begin());
    }

    [Fact]
    public void FirstPass_TransfersPriorityToNextPlayer()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "item");
        context.Manager.Begin();

        PriorityPassResult result = context.Manager.Pass(context.First);

        Assert.Equal(PriorityPassOutcome.PriorityPassed, result.Outcome);
        Assert.Equal(context.Second, result.PriorityPlayerId);
        Assert.Equal(context.Second, context.Manager.PriorityPlayerId);
        Assert.Null(result.Resolution);
        Assert.Contains(context.First, context.Manager.PlayersWhoPassed);
    }

    [Fact]
    public void PlayerWithoutPriority_CannotPass()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "item");
        context.Manager.Begin();

        Assert.Throws<InvalidOperationException>(() =>
            context.Manager.Pass(context.Second));
    }

    [Fact]
    public void PassBeforeBegin_Throws()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "item");

        Assert.Throws<InvalidOperationException>(() =>
            context.Manager.Pass(context.First));
    }

    [Fact]
    public void AllPlayersPass_ResolvesNewestItem()
    {
        TestContext context = CreateContext();
        TestChainItem bottom = context.Push(context.First, "bottom");
        TestChainItem top = context.Push(context.Second, "top");
        context.Manager.Begin();

        context.Manager.Pass(context.Second);
        PriorityPassResult result = context.Manager.Pass(context.First);

        Assert.Equal(PriorityPassOutcome.ChainItemResolved, result.Outcome);
        ResolvedChainItem resolution = Assert.IsType<ResolvedChainItem>(
            result.Resolution);
        Assert.Same(top, resolution.Item);
        Assert.Single(context.Chain.Items);
        Assert.Same(bottom, context.Chain.Peek());
    }

    [Fact]
    public void AfterResolution_PriorityReturnsToControllerOfNewestRemainingItem()
    {
        TestContext context = CreateContext();
        TestChainItem bottom = context.Push(context.First, "bottom");
        context.Push(context.Second, "top");
        context.Manager.Begin();

        context.Manager.Pass(context.Second);
        PriorityPassResult result = context.Manager.Pass(context.First);

        Assert.Equal(bottom.ControllerId, result.PriorityPlayerId);
        Assert.Equal(bottom.ControllerId, context.Manager.PriorityPlayerId);
        Assert.Empty(context.Manager.PlayersWhoPassed);
    }

    [Fact]
    public void ResolvingLastItem_ClosesChainAndClearsPriority()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "only");
        context.Manager.Begin();

        context.Manager.Pass(context.First);
        PriorityPassResult result = context.Manager.Pass(context.Second);

        Assert.Equal(PriorityPassOutcome.ChainClosed, result.Outcome);
        Assert.True(context.Chain.IsEmpty);
        Assert.False(context.Manager.IsActive);
        Assert.Null(context.Manager.PriorityPlayerId);
        Assert.Equal(1, context.Observer.ClosedCount);
    }

    [Fact]
    public void AddingItem_ResetsPassHistoryAndGivesItsControllerPriority()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "bottom");
        context.Manager.Begin();
        context.Manager.Pass(context.First);
        TestChainItem response = context.Push(context.Second, "response");

        context.Manager.NotifyChainItemAdded(response);

        Assert.Equal(context.Second, context.Manager.PriorityPlayerId);
        Assert.Empty(context.Manager.PlayersWhoPassed);
    }

    [Fact]
    public void NotifyChainItemAdded_RequiresNewestItemAlreadyOnChain()
    {
        TestContext context = CreateContext();
        context.Push(context.First, "bottom");
        context.Manager.Begin();
        TestChainItem absent = TestChainItem.Create(context.Second, "absent");

        Assert.Throws<InvalidOperationException>(() =>
            context.Manager.NotifyChainItemAdded(absent));
    }

    [Fact]
    public void FailedResolution_LeavesItemAndReturnsPriorityToItsController()
    {
        TestContext context = CreateContext(failDescription: "blocked");
        TestChainItem blocked = context.Push(context.Second, "blocked");
        context.Manager.Begin();

        context.Manager.Pass(context.Second);
        PriorityPassResult result = context.Manager.Pass(context.First);

        Assert.Equal(PriorityPassOutcome.ResolutionFailed, result.Outcome);
        ResolvedChainItem resolution = Assert.IsType<ResolvedChainItem>(
            result.Resolution);
        Assert.False(resolution.Result.Succeeded);
        Assert.Same(blocked, context.Chain.Peek());
        Assert.Equal(context.Second, context.Manager.PriorityPlayerId);
        Assert.Empty(context.Manager.PlayersWhoPassed);
        Assert.Equal(0, context.Observer.ClosedCount);
    }

    [Fact]
    public void ThreePlayerTurnOrder_RequiresEveryPlayerToPass()
    {
        PlayerId first = PlayerId.New();
        PlayerId second = PlayerId.New();
        PlayerId third = PlayerId.New();
        Chain chain = new();
        TestChainItem item = TestChainItem.Create(first, "item");
        chain.Push(item);
        RecordingResolver itemResolver = new();
        ChainResolver resolver = new(chain, itemResolver);
        ChainPriorityManager manager = new(
            chain,
            resolver,
            [first, second, third]);
        manager.Begin();

        Assert.Equal(
            PriorityPassOutcome.PriorityPassed,
            manager.Pass(first).Outcome);
        Assert.Equal(second, manager.PriorityPlayerId);
        Assert.Equal(
            PriorityPassOutcome.PriorityPassed,
            manager.Pass(second).Outcome);
        Assert.Equal(third, manager.PriorityPlayerId);

        PriorityPassResult result = manager.Pass(third);

        Assert.Equal(PriorityPassOutcome.ChainClosed, result.Outcome);
        Assert.True(chain.IsEmpty);
    }

    [Fact]
    public void Constructor_RejectsDuplicateTurnOrderPlayers()
    {
        PlayerId player = PlayerId.New();
        Chain chain = new();
        ChainResolver resolver = new(chain, new RecordingResolver());

        Assert.Throws<ArgumentException>(() =>
            new ChainPriorityManager(
                chain,
                resolver,
                [player, player]));
    }

    [Fact]
    public void Constructor_RequiresAtLeastTwoPlayers()
    {
        PlayerId player = PlayerId.New();
        Chain chain = new();
        ChainResolver resolver = new(chain, new RecordingResolver());

        Assert.Throws<ArgumentException>(() =>
            new ChainPriorityManager(
                chain,
                resolver,
                [player]));
    }

    private static TestContext CreateContext(string? failDescription = null)
    {
        PlayerId first = PlayerId.New();
        PlayerId second = PlayerId.New();
        Chain chain = new();
        RecordingResolver itemResolver = new(failDescription);
        ChainResolver resolver = new(chain, itemResolver);
        RecordingFlowObserver observer = new();
        ChainPriorityManager manager = new(
            chain,
            resolver,
            [first, second],
            observer);

        return new TestContext(
            first,
            second,
            chain,
            manager,
            observer);
    }

    private sealed record TestContext(
        PlayerId First,
        PlayerId Second,
        Chain Chain,
        ChainPriorityManager Manager,
        RecordingFlowObserver Observer)
    {
        public TestChainItem Push(PlayerId controllerId, string description)
        {
            TestChainItem item = TestChainItem.Create(
                controllerId,
                description);
            Chain.Push(item);
            return item;
        }
    }

    private sealed record TestChainItem(
        ChainItemId Id,
        PlayerId ControllerId,
        string Description)
        : IChainItem
    {
        public static TestChainItem Create(
            PlayerId controllerId,
            string description) =>
            new(ChainItemId.New(), controllerId, description);
    }

    private sealed class RecordingResolver : IChainItemResolver
    {
        private readonly string? _failDescription;

        public RecordingResolver(string? failDescription = null)
        {
            _failDescription = failDescription;
        }

        public ChainResolutionResult Resolve(IChainItem item) =>
            item.Description == _failDescription
                ? ChainResolutionResult.Failure("Configured failure.")
                : ChainResolutionResult.Success();
    }

    private sealed class RecordingFlowObserver : IChainFlowObserver
    {
        public int ClosedCount { get; private set; }

        public void OnChainClosed() => ClosedCount++;
    }
}
