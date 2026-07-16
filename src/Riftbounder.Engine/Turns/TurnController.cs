using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Results;

namespace Riftbounder.Engine.Turns;

public sealed class TurnController
{
    private readonly Game _game;
    private readonly IReadOnlyList<PlayerId> _turnOrder;
    private readonly EventJournal _journal;
    private readonly TimeProvider _timeProvider;
    private int _turnPlayerIndex;
    private readonly Dictionary<PlayerId, int> _turnsStartedByPlayer = [];

    public TurnController(
        Game game,
        IReadOnlyList<PlayerId> turnOrder,
        EventJournal journal,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(turnOrder);
        ArgumentNullException.ThrowIfNull(journal);

        if (turnOrder.Count != 2)
        {
            throw new ArgumentException("Sprint 2 supports exactly two players.", nameof(turnOrder));
        }

        if (turnOrder.Distinct().Count() != turnOrder.Count)
        {
            throw new ArgumentException("Turn order cannot contain duplicate players.", nameof(turnOrder));
        }

        foreach (PlayerId playerId in turnOrder)
        {
            _ = game.GetPlayer(playerId);
        }

        _game = game;
        _turnOrder = turnOrder.ToArray();
        _journal = journal;
        _timeProvider = timeProvider ?? TimeProvider.System;
        State = new TurnState();
    }

    public TurnState State { get; }

    public void Start()
    {
        if (State.HasStarted)
        {
            throw new InvalidOperationException("The game turn sequence has already started.");
        }

        _turnPlayerIndex = 0;
        State.TurnNumber = 1;
        State.TurnPlayerId = _turnOrder[_turnPlayerIndex];
        RecordTurnStarted(RequireTurnPlayer());

        TransitionTo(TurnPhase.Awaken);
        _journal.Append(new TurnStartedEvent(
            State.TurnNumber,
            RequireTurnPlayer(),
            GetUtcNow()));
    }

    public void AdvanceStartOfTurn()
    {
        EnsureStarted();

        switch (State.Phase)
        {
            case TurnPhase.Awaken:
                ResolveAwaken();
                TransitionTo(TurnPhase.Beginning);
                break;

            case TurnPhase.Beginning:
                TransitionTo(TurnPhase.Channel);
                ResolveChannel();
                break;

            case TurnPhase.Channel:
                TransitionTo(TurnPhase.Draw);
                ResolveTurnDraw();
                EmptyAllRunePools("EndOfDrawPhase");
                break;

            case TurnPhase.Draw:
                TransitionTo(TurnPhase.Main);
                SetPriority(RequireTurnPlayer());
                break;

            default:
                throw new InvalidOperationException(
                    $"Cannot advance start-of-turn processing from phase '{State.Phase}'.");
        }
    }

    public void AdvanceToMainPhase()
    {
        EnsureStarted();

        while (State.Phase is TurnPhase.Awaken
               or TurnPhase.Beginning
               or TurnPhase.Channel
               or TurnPhase.Draw)
        {
            AdvanceStartOfTurn();
        }

        if (State.Phase is not TurnPhase.Main)
        {
            throw new InvalidOperationException(
                $"Cannot advance to Main Phase from phase '{State.Phase}'.");
        }
    }

    public void EndMainPhase()
    {
        EnsurePhase(TurnPhase.Main);

        SetPriority(null);
        TransitionTo(TurnPhase.Ending);
    }

    public void CompleteEndingPhase()
    {
        EnsurePhase(TurnPhase.Ending);

        EmptyAllRunePools("EndOfTurn");

        _turnPlayerIndex = (_turnPlayerIndex + 1) % _turnOrder.Count;
        State.TurnNumber++;
        State.TurnPlayerId = _turnOrder[_turnPlayerIndex];
        RecordTurnStarted(RequireTurnPlayer());

        TransitionTo(TurnPhase.Awaken);
        _journal.Append(new TurnStartedEvent(
            State.TurnNumber,
            RequireTurnPlayer(),
            GetUtcNow()));
    }

