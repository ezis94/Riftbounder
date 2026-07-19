using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Board;

public sealed class GameBoard
{
    private readonly IReadOnlyDictionary<PlayerId, Zone> _bases;

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
    }

    public IReadOnlyCollection<Zone> Bases =>
        _bases.Values.ToArray();

    public IReadOnlyList<Zone> Battlefields { get; }

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

    public bool IsBattlefield(ZoneId zoneId) =>
        Battlefields.Any(zone => zone.Id == zoneId);

    public bool IsBase(ZoneId zoneId) =>
        Bases.Any(zone => zone.Id == zoneId);

    public IReadOnlyList<Zone> Locations =>
        [.. Bases, .. Battlefields];
}
