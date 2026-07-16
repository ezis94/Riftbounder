using Riftbounder.Engine.Cards;

namespace Riftbounder.Engine.Cards.Resolution;

public sealed class NoOpSpellEffectExecutor : ISpellEffectExecutor
{
    public SpellExecutionResult Execute(PlayCardChainItem spell)
    {
        ArgumentNullException.ThrowIfNull(spell);
        return SpellExecutionResult.Success();
    }
}