    private void ResolveAwaken()
    {
        PlayerId playerId = RequireTurnPlayer();

        foreach (Rune rune in _game.ReadyAllRunes(playerId))
        {
            _journal.Append(new RuneReadiedEvent(
                State.TurnNumber,
                playerId,
                rune,
                GetUtcNow()));
        }
    }

    private void ResolveChannel()
    {
        PlayerId playerId = RequireTurnPlayer();
        int requestedCount = GetChannelCount();

        _journal.Append(new ChannelRunesRequestedEvent(
            State.TurnNumber,
            playerId,
            requestedCount,
            GetUtcNow()));

        ChannelResult result = _game.ChannelRunes(playerId, requestedCount);

        foreach (Rune rune in result.ChanneledRunes)
        {
            _journal.Append(new RuneChanneledEvent(
                State.TurnNumber,
                playerId,
                rune,
                rune.IsReady,
                GetUtcNow()));
        }
    }


private void EmptyAllRunePools(string checkpoint)
{
    foreach(Player player in _game.Players)
    {
        if(!_game.EmptyRunePool(player.Id)) continue;
        _journal.Append(new RunePoolEmptiedEvent(State.TurnNumber,player.Id,checkpoint,GetUtcNow()));
    }
}

    private void ResolveTurnDraw()
    {
        if (ShouldSkipTurnDraw())
        {
            return;
        }

        PlayerId playerId = RequireTurnPlayer();
        DrawResult result = _game.DrawCard(playerId);

        if (result.Succeeded && result.Card is not null)
        {
            _journal.Append(new CardDrawnEvent(
                State.TurnNumber,
                playerId,
                result.Card,
                GetUtcNow()));
            return;
        }

        _journal.Append(new EmptyDeckDrawAttemptedEvent(
            State.TurnNumber,
            playerId,
            GetUtcNow()));
    }

    private bool ShouldSkipTurnDraw() =>
        GetTurnsStarted(RequireTurnPlayer()) == 1 && _turnPlayerIndex == 0;

    private int GetChannelCount() =>
        GetTurnsStarted(RequireTurnPlayer()) == 1 && _turnPlayerIndex == 1 ? 3 : 2;

    private void RecordTurnStarted(PlayerId playerId)
    {
        _turnsStartedByPlayer[playerId] = GetTurnsStarted(playerId) + 1;
    }

    private int GetTurnsStarted(PlayerId playerId) =>
        _turnsStartedByPlayer.GetValueOrDefault(playerId);

    private void TransitionTo(TurnPhase nextPhase)
    {
        TurnPhase previousPhase = State.Phase;
        State.Phase = nextPhase;

        _journal.Append(new TurnPhaseChangedEvent(
            State.TurnNumber,
            RequireTurnPlayer(),
            previousPhase,
            nextPhase,
            GetUtcNow()));
    }

    private void SetPriority(PlayerId? playerId)
    {
        PlayerId? previousPlayerId = State.PriorityPlayerId;
        State.PriorityPlayerId = playerId;

        if (previousPlayerId != playerId)
        {
            _journal.Append(new PriorityChangedEvent(
                previousPlayerId,
                playerId,
                GetUtcNow()));
        }
    }

    private PlayerId RequireTurnPlayer() =>
        State.TurnPlayerId
        ?? throw new InvalidOperationException("The turn player has not been assigned.");

    private void EnsureStarted()
    {
        if (!State.HasStarted)
        {
            throw new InvalidOperationException("The turn sequence has not started.");
        }
    }

    private void EnsurePhase(TurnPhase expectedPhase)
    {
        EnsureStarted();

        if (State.Phase != expectedPhase)
        {
            throw new InvalidOperationException(
                $"Expected phase '{expectedPhase}', but current phase is '{State.Phase}'.");
        }
    }

    private DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();
}
