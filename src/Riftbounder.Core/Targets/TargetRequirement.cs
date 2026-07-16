namespace Riftbounder.Core.Targets;

public sealed record TargetRequirement
{
    public TargetRequirement(
        TargetKind kind,
        bool mustBeAtBattlefield = false)
    {
        Kind = kind;
        MustBeAtBattlefield = mustBeAtBattlefield;
    }

    public TargetKind Kind { get; }

    public bool MustBeAtBattlefield { get; }
}
