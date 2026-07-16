using Riftbounder.Core.Effects;

namespace Riftbounder.Engine.Effects;

public sealed record InstructionResolutionRecord(
    SpellInstructionDefinition Instruction,
    IReadOnlyList<ResolvedInstructionTarget> Targets,
    InstructionExecutionResult Result);
