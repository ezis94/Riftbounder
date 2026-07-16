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

public sealed class ReadyExhaustInstructionHandlerTests
{
    [Fact]
    public void Exhaust_EligibleReadyTarget_ExhaustsIt()
    {
        TestContext context = CreateContext(isReady: true);
        ExhaustInstructionHandler handler = new(context.Board);

        InstructionExecutionResult result =
            handler.Execute(
                context.CreateExecutionContext(
                    ExhaustInstructionHandler.Id));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.AffectedTargetCount);
        Assert.False(context.State.IsReady);
    }

    [Fact]
    public void Exhaust_AlreadyExhaustedTarget_SucceedsWithoutChange()
    {
        TestContext context = CreateContext(isReady: false);
        ExhaustInstructionHandler handler = new(context.Board);

        InstructionExecutionResult result =
            handler.Execute(
                context.CreateExecutionContext(
                    ExhaustInstructionHandler.Id));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.AffectedTargetCount);
        Assert.False(context.State.IsReady);
    }

    [Fact]
    public void Ready_EligibleExhaustedTarget_ReadiesIt()
    {
        TestContext context = CreateContext(isReady: false);
        ReadyInstructionHandler handler = new(context.Board);

        InstructionExecutionResult result =
            handler.Execute(
                context.CreateExecutionContext(
                    ReadyInstructionHandler.Id));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.AffectedTargetCount);
        Assert.True(context.State.IsReady);
    }

    [Fact]
    public void Ready_AlreadyReadyTarget_SucceedsWithoutChange()
    {
        TestContext context = CreateContext(isReady: true);
        ReadyInstructionHandler handler = new(context.Board);

        InstructionExecutionResult result =
            handler.Execute(
                context.CreateExecutionContext(
                    ReadyInstructionHandler.Id));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.AffectedTargetCount);
        Assert.True(context.State.IsReady);
    }

    [Fact]
    public void Exhaust_TargetMovedToBase_IsSkipped()
    {
        TestContext context = CreateContext(isReady: true);
        context.State.MoveTo(CardPosition.Base);
        TargetResolutionResult resolution =
            new TargetResolver(context.Board).Resolve(
                context.Snapshot);

        ExhaustInstructionHandler handler = new(context.Board);
        InstructionExecutionResult result =
            handler.Execute(
                context.CreateExecutionContext(
                    ExhaustInstructionHandler.Id,
                    resolution));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.AffectedTargetCount);
        Assert.True(context.State.IsReady);
    }

    private static TestContext CreateContext(bool isReady)
    {
        Card unit = Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            PlayerId.New());

        BoardState board = new();
        board.Register(
            unit,
            CardPosition.Battlefield,
            isReady: isReady);

        TargetSnapshot snapshot = new(
            unit.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

        return new TestContext(
            unit,
            board,
            board.Get(unit.Id),
            snapshot);
    }

    private sealed record TestContext(
        Card Unit,
        BoardState Board,
        BoardCardState State,
        TargetSnapshot Snapshot)
    {
        public InstructionExecutionContext CreateExecutionContext(
            string instructionId,
            TargetResolutionResult? resolution = null)
        {
            TargetResolutionResult effectiveResolution =
                resolution
                ?? new TargetResolver(Board).Resolve(Snapshot);

            Card spell = Card.Create(
                new CardDefinition(
                    "spell",
                    "Spell",
                    CardType.Spell,
                    ResourceCost.EnergyOnly(0),
                    [
                        new SpellInstructionDefinition(
                            instructionId,
                            [0])
                    ]),
                PlayerId.New());

            PlayCardChainItem item =
                PlayCardChainItem.Create(
                    spell.OwnerId,
                    spell,
                    new ResourcePayment(0),
                    [Snapshot]);

            return new InstructionExecutionContext(
                item,
                spell.Definition.Instructions[0],
                [
                    new ResolvedInstructionTarget(
                        0,
                        Snapshot,
                        effectiveResolution.Status,
                        effectiveResolution.Target)
                ]);
        }
    }
}
