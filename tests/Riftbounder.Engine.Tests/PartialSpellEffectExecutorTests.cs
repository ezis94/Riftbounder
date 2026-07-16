using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class PartialSpellEffectExecutorTests
{
    [Fact]
    public void Execute_AllTargetsEligible_AffectsAllTargets()
    {
        TestContext context = CreateContext();
        Card first = context.RegisterUnit(
            CardPosition.Battlefield);
        Card second = context.RegisterUnit(
            CardPosition.Battlefield);
        List<Card> affected = [];
        PartialSpellEffectExecutor executor =
            context.CreateExecutor(affected);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem(
                [
                    Snapshot(first),
                    Snapshot(second)
                ]));

        Assert.True(result.Succeeded);
        Assert.Equal([first, second], affected);
        InstructionResolutionRecord record =
            Assert.Single(executor.LastReport!.Instructions);
        Assert.Equal(2, record.Result.AffectedTargetCount);
    }

    [Fact]
    public void Execute_OneTargetMovesToBase_AffectsRemainingTarget()
    {
        TestContext context = CreateContext();
        Card valid = context.RegisterUnit(
            CardPosition.Battlefield);
        Card moved = context.RegisterUnit(
            CardPosition.Battlefield);
        TargetSnapshot validSnapshot = Snapshot(valid);
        TargetSnapshot movedSnapshot = Snapshot(moved);
        context.Board.Get(moved.Id).MoveTo(
            CardPosition.Base);
        List<Card> affected = [];
        PartialSpellEffectExecutor executor =
            context.CreateExecutor(affected);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem(
                [validSnapshot, movedSnapshot]));

        Assert.True(result.Succeeded);
        Assert.Equal([valid], affected);

        InstructionResolutionRecord record =
            Assert.Single(executor.LastReport!.Instructions);
        Assert.Equal(
            TargetResolutionStatus.Eligible,
            record.Targets[0].Status);
        Assert.Equal(
            TargetResolutionStatus.WrongLocation,
            record.Targets[1].Status);
        Assert.Equal(1, record.Result.AffectedTargetCount);
    }

    [Fact]
    public void Execute_AllTargetsIneligible_StillSucceedsWithNoEffect()
    {
        TestContext context = CreateContext();
        Card first = context.RegisterUnit(
            CardPosition.Base);
        Card second = context.RegisterUnit(
            CardPosition.Base);
        List<Card> affected = [];
        PartialSpellEffectExecutor executor =
            context.CreateExecutor(affected);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem(
                [
                    Snapshot(first),
                    Snapshot(second)
                ]));

        Assert.True(result.Succeeded);
        Assert.Empty(affected);
        Assert.Equal(
            0,
            Assert.Single(
                executor.LastReport!.Instructions)
                .Result.AffectedTargetCount);
    }

    [Fact]
    public void Execute_TargetGainsDeflect_StillAffectsTarget()
    {
        TestContext context = CreateContext(
            targetIndexes: [0]);
        Card target = context.RegisterUnit(
            CardPosition.Battlefield);
        TargetSnapshot snapshot = Snapshot(target);
        context.Board.Get(target.Id).SetDeflect(true);
        List<Card> affected = [];
        PartialSpellEffectExecutor executor =
            context.CreateExecutor(affected);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem([snapshot]));

        Assert.True(result.Succeeded);
        Assert.Equal([target], affected);
    }

    [Fact]
    public void Execute_MissingInstructionHandler_IsEngineFailure()
    {
        TestContext context = CreateContext();
        Card target = context.RegisterUnit(
            CardPosition.Battlefield);
        PartialSpellEffectExecutor executor = new(
            new TargetResolver(context.Board),
            []);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem([Snapshot(target)]));

        Assert.False(result.Succeeded);
        Assert.Contains(
            "No handler",
            result.FailureReason,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_MissingTargetIndex_IsEngineFailure()
    {
        TestContext context = CreateContext(
            targetIndexes: [1]);
        Card target = context.RegisterUnit(
            CardPosition.Battlefield);
        PartialSpellEffectExecutor executor =
            context.CreateExecutor([]);

        SpellExecutionResult result = executor.Execute(
            context.CreateSpellItem([Snapshot(target)]));

        Assert.False(result.Succeeded);
        Assert.Contains(
            "missing target index",
            result.FailureReason,
            StringComparison.OrdinalIgnoreCase);
    }

    private static TargetSnapshot Snapshot(Card card) =>
        new(
            card.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

    private static TestContext CreateContext(
        IReadOnlyList<int>? targetIndexes = null) =>
        new(targetIndexes ?? [0, 1]);

    private sealed class TestContext
    {
        private readonly IReadOnlyList<int> _targetIndexes;

        public TestContext(
            IReadOnlyList<int> targetIndexes)
        {
            _targetIndexes = targetIndexes;
        }

        public BoardState Board { get; } = new();

        public Card RegisterUnit(CardPosition position)
        {
            Card card = Card.Create(
                new CardDefinition(
                    Guid.NewGuid().ToString("N"),
                    "Unit",
                    CardType.Unit,
                    ResourceCost.EnergyOnly(0)),
                PlayerId.New());

            Board.Register(card, position);
            return card;
        }

        public PartialSpellEffectExecutor CreateExecutor(
            List<Card> affected) =>
            new(
                new TargetResolver(Board),
                [
                    new AffectEligibleTargetsHandler(
                        "affect",
                        target =>
                        {
                            if (target.Target is not null)
                            {
                                affected.Add(target.Target);
                            }
                        })
                ]);

        public PlayCardChainItem CreateSpellItem(
            IReadOnlyList<TargetSnapshot> targets)
        {
            Card spell = Card.Create(
                new CardDefinition(
                    "partial-spell",
                    "Partial Spell",
                    CardType.Spell,
                    ResourceCost.EnergyOnly(0),
                    [
                        new SpellInstructionDefinition(
                            "affect",
                            _targetIndexes)
                    ]),
                PlayerId.New());

            return PlayCardChainItem.Create(
                spell.OwnerId,
                spell,
                new ResourcePayment(0),
                targets);
        }
    }
}
