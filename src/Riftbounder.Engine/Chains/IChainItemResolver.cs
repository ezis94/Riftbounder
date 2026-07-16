namespace Riftbounder.Engine.Chains;

public interface IChainItemResolver
{
    ChainResolutionResult Resolve(IChainItem item);
}
