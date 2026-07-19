using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Board;

public sealed class GameBoard
{
    private readonly IReadOnlyDictionary<PlayerId, Zone> _bases;
    private readonly IReadOnlyDictionary<ZoneId, Zone> _facedownZones;
    private readonly IReadOnlyList<Zone> _baseZones;
    private readonly IReadOnlyList<Zone> _locations;

    public GameBoard(
        Player firstPlayer,
        Player secondPlayer)
    {
        ArgumentNullException.ThrowIfNull(firstPlayer);
        ArgumentNullException.ThrowIfNull(secondPlayer);

        if (firstPlayer.Id == secondPlayer.Id)
        {
            throw new ArgumentException(
                "The board requires two distinct players.");
        }

        _bases = new Dictionary<PlayerId, Zone>
        {
            [firstPlayer.Id] = firstPlayer.Base,
            [secondPlayer.Id] = secondPlayer.Base
        };
        _baseZones = _bases.Values.ToArray();

        Battlefields =
        [
            new Zone(
                ZoneId.New(),
                ownerId: null,
                ZoneKind.Battlefield,
                "Battlefield 1"),
            new Zone(
                ZoneId.New(),
                ownerId: null,
                ZoneKind.Battlefield,
                "Battlefield 2")
        ];

        _facedownZones = Battlefields.ToDictionary(
            battlefield => battlefield.Id,
            battlefield => new Zone(
                ZoneId.New(),
                ownerId: null,
                ZoneKind.Facedown,
                $"{battlefield.Name} Facedown Zone",
                maximumOccupancy: 1));

        _locations = [.. _baseZones, .. Battlefields];
    }

    public IReadOnlyCollection<Zone> Bases => _baseZones;

    public IReadOnlyList<Zone> Battlefields { get; }

    public IReadOnlyCollection<Zone> FacedownZones =>
        _facedownZones.Values.ToArray();

    public IReadOnlyList<Zone> Locations => _locations;

    public Zone GetBase(PlayerId playerId) =>
        _bases.TryGetValue(playerId, out Zone? zone)
            ? zone
            : throw new KeyNotFoundException(
                $"Player {playerId} does not have a Base on this board.");

    public Zone GetBattlefield(int number)
    {
        if (number is < 1 or > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(number),
                "Battlefield number must be 1 or 2.");
        }

        return Battlefields[number - 1];
    }

    public Zone GetFacedownZone(int battlefieldNumber) =>
        GetFacedownZone(GetBattlefield(battlefieldNumber).Id);

    public Zone GetFacedownZone(ZoneId battlefieldId) =>
        _facedownZones.TryGetValue(
            battlefieldId,
            out Zone? zone)
                ? zone
                : throw new KeyNotFoundException(
                    $"Battlefield {battlefieldId} does not have a Facedown Zone on this board.");

    public bool IsBattlefield(ZoneId zoneId) =>
        Battlefields.Any(zone => zone.Id == zoneId);

    public bool IsBase(ZoneId zoneId) =>
        _baseZones.Any(zone => zone.Id == zoneId);
}
