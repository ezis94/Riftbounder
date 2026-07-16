using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Board;

public sealed class BoardCardState
{
    public BoardCardState(
        Card card,
        CardPosition position,
        bool hasDeflect = false)
    {
        ArgumentNullException.ThrowIfNull(card);

        Card = card;
        Position = position;
        HasDeflect = hasDeflect;
    }

    public Card Card { get; }

    public CardId CardId => Card.Id;

    public CardPosition Position { get; private set; }

    public bool HasDeflect { get; private set; }

    public void MoveTo(CardPosition position) =>
        Position = position;

    public void SetDeflect(bool hasDeflect) =>
        HasDeflect = hasDeflect;
}
