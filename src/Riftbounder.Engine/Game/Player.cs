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

        MainDeck = new Zone(
            id,
            ZoneKind.MainDeck,
            $"{name}'s Main Deck");
        Hand = new Zone(
            id,
            ZoneKind.Hand,
            $"{name}'s Hand");
        Base = new Zone(
            id,
            ZoneKind.Base,
            $"{name}'s Base");
        Hidden = new Zone(
            id,
            ZoneKind.Hidden,
            $"{name}'s Hidden Zone");
        Trash = new Zone(
            id,
            ZoneKind.Trash,
            $"{name}'s Trash");
        Banish = new Zone(
            id,
            ZoneKind.Banish,
            $"{name}'s Banish Zone");

        RuneDeck = new RuneZone(
            id,
            $"{name}'s Rune Deck");
        RunesInBase = new RuneZone(
            id,
            $"{name}'s Runes in Base");
        RunePool = new RunePool();
    }

    public PlayerId Id { get; }

    public string Name { get; }

    public Zone MainDeck { get; }

    public Zone Hand { get; }

    public Zone Base { get; }

    public Zone Hidden { get; }

    public Zone Trash { get; }

    public Zone Banish { get; }

    public RuneZone RuneDeck { get; }

    public RuneZone RunesInBase { get; }

    public RunePool RunePool { get; }

    public IReadOnlyList<Zone> CardZones =>
    [
        MainDeck,
        Hand,
        Base,
        Hidden,
        Trash,
        Banish
    ];
}
