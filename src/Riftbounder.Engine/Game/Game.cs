using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Results;

namespace Riftbounder.Engine.Games;

public sealed class Game
{
    private readonly Dictionary<PlayerId, Player> _players;
    private readonly List<Zone> _registeredZones;
    private readonly List<RuneZone> _registeredRuneZones;

    public Game(Player firstPlayer, Player secondPlayer)
    {
        ArgumentNullException.ThrowIfNull(firstPlayer);
        ArgumentNullException.ThrowIfNull(secondPlayer);

        if (firstPlayer.Id == secondPlayer.Id)
        {
            throw new ArgumentException("A game requires two distinct players.");
        }

        _players = new Dictionary<PlayerId, Player>
        {
            [firstPlayer.Id] = firstPlayer,
            [secondPlayer.Id] = secondPlayer
        };

        _registeredZones =
        [
            firstPlayer.MainDeck,
            firstPlayer.Hand,
            firstPlayer.Trash,
            secondPlayer.MainDeck,
            secondPlayer.Hand,
            secondPlayer.Trash
        ];

        _registeredRuneZones =
        [
            firstPlayer.RuneDeck,
            firstPlayer.RunesInBase,
            secondPlayer.RuneDeck,
            secondPlayer.RunesInBase
        ];
    }

    public IReadOnlyCollection<Player> Players => _players.Values;

    public Player GetPlayer(PlayerId playerId) =>
        _players.TryGetValue(playerId, out Player? player)
            ? player
            : throw new KeyNotFoundException(
                $"Player {playerId} is not part of this game.");

    public DrawResult DrawCard(PlayerId playerId)
    {
        Player player = GetPlayer(playerId);
        Card? topCard = player.MainDeck.PeekTop();

        if (topCard is null)
        {
            return DrawResult.EmptyDeck;
        }

        TransferCard(topCard.Id, player.MainDeck, player.Hand);
        return DrawResult.Success(topCard);
    }

    public ChannelResult ChannelRunes(
        PlayerId playerId,
        int count,
        bool enterExhausted = false)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Channel count cannot be negative.");
        }

        Player player = GetPlayer(playerId);
        List<Rune> channeledRunes = [];

        for (int index = 0; index < count; index++)
        {
            Rune? topRune = player.RuneDeck.PeekTop();
            if (topRune is null)
            {
                break;
            }

            TransferRune(topRune.Id, player.RuneDeck, player.RunesInBase);

            if (enterExhausted)
            {
                topRune.Exhaust();
            }
            else
            {
                topRune.Ready();
            }

            channeledRunes.Add(topRune);
        }

        return new ChannelResult(count, channeledRunes);
    }

    public IReadOnlyList<Rune> ReadyAllRunes(PlayerId playerId)
    {
        Player player = GetPlayer(playerId);
        List<Rune> readiedRunes = [];

        foreach (Rune rune in player.RunesInBase.Runes)
        {
            if (rune.IsReady)
            {
                continue;
            }

            rune.Ready();
            readiedRunes.Add(rune);
        }

        return readiedRunes;
    }

    public void RegisterCard(Card card, Zone destination)
    {
        ArgumentNullException.ThrowIfNull(card);
        EnsureRegisteredZone(destination);

        if (FindZoneContaining(card.Id) is not null)
        {
            throw new InvalidOperationException(
                $"Card {card.Id} is already registered in this game.");
        }

        destination.AddToTop(card);
    }

    public void RegisterRune(Rune rune, RuneZone destination)
    {
        ArgumentNullException.ThrowIfNull(rune);
        EnsureRegisteredRuneZone(destination);

        if (FindRuneZoneContaining(rune.Id) is not null)
        {
            throw new InvalidOperationException(
                $"Rune {rune.Id} is already registered in this game.");
        }

        destination.AddToTop(rune);
    }

    public void TransferCard(CardId cardId, Zone source, Zone destination)
    {
        EnsureRegisteredZone(source);
        EnsureRegisteredZone(destination);

        if (ReferenceEquals(source, destination))
        {
            throw new ArgumentException(
                "Source and destination zones must be different.");
        }

        if (FindZoneContaining(cardId) is { } actualZone
            && !ReferenceEquals(actualZone, source))
        {
            throw new InvalidOperationException(
                $"Card {cardId} is in '{actualZone.Name}', not the supplied source zone '{source.Name}'.");
        }

        if (!source.Remove(cardId, out Card? card) || card is null)
        {
            throw new InvalidOperationException(
                $"Card {cardId} is not in source zone '{source.Name}'.");
        }

        try
        {
            destination.AddToTop(card);
        }
        catch
        {
            source.AddToTop(card);
            throw;
        }
    }

    public void TransferRune(
        RuneId runeId,
        RuneZone source,
        RuneZone destination)
    {
        EnsureRegisteredRuneZone(source);
        EnsureRegisteredRuneZone(destination);

        if (ReferenceEquals(source, destination))
        {
            throw new ArgumentException(
                "Source and destination rune zones must be different.");
        }

        if (FindRuneZoneContaining(runeId) is { } actualZone
            && !ReferenceEquals(actualZone, source))
        {
            throw new InvalidOperationException(
                $"Rune {runeId} is in '{actualZone.Name}', not the supplied source zone '{source.Name}'.");
        }

        if (!source.Remove(runeId, out Rune? rune) || rune is null)
        {
            throw new InvalidOperationException(
                $"Rune {runeId} is not in source rune zone '{source.Name}'.");
        }

        try
        {
            destination.AddToTop(rune);
        }
        catch
        {
            source.AddToTop(rune);
            throw;
        }
    }

    public Zone? FindZoneContaining(CardId cardId) =>
        _registeredZones.SingleOrDefault(zone => zone.Contains(cardId));

    public RuneZone? FindRuneZoneContaining(RuneId runeId) =>
        _registeredRuneZones.SingleOrDefault(zone => zone.Contains(runeId));

    private void EnsureRegisteredZone(Zone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (!_registeredZones.Contains(zone))
        {
            throw new InvalidOperationException(
                $"Zone '{zone.Name}' is not registered with this game.");
        }
    }

    private void EnsureRegisteredRuneZone(RuneZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (!_registeredRuneZones.Contains(zone))
        {
            throw new InvalidOperationException(
                $"Rune zone '{zone.Name}' is not registered with this game.");
        }
    }
}
