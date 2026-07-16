using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;

namespace Riftbounder.Engine.Targets;

public sealed record TargetSelectionResult
{
    private TargetSelectionResult(
        TargetSnapshot? snapshot,
        PowerRequirement? addedCost,
        string? failureReason)
    {
        Snapshot = snapshot;
        AddedCost = addedCost;
        FailureReason = failureReason;
    }

    public TargetSnapshot? Snapshot { get; }

    public PowerRequirement? AddedCost { get; }

    public string? FailureReason { get; }

    public bool Succeeded => Snapshot is not null;

    public static TargetSelectionResult Success(
        TargetSnapshot snapshot,
        PowerRequirement? addedCost = null) =>
        new(snapshot, addedCost, null);

    public static TargetSelectionResult Failure(
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new TargetSelectionResult(null, null, reason);
    }
}
