using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Board;

public sealed class BoardState
{
    private readonly Dictionary<CardId, BoardCardState> _cards = [];

    public void Register(
        Card card,
        CardPosition position,
        bool hasDeflect = false,
        bool isReady = true,
        bool isTargetable = true)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (_cards.ContainsKey(card.Id))
        {
            throw new InvalidOperationException(
                $"Card {card.Id} is already registered in board state.");
        }

        _cards[card.Id] = new BoardCardState(
            card,
            position,
            hasDeflect,
            isReady,
            isTargetable);
    }

    public BoardCardState Get(CardId cardId) =>
        _cards.TryGetValue(cardId, out BoardCardState? state)
            ? state
            : throw new KeyNotFoundException(
                $"Card {cardId} is not registered in board state.");

    public bool TryGet(
        CardId cardId,
        out BoardCardState? state) =>
        _cards.TryGetValue(cardId, out state);
}
