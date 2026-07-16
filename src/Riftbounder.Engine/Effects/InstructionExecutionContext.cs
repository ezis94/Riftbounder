using Riftbounder.Core.Effects;
using Riftbounder.Engine.Cards;

namespace Riftbounder.Engine.Effects;

public sealed record InstructionExecutionContext(
    PlayCardChainItem Spell,
    SpellInstructionDefinition Instruction,
    IReadOnlyList<ResolvedInstructionTarget> Targets);
