using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Zones;

public sealed class Zone
{
    private readonly List<Card> _cards = [];

    public Zone(
        PlayerId ownerId,
        ZoneKind kind,
        string name,
        int? maximumOccupancy = null)
        : this(
            ZoneId.New(),
            ownerId,
            kind,
            name,
            maximumOccupancy)
    {
    }

    public Zone(
        ZoneId id,
        PlayerId? ownerId,
        ZoneKind kind,
        string name,
        int? maximumOccupancy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (maximumOccupancy is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumOccupancy),
                "Maximum occupancy cannot be negative.");
        }

        Id = id;
        OwnerId = ownerId;
        Kind = kind;
        Name = name;
        MaximumOccupancy = maximumOccupancy;
    }

    public ZoneId Id { get; }

    public PlayerId? OwnerId { get; }

    public bool IsShared => OwnerId is null;

    public ZoneKind Kind { get; }

    public string Name { get; }

    public int? MaximumOccupancy { get; }

    public IReadOnlyList<Card> Cards => _cards;

    public int Count => _cards.Count;

    public bool Contains(CardId cardId) =>
        _cards.Any(card => card.Id == cardId);

    public Card? PeekTop() =>
        _cards.Count == 0 ? null : _cards[^1];

    public void AddToTop(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);
        EnsureCanAdd(card);
        _cards.Add(card);
    }

    public void AddToBottom(Card card)
    {
        ArgumentNullException.ThrowIfNull(card);
        EnsureCanAdd(card);
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

    private void EnsureCanAdd(Card card)
    {
        if (Contains(card.Id))
        {
            throw new InvalidOperationException(
                $"Card {card.Id} is already in zone '{Name}'.");
        }

        if (MaximumOccupancy is int maximumOccupancy
            && Count >= maximumOccupancy)
        {
            throw new InvalidOperationException(
                $"Zone '{Name}' has reached its maximum occupancy of {maximumOccupancy}.");
        }
    }
}
