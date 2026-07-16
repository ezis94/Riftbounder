using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Cards;

public sealed record PlayCardChainItem(
    ChainItemId Id,
    PlayerId ControllerId,
    Card Card,
    ResourcePayment Payment,
    IReadOnlyList<TargetSnapshot> Targets,
    string Description)
    : IChainItem
{
    public static PlayCardChainItem Create(
        PlayerId controllerId,
        Card card,
        ResourcePayment payment,
        IReadOnlyList<TargetSnapshot>? targets = null)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(payment);

        return new PlayCardChainItem(
            ChainItemId.New(),
            controllerId,
            card,
            payment,
            targets?.ToArray()
                ?? Array.Empty<TargetSnapshot>(),
            $"Play {card.Definition.Name}");
    }
}
