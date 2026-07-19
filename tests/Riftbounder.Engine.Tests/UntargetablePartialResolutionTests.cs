using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class UntargetablePartialResolutionTests
{
    [Fact]
    public void Execute_OneTargetBecomesUntargetable_AffectsOnlyEligibleTarget()
    {
        PlayerId playerId = PlayerId.New();
        Card first = CreateUnit(playerId, "first");
        Card second = CreateUnit(playerId, "second");
        BoardState board = new();
        board.Register(first, CardPosition.Battlefield);
        board.Register(second, CardPosition.Battlefield);

        TargetRequirement requirement = new(
            TargetKind.Unit,
            mustBeAtBattlefield: true);
        TargetSnapshot firstSnapshot = new(first.Id, requirement);
        TargetSnapshot secondSnapshot = new(second.Id, requirement);
        board.Get(second.Id).SetTargetable(false);

        Card spell = Card.Create(
            new CardDefinition(
                "spell",
                "Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0),
                [new SpellInstructionDefinition("affect", [0, 1])]),
            playerId);
        PlayCardChainItem item = PlayCardChainItem.Create(
            playerId,
            spell,
            new ResourcePayment(0),
            [firstSnapshot, secondSnapshot]);
        List<Card> affectedCards = [];
        AffectEligibleTargetsHandler handler = new(
            "affect",
            target =>
                affectedCards.Add(
                    Assert.IsType<Card>(target.Target)));
        PartialSpellEffectExecutor executor = new(
            new TargetResolver(board),
            [handler]);

        var execution = executor.Execute(item);
        SpellEffectExecutionReport report =
            Assert.IsType<SpellEffectExecutionReport>(
                executor.LastReport);

        Assert.True(execution.Succeeded);
        Assert.True(report.Succeeded);
        Assert.Single(affectedCards);
        Assert.Same(first, affectedCards[0]);
        Assert.Equal(
            TargetResolutionStatus.Untargetable,
            report.Instructions[0].Targets[1].Status);
    }

    private static Card CreateUnit(
        PlayerId ownerId,
        string id) =>
        Card.Create(
            new CardDefinition(
                id,
                id,
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            ownerId);
}
