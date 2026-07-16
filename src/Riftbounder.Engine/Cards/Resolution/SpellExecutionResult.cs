namespace Riftbounder.Engine.Cards.Resolution;

public sealed record SpellExecutionResult
{
    private SpellExecutionResult(bool succeeded, string? failureReason)
    {
        if (!succeeded && string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "A failed spell execution requires a reason.",
                nameof(failureReason));
        }

        Succeeded = succeeded;
        FailureReason = failureReason;
    }

    public bool Succeeded { get; }

    public string? FailureReason { get; }

    public static SpellExecutionResult Success() =>
        new(true, null);

    public static SpellExecutionResult Failure(string reason) =>
        new(false, reason);
}
