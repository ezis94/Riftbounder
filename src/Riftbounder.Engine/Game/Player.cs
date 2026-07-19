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
        LegendZone = new Zone(
            id,
            ZoneKind.Legend,
            $"{name}'s Legend Zone");
        ChampionZone = new Zone(
            id,
            ZoneKind.Champion,
            $"{name}'s Champion Zone",
            maximumOccupancy: 1);
        Trash = new Zone(
            id,
            ZoneKind.Trash,
            $"{name}'s Trash");
        Banishment = new Zone(
            id,
            ZoneKind.Banishment,
            $"{name}'s Banishment");

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

    public Zone LegendZone { get; }

    public Zone ChampionZone { get; }

    public Zone Trash { get; }

    public Zone Banishment { get; }

    public RuneZone RuneDeck { get; }

    public RuneZone RunesInBase { get; }

    public RunePool RunePool { get; }

    public IReadOnlyList<Zone> CardZones =>
    [
        MainDeck,
        Hand,
        Base,
        LegendZone,
        ChampionZone,
        Trash,
        Banishment
    ];
}
