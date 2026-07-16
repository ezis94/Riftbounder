using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Cards;

public sealed record PlayCardChainItem(
    ChainItemId Id,
    PlayerId ControllerId,
    Card Card,
    ResourcePayment Payment,
    string Description)
    : IChainItem
{
    public static PlayCardChainItem Create(
        PlayerId controllerId,
        Card card,
        ResourcePayment payment)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(payment);

        return new PlayCardChainItem(
            ChainItemId.New(),
            controllerId,
            card,
            payment,
            $"Play {card.Definition.Name}");
    }
}
