using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Cards;

public sealed record Card(CardId Id, string DefinitionId, PlayerId OwnerId)
{
    public Card
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DefinitionId);
    }

    public static Card Create(string definitionId, PlayerId ownerId) =>
        new(CardId.New(), definitionId, ownerId);
}
