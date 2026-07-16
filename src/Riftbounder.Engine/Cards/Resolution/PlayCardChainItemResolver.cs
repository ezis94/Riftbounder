using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Cards.Resolution;

public sealed class PlayCardChainItemResolver : IChainItemResolver
{
    private readonly Game _game;
    private readonly ISpellEffectExecutor _spellEffectExecutor;
    private readonly EventJournal _journal;
    private readonly TimeProvider _timeProvider;

    public PlayCardChainItemResolver(
        Game game,
        ISpellEffectExecutor spellEffectExecutor,
        EventJournal journal,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(spellEffectExecutor);
        ArgumentNullException.ThrowIfNull(journal);

        _game = game;
        _spellEffectExecutor = spellEffectExecutor;
        _journal = journal;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public ChainResolutionResult Resolve(IChainItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item is not PlayCardChainItem spell)
        {
            return ChainResolutionResult.Failure(
                $"Unsupported Chain item type '{item.GetType().Name}'.");
        }

        _journal.Append(new SpellResolutionStartedEvent(
            spell.Id,
            spell.ControllerId,
            spell.Card,
            _timeProvider.GetUtcNow()));

        SpellExecutionResult execution =
            _spellEffectExecutor.Execute(spell);

        if (!execution.Succeeded)
        {
            string reason = execution.FailureReason
                ?? "The spell effect executor failed without a reason.";

            _journal.Append(new SpellResolutionFailedEvent(
                spell.Id,
                spell.ControllerId,
                spell.Card,
                reason,
                _timeProvider.GetUtcNow()));

            return ChainResolutionResult.Failure(reason);
        }

        _game.PutResolvedSpellInOwnersTrash(spell.Card);

        _journal.Append(new SpellResolutionCompletedEvent(
            spell.Id,
            spell.ControllerId,
            spell.Card,
            _timeProvider.GetUtcNow()));

        return ChainResolutionResult.Success();
    }
}
