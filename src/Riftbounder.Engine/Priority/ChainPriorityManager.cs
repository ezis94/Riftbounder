using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Priority;

public sealed class ChainPriorityManager
{
    private readonly Chain _chain;
    private readonly ChainResolver _chainResolver;
    private readonly IReadOnlyList<PlayerId> _turnOrder;
    private readonly IChainFlowObserver _flowObserver;
    private readonly HashSet<PlayerId> _playersWhoPassed = [];

    public ChainPriorityManager(
        Chain chain,
        ChainResolver chainResolver,
        IReadOnlyList<PlayerId> turnOrder,
        IChainFlowObserver? flowObserver = null)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(chainResolver);
        ArgumentNullException.ThrowIfNull(turnOrder);

        if (turnOrder.Count < 2)
        {
            throw new ArgumentException(
                "Priority requires at least two players.",
                nameof(turnOrder));
        }

        if (turnOrder.Distinct().Count() != turnOrder.Count)
        {
            throw new ArgumentException(
                "Turn order cannot contain duplicate players.",
                nameof(turnOrder));
        }

        _chain = chain;
        _chainResolver = chainResolver;
        _turnOrder = turnOrder.ToArray();
        _flowObserver = flowObserver ?? NullChainFlowObserver.Instance;
    }

    public PlayerId? PriorityPlayerId { get; private set; }

    public bool IsActive => PriorityPlayerId is not null;

    public IReadOnlyCollection<PlayerId> PlayersWhoPassed => _playersWhoPassed;

    public void Begin()
    {
        if (_chain.IsEmpty)
        {
            throw new InvalidOperationException(
                "Cannot begin Chain priority while the Chain is empty.");
        }

        _playersWhoPassed.Clear();
        PriorityPlayerId = _chain.Peek().ControllerId;
    }

    public void NotifyChainItemAdded(IChainItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_chain.IsEmpty || !ReferenceEquals(_chain.Peek(), item))
        {
            throw new InvalidOperationException(
                "The added Chain item must already be the newest item on the Chain.");
        }

        _playersWhoPassed.Clear();
        PriorityPlayerId = item.ControllerId;
    }

    public PriorityPassResult Pass(PlayerId playerId)
    {
        EnsureActivePriority(playerId);

        _playersWhoPassed.Add(playerId);

        if (_playersWhoPassed.Count < _turnOrder.Count)
        {
            PriorityPlayerId = GetNextPlayer(playerId);
            return new PriorityPassResult(
                PriorityPassOutcome.PriorityPassed,
                PriorityPlayerId,
                null);
        }

        ResolvedChainItem resolution = _chainResolver.ResolveTop();

        if (!resolution.Result.Succeeded)
        {
            _playersWhoPassed.Clear();
            PriorityPlayerId = resolution.Item.ControllerId;

            return new PriorityPassResult(
                PriorityPassOutcome.ResolutionFailed,
                PriorityPlayerId,
                resolution);
        }

        _playersWhoPassed.Clear();

        if (_chain.IsEmpty)
        {
            PriorityPlayerId = null;
            _flowObserver.OnChainClosed();

            return new PriorityPassResult(
                PriorityPassOutcome.ChainClosed,
                null,
                resolution);
        }

        PriorityPlayerId = _chain.Peek().ControllerId;

        return new PriorityPassResult(
            PriorityPassOutcome.ChainItemResolved,
            PriorityPlayerId,
            resolution);
    }

    private void EnsureActivePriority(PlayerId playerId)
    {
        if (PriorityPlayerId is null)
        {
            throw new InvalidOperationException(
                "Chain priority is not active.");
        }

        if (PriorityPlayerId != playerId)
        {
            throw new InvalidOperationException(
                $"Player {playerId} does not currently have priority.");
        }
    }

    private PlayerId GetNextPlayer(PlayerId playerId)
    {
        for (int index = 0; index < _turnOrder.Count; index++)
        {
            if (_turnOrder[index] == playerId)
            {
                return _turnOrder[(index + 1) % _turnOrder.Count];
            }
        }

        throw new InvalidOperationException(
            $"Player {playerId} is not in the configured turn order.");
    }
}
