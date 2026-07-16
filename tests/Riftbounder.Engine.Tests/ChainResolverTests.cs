using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Tests;

public sealed class ChainResolverTests
{
    [Fact]
    public void ResolveTop_Success_RemovesResolvedItem()
    {
        Chain chain = new();
        TestChainItem item = TestChainItem.Create("item");
        chain.Push(item);
        RecordingResolver itemResolver = new();
        ChainResolver resolver = new(chain, itemResolver);

        ResolvedChainItem result = resolver.ResolveTop();

        Assert.True(result.Result.Succeeded);
        Assert.Same(item, result.Item);
        Assert.True(chain.IsEmpty);
        Assert.Collection(
            itemResolver.ResolvedItems,
            resolved => Assert.Same(item, resolved));
    }

    [Fact]
    public void ResolveTop_Failure_LeavesItemOnChain()
    {
        Chain chain = new();
        TestChainItem item = TestChainItem.Create("item");
        chain.Push(item);
        RecordingResolver itemResolver = new(item.Id);
        ChainResolver resolver = new(chain, itemResolver);

        ResolvedChainItem result = resolver.ResolveTop();

        Assert.False(result.Result.Succeeded);
        Assert.Equal("Configured failure.", result.Result.FailureReason);
        Assert.Equal(1, chain.Count);
        Assert.Same(item, chain.Peek());
    }

    [Fact]
    public void ResolveTop_EmptyChain_Throws()
    {
        ChainResolver resolver = new(
            new Chain(),
            new RecordingResolver());

        Assert.Throws<InvalidOperationException>(() =>
            resolver.ResolveTop());
    }

    [Fact]
    public void ResolveAll_ResolvesInLastInFirstOutOrder()
    {
        Chain chain = new();
        TestChainItem first = TestChainItem.Create("first");
        TestChainItem second = TestChainItem.Create("second");
        TestChainItem third = TestChainItem.Create("third");
        chain.Push(first);
        chain.Push(second);
        chain.Push(third);
        RecordingResolver itemResolver = new();
        ChainResolver resolver = new(chain, itemResolver);

        ResolveAllResult result = resolver.ResolveAll();

        Assert.True(result.Completed);
        Assert.True(chain.IsEmpty);
        Assert.Collection(
            itemResolver.ResolvedItems,
            item => Assert.Same(third, item),
            item => Assert.Same(second, item),
            item => Assert.Same(first, item));
        Assert.Equal(3, result.ResolvedItems.Count);
    }

    [Fact]
    public void ResolveAll_StopsAtFirstFailure()
    {
        Chain chain = new();
        TestChainItem first = TestChainItem.Create("first");
        TestChainItem blocked = TestChainItem.Create("blocked");
        TestChainItem top = TestChainItem.Create("top");
        chain.Push(first);
        chain.Push(blocked);
        chain.Push(top);
        RecordingResolver itemResolver = new(blocked.Id);
        ChainResolver resolver = new(chain, itemResolver);

        ResolveAllResult result = resolver.ResolveAll();

        Assert.False(result.Completed);
        Assert.Same(blocked, result.BlockedItem);
        Assert.Equal("Configured failure.", result.Failure?.FailureReason);
        Assert.Single(result.ResolvedItems);
        Assert.Equal(2, chain.Count);
        Assert.Same(blocked, chain.Peek());
        Assert.Collection(
            itemResolver.ResolvedItems,
            item => Assert.Same(top, item),
            item => Assert.Same(blocked, item));
    }

    [Fact]
    public void ResolveAll_EmptyChain_CompletesWithoutResolutions()
    {
        Chain chain = new();
        ChainResolver resolver = new(
            chain,
            new RecordingResolver());

        ResolveAllResult result = resolver.ResolveAll();

        Assert.True(result.Completed);
        Assert.Empty(result.ResolvedItems);
        Assert.Null(result.BlockedItem);
        Assert.Null(result.Failure);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Chain chain = new();
        RecordingResolver itemResolver = new();

        Assert.Throws<ArgumentNullException>(() =>
            new ChainResolver(null!, itemResolver));
        Assert.Throws<ArgumentNullException>(() =>
            new ChainResolver(chain, null!));
    }

    [Fact]
    public void ChainItemIds_AreUnique()
    {
        ChainItemId first = ChainItemId.New();
        ChainItemId second = ChainItemId.New();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Failure_RequiresReason()
    {
        Assert.Throws<ArgumentException>(() =>
            ChainResolutionResult.Failure(" "));
    }

    private sealed record TestChainItem(
        ChainItemId Id,
        PlayerId ControllerId,
        string Description)
        : IChainItem
    {
        public static TestChainItem Create(string description) =>
            new(ChainItemId.New(), PlayerId.New(), description);
    }

    private sealed class RecordingResolver : IChainItemResolver
    {
        private readonly ChainItemId? _failureItemId;

        public RecordingResolver(ChainItemId? failureItemId = null)
        {
            _failureItemId = failureItemId;
        }

        public List<IChainItem> ResolvedItems { get; } = [];

        public ChainResolutionResult Resolve(IChainItem item)
        {
            ResolvedItems.Add(item);

            return item.Id == _failureItemId
                ? ChainResolutionResult.Failure("Configured failure.")
                : ChainResolutionResult.Success();
        }
    }
}
