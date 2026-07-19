using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Board;

public sealed class BoardCardState
{
    public BoardCardState(
        Card card,
        CardPosition position,
        bool hasDeflect = false,
        bool isReady = true,
        bool isTargetable = true)
    {
        ArgumentNullException.ThrowIfNull(card);

        Card = card;
        Position = position;
        HasDeflect = hasDeflect;
        IsReady = isReady;
        IsTargetable = isTargetable;
    }

    public Card Card { get; }

    public CardId CardId => Card.Id;

    public CardPosition Position { get; private set; }

    public bool HasDeflect { get; private set; }

    public bool IsReady { get; private set; }

    public bool IsTargetable { get; private set; }

    public void MoveTo(CardPosition position) =>
        Position = position;

    public void SetDeflect(bool hasDeflect) =>
        HasDeflect = hasDeflect;

    public void SetTargetable(bool isTargetable) =>
        IsTargetable = isTargetable;

    public bool Ready()
    {
        if (IsReady)
        {
            return false;
        }

        IsReady = true;
        return true;
    }

    public bool Exhaust()
    {
        if (!IsReady)
        {
            return false;
        }

        IsReady = false;
        return true;
    }
}
