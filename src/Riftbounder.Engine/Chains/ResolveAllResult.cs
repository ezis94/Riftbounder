namespace Riftbounder.Engine.Chains;

public sealed class ResolveAllResult
{
    public ResolveAllResult(
        IReadOnlyList<ResolvedChainItem> resolvedItems,
        IChainItem? blockedItem,
        ChainResolutionResult? failure)
    {
        ArgumentNullException.ThrowIfNull(resolvedItems);

        if ((blockedItem is null) != (failure is null))
        {
            throw new ArgumentException(
                "A blocked item and failure result must either both be present or both be absent.");
        }

        if (failure is { Succeeded: true })
        {
            throw new ArgumentException(
                "The failure result must represent a failed resolution.",
                nameof(failure));
        }

        ResolvedItems = resolvedItems;
        BlockedItem = blockedItem;
        Failure = failure;
    }

    public IReadOnlyList<ResolvedChainItem> ResolvedItems { get; }

    public IChainItem? BlockedItem { get; }

    public ChainResolutionResult? Failure { get; }

    public bool Completed => BlockedItem is null;

    public static ResolveAllResult Complete(
        IReadOnlyList<ResolvedChainItem> resolvedItems) =>
        new(resolvedItems, null, null);

    public static ResolveAllResult Blocked(
        IReadOnlyList<ResolvedChainItem> resolvedItems,
        IChainItem blockedItem,
        ChainResolutionResult failure) =>
        new(resolvedItems, blockedItem, failure);
}
