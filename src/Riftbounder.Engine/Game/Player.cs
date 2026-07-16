using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Resources;

namespace Riftbounder.Engine.Games;

public sealed class Player
{
    public Player(PlayerId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        MainDeck = new Zone(id, ZoneKind.MainDeck, $"{name}'s Main Deck");
        Hand = new Zone(id, ZoneKind.Hand, $"{name}'s Hand");
        Trash = new Zone(id, ZoneKind.Trash, $"{name}'s Trash");
        RuneDeck = new RuneZone(id, $"{name}'s Rune Deck");
        RunesInBase = new RuneZone(id, $"{name}'s Runes in Base");
        RunePool = new RunePool();
    }

    public PlayerId Id { get; }

    public string Name { get; }

    public Zone MainDeck { get; }

    public Zone Hand { get; }

    public Zone Trash { get; }

    public RuneZone RuneDeck { get; }

    public RuneZone RunesInBase { get; }

    public RunePool RunePool { get; }
}
