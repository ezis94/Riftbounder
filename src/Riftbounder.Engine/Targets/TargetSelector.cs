using Riftbounder.Core.Cards;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Targets;

public sealed class TargetSelector
{
    private readonly BoardState _boardState;

    public TargetSelector(BoardState boardState)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        _boardState = boardState;
    }

    public TargetSelectionResult Select(
        Card target,
        TargetRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(requirement);

        if (!_boardState.TryGet(
                target.Id,
                out BoardCardState? targetState)
            || targetState is null)
        {
            return TargetSelectionResult.Failure(
                "The target does not exist in the current game state.");
        }

        if (!MatchesKind(target, requirement.Kind))
        {
            return TargetSelectionResult.Failure(
                $"The target is not a valid {requirement.Kind}.");
        }

        if (requirement.MustBeAtBattlefield
            && targetState.Position is not CardPosition.Battlefield)
        {
            return TargetSelectionResult.Failure(
                "The target must be at a battlefield.");
        }

        TargetSnapshot snapshot = new(
            target.Id,
            requirement);

        PowerRequirement? addedCost =
            targetState.HasDeflect
                ? PowerRequirement.Any(1)
                : null;

        return TargetSelectionResult.Success(
            snapshot,
            addedCost);
    }

    private static bool MatchesKind(
        Card target,
        TargetKind kind) =>
        kind switch
        {
            TargetKind.Card => true,
            TargetKind.Unit =>
                target.Definition.CardType is CardType.Unit
                    or CardType.Champion,
            _ => false
        };
}
