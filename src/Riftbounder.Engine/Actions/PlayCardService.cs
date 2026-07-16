using Riftbounder.Core.Cards;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Priority;

namespace Riftbounder.Engine.Actions;

public sealed class PlayCardService
{
    private readonly Game _game;
    private readonly Chain _chain;
    private readonly ChainPriorityManager _priorityManager;
    private readonly IPlayCardPermission _permission;
    private readonly IPlayCostResolver _costResolver;

    public PlayCardService(
        Game game,
        Chain chain,
        ChainPriorityManager priorityManager,
        IPlayCardPermission permission,
        IPlayCostResolver? costResolver = null)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(priorityManager);
        ArgumentNullException.ThrowIfNull(permission);

        _game = game;
        _chain = chain;
        _priorityManager = priorityManager;
        _permission = permission;
        _costResolver = costResolver ?? new PrintedPlayCostResolver();
    }

    public PlayCardResult Play(PlayCardRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Payment);

        Player player = _game.GetPlayer(request.PlayerId);
        Card? card = player.Hand.Cards.SingleOrDefault(
            candidate => candidate.Id == request.CardId);

        if (card is null)
        {
            return PlayCardResult.Failed(
                PlayCardFailure.CardNotInHand);
        }

        if (card.OwnerId != request.PlayerId)
        {
            return PlayCardResult.Failed(
                PlayCardFailure.CardNotOwnedByPlayer);
        }

        if (card.Definition.CardType is not CardType.Spell)
        {
            return PlayCardResult.Failed(
                PlayCardFailure.UnsupportedCardType);
        }

        if (!_permission.CanPlay(request.PlayerId, card))
        {
            return PlayCardResult.Failed(
                PlayCardFailure.PlayerNotPermitted);
        }

        IReadOnlyList<TargetSnapshot> targets =
            request.Targets?.ToArray()
            ?? Array.Empty<TargetSnapshot>();

        ResourceCost totalCost =
            _costResolver.GetTotalCost(card, targets);

        if (!player.RunePool.CanPay(
                totalCost,
                request.Payment))
        {
            return PlayCardResult.Failed(
                PlayCardFailure.InvalidPayment);
        }

        // Rule 354: remove the card from its source zone and put it on the
        // Chain. This engine represents the Chain zone through the item.
        if (!player.Hand.Remove(card.Id, out Card? removed)
            || removed is null)
        {
            throw new InvalidOperationException(
                "The card disappeared from hand during play finalization.");
        }

        PlayCardChainItem item = PlayCardChainItem.Create(
            request.PlayerId,
            removed,
            request.Payment,
            targets);

        bool paymentCompleted = false;
        bool pushed = false;

        try
        {
            player.RunePool.Pay(
                totalCost,
                request.Payment);
            paymentCompleted = true;

            _chain.Push(item);
            pushed = true;

            if (_priorityManager.IsActive)
            {
                _priorityManager.NotifyChainItemAdded(item);
            }
            else
            {
                _priorityManager.Begin();
            }

            return PlayCardResult.Success(item);
        }
        catch
        {
            if (pushed
                && !_chain.IsEmpty
                && ReferenceEquals(_chain.Peek(), item))
            {
                _chain.Pop();
            }

            if (paymentCompleted)
            {
                player.RunePool.Refund(request.Payment);
            }

            player.Hand.AddToTop(removed);
            throw;
        }
    }
}
