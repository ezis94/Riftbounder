namespace Riftbounder.Engine.Priority;

public sealed class NullChainFlowObserver : IChainFlowObserver
{
    public static NullChainFlowObserver Instance { get; } = new();

    private NullChainFlowObserver()
    {
    }

    public void OnChainClosed()
    {
    }
}
