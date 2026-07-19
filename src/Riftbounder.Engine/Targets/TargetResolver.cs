using Riftbounder.Core.Cards;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Targets;

public sealed class TargetResolver
{
    private readonly BoardState _boardState;

    public TargetResolver(BoardState boardState)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        _boardState = boardState;
    }

    public TargetResolutionResult Resolve(
        TargetSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!_boardState.TryGet(
                snapshot.CardId,
                out BoardCardState? targetState)
            || targetState is null)
        {
            return new TargetResolutionResult(
                TargetResolutionStatus.Missing,
                null);
        }

        if (!MatchesKind(
                targetState.Card,
                snapshot.Requirement.Kind))
        {
            return new TargetResolutionResult(
                TargetResolutionStatus.WrongKind,
                targetState.Card);
        }

        if (snapshot.Requirement.MustBeAtBattlefield
            && targetState.Position is not CardPosition.Battlefield)
        {
            return new TargetResolutionResult(
                TargetResolutionStatus.WrongLocation,
                targetState.Card);
        }

        if (!targetState.IsTargetable)
        {
            return new TargetResolutionResult(
                TargetResolutionStatus.Untargetable,
                targetState.Card);
        }

        // Deflect is deliberately not checked here. It modifies the cost of
        // choosing the target and has no effect after finalization.
        return new TargetResolutionResult(
            TargetResolutionStatus.Eligible,
            targetState.Card);
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
