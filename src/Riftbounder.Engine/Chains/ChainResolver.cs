namespace Riftbounder.Engine.Chains;

public sealed class ChainResolver
{
    private readonly Chain _chain;
    private readonly IChainItemResolver _itemResolver;

    public ChainResolver(Chain chain, IChainItemResolver itemResolver)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(itemResolver);

        _chain = chain;
        _itemResolver = itemResolver;
    }

    public ResolvedChainItem ResolveTop()
    {
        IChainItem item = _chain.Peek();
        ChainResolutionResult result = _itemResolver.Resolve(item);

        if (result.Succeeded)
        {
            IChainItem removed = _chain.Pop();

            if (!ReferenceEquals(item, removed))
            {
                throw new InvalidOperationException(
                    "The Chain changed unexpectedly during item resolution.");
            }
        }

        return new ResolvedChainItem(item, result);
    }

    public ResolveAllResult ResolveAll()
    {
        List<ResolvedChainItem> resolvedItems = [];

        while (!_chain.IsEmpty)
        {
            ResolvedChainItem resolution = ResolveTop();

            if (!resolution.Result.Succeeded)
            {
                return ResolveAllResult.Blocked(
                    resolvedItems,
                    resolution.Item,
                    resolution.Result);
            }

            resolvedItems.Add(resolution);
        }

        return ResolveAllResult.Complete(resolvedItems);
    }
}
