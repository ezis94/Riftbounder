namespace Riftbounder.Engine.Effects;

public sealed record SpellEffectExecutionReport(
    IReadOnlyList<InstructionResolutionRecord> Instructions,
    bool Succeeded,
    string? FailureReason);
