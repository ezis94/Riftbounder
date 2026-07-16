using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Actions;
using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Tests;

public sealed class DeflectPlayCostTests
{
    [Fact]
    public void GetTotalCost_DeflectTarget_AddsAnyDomainPower()
    {
        BoardState board = new();
        Card target = CreateUnit();
        board.Register(
            target,
            CardPosition.Battlefield,
            hasDeflect: true);
        Card spell = CreateSpell();

        ResourceCost total =
            new DeflectAwarePlayCostResolver(
                board).GetTotalCost(
                    spell,
                    [
                        new TargetSnapshot(
                            target.Id,
                            new TargetRequirement(
                                TargetKind.Unit))
                    ]);

        Assert.Single(total.PowerRequirements);
        Assert.True(
            total.PowerRequirements[0]
                .AcceptsAnyDomain);
    }

    [Fact]
    public void GetTotalCost_TargetGainsDeflectAfterSnapshot_AddsCostAtFinalization()
    {
        BoardState board = new();
        Card target = CreateUnit();
        board.Register(
            target,
            CardPosition.Battlefield);
        TargetSnapshot snapshot = new(
            target.Id,
            new TargetRequirement(
                TargetKind.Unit));
        board.Get(target.Id).SetDeflect(true);

        ResourceCost total =
            new DeflectAwarePlayCostResolver(
                board).GetTotalCost(
                    CreateSpell(),
                    [snapshot]);

        Assert.Single(total.PowerRequirements);
    }

    [Fact]
    public void GetTotalCost_TargetLosesDeflectBeforeFinalization_DoesNotAddCost()
    {
        BoardState board = new();
        Card target = CreateUnit();
        board.Register(
            target,
            CardPosition.Battlefield,
            hasDeflect: true);
        TargetSnapshot snapshot = new(
            target.Id,
            new TargetRequirement(
                TargetKind.Unit));
        board.Get(target.Id).SetDeflect(false);

        ResourceCost total =
            new DeflectAwarePlayCostResolver(
                board).GetTotalCost(
                    CreateSpell(),
                    [snapshot]);

        Assert.Empty(total.PowerRequirements);
    }

    private static Card CreateUnit() =>
        Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            PlayerId.New());

    private static Card CreateSpell() =>
        Card.Create(
            new CardDefinition(
                "spell",
                "Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(1)),
            PlayerId.New());
}
