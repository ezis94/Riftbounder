namespace Riftbounder.Engine.Effects;

public interface ISpellInstructionHandler
{
    string InstructionId { get; }

    InstructionExecutionResult Execute(
        InstructionExecutionContext context);
}
