using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Effects;

public sealed class ExhaustInstructionHandler :
    ISpellInstructionHandler
{
    public const string Id = "exhaust";

    private readonly BoardState _boardState;

    public ExhaustInstructionHandler(BoardState boardState)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        _boardState = boardState;
    }

    public string InstructionId => Id;

    public InstructionExecutionResult Execute(
        InstructionExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int exhaustedCount = 0;

        foreach (ResolvedInstructionTarget target
                 in context.Targets.Where(target => target.IsEligible))
        {
            if (target.Target is null)
            {
                continue;
            }

            BoardCardState state =
                _boardState.Get(target.Target.Id);

            if (state.Exhaust())
            {
                exhaustedCount++;
            }
        }

        return InstructionExecutionResult.Success(exhaustedCount);
    }
}
