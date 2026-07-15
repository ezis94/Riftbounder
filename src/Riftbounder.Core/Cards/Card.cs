using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Cards;

public sealed record Card
{
    public Card(CardId id, string definitionId, PlayerId ownerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        Id = id;
        DefinitionId = definitionId;
        OwnerId = ownerId;
    }

    public CardId Id { get; }

    public string DefinitionId { get; }

    public PlayerId OwnerId { get; }

    public static Card Create(string definitionId, PlayerId ownerId) =>
        new(CardId.New(), definitionId, ownerId);
}
