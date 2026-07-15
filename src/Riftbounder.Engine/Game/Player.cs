using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;

namespace Riftbounder.Engine.Game;

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
    }

    public PlayerId Id { get; }

    public string Name { get; }

    public Zone MainDeck { get; }

    public Zone Hand { get; }

    public Zone Trash { get; }
}
