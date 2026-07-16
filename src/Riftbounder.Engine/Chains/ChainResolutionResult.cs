namespace Riftbounder.Engine.Chains;

public sealed record ChainResolutionResult
{
    private ChainResolutionResult(
        ChainResolutionStatus status,
        string? failureReason)
    {
        if (status is ChainResolutionStatus.Failed
            && string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "A failed resolution requires a reason.",
                nameof(failureReason));
        }

        Status = status;
        FailureReason = failureReason;
    }

    public ChainResolutionStatus Status { get; }

    public string? FailureReason { get; }

    public bool Succeeded => Status is ChainResolutionStatus.Succeeded;

    public static ChainResolutionResult Success() =>
        new(ChainResolutionStatus.Succeeded, null);

    public static ChainResolutionResult Failure(string reason) =>
        new(ChainResolutionStatus.Failed, reason);
}
