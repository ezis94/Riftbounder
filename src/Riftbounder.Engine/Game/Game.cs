using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Zones;
using Riftbounder.Engine.Results;

namespace Riftbounder.Engine.Game;

public sealed class Game
{
    private readonly Dictionary<PlayerId, Player> _players;
    private readonly List<Zone> _registeredZones;

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
    }

    public IReadOnlyCollection<Player> Players => _players.Values;

    public Player GetPlayer(PlayerId playerId) =>
        _players.TryGetValue(playerId, out Player? player)
            ? player
            : throw new KeyNotFoundException($"Player {playerId} is not part of this game.");

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

    public void RegisterCard(Card card, Zone destination)
    {
        ArgumentNullException.ThrowIfNull(card);
        EnsureRegisteredZone(destination);

        if (FindZoneContaining(card.Id) is not null)
        {
            throw new InvalidOperationException($"Card {card.Id} is already registered in this game.");
        }

        destination.AddToTop(card);
    }

    public void TransferCard(CardId cardId, Zone source, Zone destination)
    {
        EnsureRegisteredZone(source);
        EnsureRegisteredZone(destination);

        if (ReferenceEquals(source, destination))
        {
            throw new ArgumentException("Source and destination zones must be different.");
        }

        if (FindZoneContaining(cardId) is { } actualZone && !ReferenceEquals(actualZone, source))
        {
            throw new InvalidOperationException(
                $"Card {cardId} is in '{actualZone.Name}', not the supplied source zone '{source.Name}'.");
        }

        if (!source.Remove(cardId, out Card? card) || card is null)
        {
            throw new InvalidOperationException($"Card {cardId} is not in source zone '{source.Name}'.");
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

    public Zone? FindZoneContaining(CardId cardId) =>
        _registeredZones.SingleOrDefault(zone => zone.Contains(cardId));

    private void EnsureRegisteredZone(Zone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (!_registeredZones.Contains(zone))
        {
            throw new InvalidOperationException($"Zone '{zone.Name}' is not registered with this game.");
        }
    }
}
