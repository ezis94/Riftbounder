using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class PartialSpellResolutionIntegrationTests
{
    [Fact]
    public void Resolve_SomeTargetsInvalid_SpellStillMovesToTrash()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        BoardState board = new();

        Card valid = CreateUnit(first.Id);
        Card recalled = CreateUnit(second.Id);
        board.Register(valid, CardPosition.Battlefield);
        board.Register(recalled, CardPosition.Battlefield);

        TargetSnapshot validTarget = Snapshot(valid);
        TargetSnapshot recalledTarget = Snapshot(recalled);
        board.Get(recalled.Id).MoveTo(CardPosition.Base);

        Card spell = Card.Create(
            new CardDefinition(
                "two-target-spell",
                "Two Target Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0),
                [
                    new SpellInstructionDefinition(
                        "affect",
                        [0, 1])
                ]),
            first.Id);

        PlayCardChainItem item = PlayCardChainItem.Create(
            first.Id,
            spell,
            new ResourcePayment(0),
            [validTarget, recalledTarget]);

        List<Card> affected = [];
        PartialSpellEffectExecutor effectExecutor = new(
            new TargetResolver(board),
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

        PlayCardChainItemResolver itemResolver = new(
            game,
            effectExecutor,
            new EventJournal());

        ChainResolutionResult result =
            itemResolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.Equal([valid], affected);
        Assert.Single(first.Trash.Cards);
        Assert.Same(spell, first.Trash.Cards[0]);
    }

    private static TargetSnapshot Snapshot(Card card) =>
        new(
            card.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

    private static Card CreateUnit(PlayerId ownerId) =>
        Card.Create(
            new CardDefinition(
                Guid.NewGuid().ToString("N"),
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            ownerId);
}
