using Riftbounder.Core.Cards;

namespace Riftbounder.Engine.Targets;

public sealed record TargetResolutionResult(
    TargetResolutionStatus Status,
    Card? Target)
{
    public bool IsEligible =>
        Status is TargetResolutionStatus.Eligible;
}
