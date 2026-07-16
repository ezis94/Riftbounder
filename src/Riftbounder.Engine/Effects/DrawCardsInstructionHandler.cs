using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Results;

namespace Riftbounder.Engine.Effects;

public sealed class DrawCardsInstructionHandler :
    ISpellInstructionHandler
{
    public const string Id = "draw";

    private readonly Game _game;
    private readonly EventJournal _journal;
    private readonly TimeProvider _timeProvider;

    public DrawCardsInstructionHandler(
        Game game,
        EventJournal journal,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(journal);

        _game = game;
        _journal = journal;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string InstructionId => Id;

    public InstructionExecutionResult Execute(
        InstructionExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PlayerId playerId = context.Spell.ControllerId;
        int drawnCount = 0;

        for (int drawIndex = 1;
             drawIndex <= context.Instruction.Amount;
             drawIndex++)
        {
            DrawResult result = _game.DrawCard(playerId);

            if (!result.Succeeded || result.Card is not Card card)
            {
                _journal.Append(
                    new SpellEmptyDeckDrawAttemptedEvent(
                        context.Spell.Id,
                        playerId,
                        drawIndex,
                        _timeProvider.GetUtcNow()));

                break;
            }

            drawnCount++;
            _journal.Append(
                new SpellCardDrawnEvent(
                    context.Spell.Id,
                    playerId,
                    card,
                    drawIndex,
                    _timeProvider.GetUtcNow()));
        }

        return InstructionExecutionResult.Success(drawnCount);
    }
}
