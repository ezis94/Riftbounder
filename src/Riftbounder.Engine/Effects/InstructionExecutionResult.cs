namespace Riftbounder.Engine.Effects;

public sealed record InstructionExecutionResult
{
    private InstructionExecutionResult(
        bool succeeded,
        int affectedTargetCount,
        string? failureReason)
    {
        if (affectedTargetCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(affectedTargetCount));
        }

        if (!succeeded && string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "A failed instruction requires a reason.",
                nameof(failureReason));
        }

        Succeeded = succeeded;
        AffectedTargetCount = affectedTargetCount;
        FailureReason = failureReason;
    }

    public bool Succeeded { get; }

    public int AffectedTargetCount { get; }

    public string? FailureReason { get; }

    public static InstructionExecutionResult Success(
        int affectedTargetCount = 0) =>
        new(true, affectedTargetCount, null);

    public static InstructionExecutionResult Failure(
        string reason) =>
        new(false, 0, reason);
}
