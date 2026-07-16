using Riftbounder.Core.Resources;

namespace Riftbounder.Core.Cards;

public sealed class CardDefinition
{
    public CardDefinition(
        string id,
        string name,
        CardType cardType,
        ResourceCost cost)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(cost);

        Id = id;
        Name = name;
        CardType = cardType;
        Cost = cost;
    }

    public string Id { get; }

    public string Name { get; }

    public CardType CardType { get; }

    public ResourceCost Cost { get; }
}
