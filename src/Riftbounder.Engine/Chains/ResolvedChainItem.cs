namespace Riftbounder.Engine.Chains;

public sealed record ResolvedChainItem(
    IChainItem Item,
    ChainResolutionResult Result);
