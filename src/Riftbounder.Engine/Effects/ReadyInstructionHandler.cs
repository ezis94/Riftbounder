using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Effects;

public sealed class ReadyInstructionHandler :
    ISpellInstructionHandler
{
    public const string Id = "ready";

    private readonly BoardState _boardState;

    public ReadyInstructionHandler(BoardState boardState)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        _boardState = boardState;
    }

    public string InstructionId => Id;

    public InstructionExecutionResult Execute(
        InstructionExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        int readiedCount = 0;

        foreach (ResolvedInstructionTarget target
                 in context.Targets.Where(target => target.IsEligible))
        {
            if (target.Target is null)
            {
                continue;
            }

            BoardCardState state =
                _boardState.Get(target.Target.Id);

            if (state.Ready())
            {
                readiedCount++;
            }
        }

        return InstructionExecutionResult.Success(readiedCount);
    }
}
