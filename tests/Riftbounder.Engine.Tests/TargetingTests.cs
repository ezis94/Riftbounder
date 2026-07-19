using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class TargetingTests
{
    [Fact]
    public void Select_UnitAtBattlefield_Succeeds()
    {
        TestContext context = CreateUnit(
            CardPosition.Battlefield);
        TargetSelector selector = new(
            context.Board);
        TargetRequirement requirement = new(
            TargetKind.Unit,
            mustBeAtBattlefield: true);

        TargetSelectionResult result =
            selector.Select(
                context.Card,
                requirement);

        Assert.True(result.Succeeded);
        Assert.Equal(
            context.Card.Id,
            result.Snapshot?.CardId);
        Assert.Null(result.AddedCost);
    }

    [Fact]
    public void Select_UnitAtBase_FailsBattlefieldRequirement()
    {
        TestContext context = CreateUnit(
            CardPosition.Base);
        TargetSelector selector = new(
            context.Board);

        TargetSelectionResult result =
            selector.Select(
                context.Card,
                new TargetRequirement(
                    TargetKind.Unit,
                    mustBeAtBattlefield: true));

        Assert.False(result.Succeeded);
        Assert.NotNull(result.FailureReason);
        Assert.Contains(
            "battlefield",
            result.FailureReason,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Select_DeflectUnit_AddsAnyDomainPowerCost()
    {
        TestContext context = CreateUnit(
            CardPosition.Battlefield,
            hasDeflect: true);
        TargetSelector selector = new(
            context.Board);

        TargetSelectionResult result =
            selector.Select(
                context.Card,
                new TargetRequirement(
                    TargetKind.Unit,
                    mustBeAtBattlefield: true));

        Assert.True(result.Succeeded);
        Assert.True(result.AddedCost.HasValue);
        PowerRequirement added =
            result.AddedCost.Value;
        Assert.True(added.AcceptsAnyDomain);
        Assert.Equal(1, added.Amount);
    }

    [Fact]
    public void Resolve_TargetGainsDeflect_RemainsEligible()
    {
        TestContext context = CreateUnit(
            CardPosition.Battlefield);
        TargetSnapshot snapshot = new(
            context.Card.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));
        context.Board.Get(
            context.Card.Id).SetDeflect(true);

        TargetResolutionResult result =
            new TargetResolver(
                context.Board).Resolve(snapshot);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public void Resolve_TargetMovesToBase_BecomesWrongLocation()
    {
        TestContext context = CreateUnit(
            CardPosition.Battlefield);
        TargetSnapshot snapshot = new(
            context.Card.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));
        context.Board.Get(
            context.Card.Id).MoveTo(
                CardPosition.Base);

        TargetResolutionResult result =
            new TargetResolver(
                context.Board).Resolve(snapshot);

        Assert.False(result.IsEligible);
        Assert.Equal(
            TargetResolutionStatus.WrongLocation,
            result.Status);
    }

    [Fact]
    public void Resolve_TargetMissing_IsIneligible()
    {
        BoardState board = new();
        Card missing = CreateUnitCard();

        TargetResolutionResult result =
            new TargetResolver(board).Resolve(
                new TargetSnapshot(
                    missing.Id,
                    new TargetRequirement(
                        TargetKind.Unit)));

        Assert.Equal(
            TargetResolutionStatus.Missing,
            result.Status);
    }

    [Fact]
    public void Apply_DeflectSelection_AddsOneRequirementPerTarget()
    {
        TestContext first = CreateUnit(
            CardPosition.Battlefield,
            hasDeflect: true);
        TestContext second = CreateUnit(
            CardPosition.Battlefield,
            hasDeflect: true);

        TargetSelectionResult firstSelection =
            new TargetSelector(
                first.Board).Select(
                    first.Card,
                    new TargetRequirement(
                        TargetKind.Unit));
        TargetSelectionResult secondSelection =
            new TargetSelector(
                second.Board).Select(
                    second.Card,
                    new TargetRequirement(
                        TargetKind.Unit));

        ResourceCost cost =
            TargetCostModifier.Apply(
                ResourceCost.EnergyOnly(2),
                [
                    firstSelection,
                    secondSelection
                ]);

        Assert.Equal(2, cost.Energy);
        Assert.Equal(
            2,
            cost.PowerRequirements.Count);
        Assert.All(
            cost.PowerRequirements,
            requirement =>
                Assert.True(
                    requirement.AcceptsAnyDomain));
    }

    private static TestContext CreateUnit(
        CardPosition position,
        bool hasDeflect = false)
    {
        Card card = CreateUnitCard();
        BoardState board = new();
        board.Register(
            card,
            position,
            hasDeflect);
        return new TestContext(card, board);
    }

    private static Card CreateUnitCard() =>
        Card.Create(
            new CardDefinition(
                Guid.NewGuid().ToString("N"),
                "Target Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            PlayerId.New());

    private sealed record TestContext(
        Card Card,
        BoardState Board);

    [Fact]
    public void Select_UntargetableCard_IsIllegal()
    {
        Card unit = CreateUnitCard();
        BoardState board = new();
        board.Register(
            unit,
            CardPosition.Battlefield,
            isTargetable: false);

        TargetSelectionResult result = new TargetSelector(board).Select(
            unit,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

        Assert.False(result.Succeeded);
        Assert.Contains("untargetable", result.FailureReason ?? string.Empty);
    }

    [Fact]
    public void Resolve_TargetBecomesUntargetable_Mistargets()
    {
        Card unit = CreateUnitCard();
        BoardState board = new();
        board.Register(unit, CardPosition.Battlefield);
        TargetSnapshot snapshot = SelectBattlefieldUnit(board, unit);

        board.Get(unit.Id).SetTargetable(false);

        TargetResolutionResult result =
            new TargetResolver(board).Resolve(snapshot);

        Assert.Equal(TargetResolutionStatus.Untargetable, result.Status);
        Assert.False(result.IsEligible);
    }

    [Fact]
    public void Resolve_TargetBecomesTargetableAgain_IsEligible()
    {
        Card unit = CreateUnitCard();
        BoardState board = new();
        board.Register(unit, CardPosition.Battlefield);
        TargetSnapshot snapshot = SelectBattlefieldUnit(board, unit);

        board.Get(unit.Id).SetTargetable(false);
        board.Get(unit.Id).SetTargetable(true);

        TargetResolutionResult result =
            new TargetResolver(board).Resolve(snapshot);

        Assert.Equal(TargetResolutionStatus.Eligible, result.Status);
        Assert.True(result.IsEligible);
    }

    private static TargetSnapshot SelectBattlefieldUnit(
        BoardState board,
        Card unit)
    {
        TargetSelectionResult selection = new TargetSelector(board).Select(
            unit,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

        return Assert.IsType<TargetSnapshot>(selection.Snapshot);
    }
}
