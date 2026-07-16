using Riftbounder.Core.Effects;
using Riftbounder.Core.Resources;

namespace Riftbounder.Core.Cards;

public sealed class CardDefinition
{
    public CardDefinition(
        string id,
        string name,
        CardType cardType,
        ResourceCost cost,
        IReadOnlyList<SpellInstructionDefinition>? instructions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(cost);

        Id = id;
        Name = name;
        CardType = cardType;
        Cost = cost;
        Instructions = instructions?.ToArray()
            ?? Array.Empty<SpellInstructionDefinition>();
    }

    public string Id { get; }

    public string Name { get; }

    public CardType CardType { get; }

    public ResourceCost Cost { get; }

    public IReadOnlyList<SpellInstructionDefinition> Instructions { get; }
}
