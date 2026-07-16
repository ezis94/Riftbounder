using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Tests;

public sealed class ChainTests
{
    [Fact]
    public void NewChain_IsEmpty()
    {
        Chain chain = new();

        Assert.True(chain.IsEmpty);
        Assert.Equal(0, chain.Count);
        Assert.Empty(chain.Items);
    }

    [Fact]
    public void Push_AddsItemToTop()
    {
        Chain chain = new();
        TestChainItem item = TestChainItem.Create("first");

        chain.Push(item);

        Assert.False(chain.IsEmpty);
        Assert.Equal(1, chain.Count);
        Assert.Same(item, chain.Peek());
    }

    [Fact]
    public void Push_MultipleItems_UsesLastInFirstOutOrder()
    {
        Chain chain = new();
        TestChainItem first = TestChainItem.Create("first");
        TestChainItem second = TestChainItem.Create("second");

        chain.Push(first);
        chain.Push(second);

        Assert.Same(second, chain.Pop());
        Assert.Same(first, chain.Pop());
        Assert.True(chain.IsEmpty);
    }

    [Fact]
    public void Peek_DoesNotRemoveItem()
    {
        Chain chain = new();
        TestChainItem item = TestChainItem.Create("first");
        chain.Push(item);

        IChainItem result = chain.Peek();

        Assert.Same(item, result);
        Assert.Equal(1, chain.Count);
    }

    [Fact]
    public void Pop_EmptyChain_Throws()
    {
        Chain chain = new();

        Assert.Throws<InvalidOperationException>(() => chain.Pop());
    }

    [Fact]
    public void Peek_EmptyChain_Throws()
    {
        Chain chain = new();

        Assert.Throws<InvalidOperationException>(() => chain.Peek());
    }

    [Fact]
    public void Push_Null_Throws()
    {
        Chain chain = new();

        Assert.Throws<ArgumentNullException>(() => chain.Push(null!));
    }

    [Fact]
    public void Push_DuplicateId_Throws()
    {
        Chain chain = new();
        ChainItemId id = ChainItemId.New();
        TestChainItem first = new(
            id,
            PlayerId.New(),
            "first");
        TestChainItem duplicate = new(
            id,
            PlayerId.New(),
            "duplicate");

        chain.Push(first);

        Assert.Throws<InvalidOperationException>(() =>
            chain.Push(duplicate));
    }

    [Fact]
    public void Items_AreOrderedFromBottomToTop()
    {
        Chain chain = new();
        TestChainItem first = TestChainItem.Create("first");
        TestChainItem second = TestChainItem.Create("second");
        chain.Push(first);
        chain.Push(second);

        Assert.Collection(
            chain.Items,
            item => Assert.Same(first, item),
            item => Assert.Same(second, item));
    }

    [Fact]
    public void Clear_RemovesEveryItem()
    {
        Chain chain = new();
        chain.Push(TestChainItem.Create("first"));
        chain.Push(TestChainItem.Create("second"));

        chain.Clear();

        Assert.True(chain.IsEmpty);
        Assert.Empty(chain.Items);
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
}
