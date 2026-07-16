using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Runes;

public sealed class RuneZone
{
    private readonly List<Rune> _runes = [];

    public RuneZone(PlayerId ownerId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        OwnerId = ownerId;
        Name = name;
    }

    public PlayerId OwnerId { get; }

    public string Name { get; }

    public IReadOnlyList<Rune> Runes => _runes;

    public int Count => _runes.Count;

    public bool Contains(RuneId runeId) =>
        _runes.Any(rune => rune.Id == runeId);

    public Rune? PeekTop() =>
        _runes.Count == 0 ? null : _runes[^1];

    public void AddToTop(Rune rune)
    {
        ArgumentNullException.ThrowIfNull(rune);

        if (Contains(rune.Id))
        {
            throw new InvalidOperationException(
                $"Rune {rune.Id} is already in rune zone '{Name}'.");
        }

        _runes.Add(rune);
    }

    public void AddToBottom(Rune rune)
    {
        ArgumentNullException.ThrowIfNull(rune);

        if (Contains(rune.Id))
        {
            throw new InvalidOperationException(
                $"Rune {rune.Id} is already in rune zone '{Name}'.");
        }

        _runes.Insert(0, rune);
    }

    public bool Remove(RuneId runeId, out Rune? rune)
    {
        int index = _runes.FindIndex(candidate => candidate.Id == runeId);
        if (index < 0)
        {
            rune = null;
            return false;
        }

        rune = _runes[index];
        _runes.RemoveAt(index);
        return true;
    }
}
