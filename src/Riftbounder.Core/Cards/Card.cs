using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;

namespace Riftbounder.Core.Cards;

public sealed record Card
{
    public Card(
        CardId id,
        CardDefinition definition,
        PlayerId ownerId)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Id = id;
        Definition = definition;
        OwnerId = ownerId;
    }

    public CardId Id { get; }

    public CardDefinition Definition { get; }

    public string DefinitionId => Definition.Id;

    public PlayerId OwnerId { get; }

    public static Card Create(
        CardDefinition definition,
        PlayerId ownerId) =>
        new(CardId.New(), definition, ownerId);

    public static Card Create(
        string definitionId,
        PlayerId ownerId) =>
        Create(
            new CardDefinition(
                definitionId,
                definitionId,
                CardType.Spell,
                ResourceCost.EnergyOnly(0)),
            ownerId);
}
