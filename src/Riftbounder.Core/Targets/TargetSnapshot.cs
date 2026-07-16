using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Targets;

public sealed record TargetSnapshot
{
    public TargetSnapshot(
        CardId cardId,
        TargetRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);

        CardId = cardId;
        Requirement = requirement;
    }

    public CardId CardId { get; }

    public TargetRequirement Requirement { get; }
}
