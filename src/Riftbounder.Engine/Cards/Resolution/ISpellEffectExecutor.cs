using Riftbounder.Engine.Cards;

namespace Riftbounder.Engine.Cards.Resolution;

public interface ISpellEffectExecutor
{
    SpellExecutionResult Execute(PlayCardChainItem spell);
}
