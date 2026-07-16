namespace Riftbounder.Engine.Effects;

public sealed class AffectEligibleTargetsHandler :
    ISpellInstructionHandler
{
    private readonly Action<ResolvedInstructionTarget> _effect;

    public AffectEligibleTargetsHandler(
        string instructionId,
        Action<ResolvedInstructionTarget> effect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instructionId);
        ArgumentNullException.ThrowIfNull(effect);

        InstructionId = instructionId;
        _effect = effect;
    }

    public string InstructionId { get; }

    public InstructionExecutionResult Execute(
        InstructionExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int affected = 0;

        foreach (ResolvedInstructionTarget target
                 in context.Targets.Where(target => target.IsEligible))
        {
            _effect(target);
            affected++;
        }

        return InstructionExecutionResult.Success(affected);
    }
}
