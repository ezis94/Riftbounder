using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Zones;

public sealed class Zone
{
    private readonly List<Card> _cards = [];

    public Zone(
        PlayerId ownerId,
        ZoneKind kind,
        string name)
        : this(
            ZoneId.New(),
            ownerId,
            kind,
            name)
    {
    }

    public Zone(
        ZoneId id,
        PlayerId? ownerId,
        ZoneKind kind,
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        OwnerId = ownerId;
        Kind = kind;
        Name = name;
    }

    public ZoneId Id { get; }

    public PlayerId? OwnerId { get; }

    public bool IsShared => OwnerId is null;

    public ZoneKind Kind { get; }

    public string Name { get; }

    public IReadOnlyList<Card> Cards => _cards;

    public int Count => _cards.Count;

    public bool Contains(CardId cardId) =>
        _cards.Any(card => card.Id == cardId);

    public Card? PeekTop() =>
        _cards.Count == 0 ? null : _cards[^1];

    public void AddToTop(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (Contains(card.Id))
        {
            throw new InvalidOperationException(
                $"Card {card.Id} is already in zone '{Name}'.");
        }

        _cards.Add(card);
    }

    public void AddToBottom(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (Contains(card.Id))
        {
            throw new InvalidOperationException(
                $"Card {card.Id} is already in zone '{Name}'.");
        }

        _cards.Insert(0, card);
    }

    public bool Remove(CardId cardId, out Card? card)
    {
        int index = _cards.FindIndex(
            candidate => candidate.Id == cardId);

        if (index < 0)
        {
            card = null;
            return false;
        }

        card = _cards[index];
        _cards.RemoveAt(index);
        return true;
    }
}
