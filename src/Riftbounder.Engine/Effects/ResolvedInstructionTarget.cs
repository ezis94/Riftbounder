using Riftbounder.Core.Cards;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Effects;

public sealed record ResolvedInstructionTarget(
    int TargetIndex,
    TargetSnapshot Snapshot,
    TargetResolutionStatus Status,
    Card? Target)
{
    public bool IsEligible =>
        Status is TargetResolutionStatus.Eligible;
}
