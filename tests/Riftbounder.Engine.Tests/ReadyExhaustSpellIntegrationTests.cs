using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class ReadyExhaustSpellIntegrationTests
{
    [Fact]
    public void Resolve_ExhaustSpell_ExhaustsUnitAndTrashesSpell()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        BoardState board = new();

        Card unit = Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            second.Id);
        board.Register(
            unit,
            CardPosition.Battlefield);

        TargetSnapshot target = new(
            unit.Id,
            new TargetRequirement(
                TargetKind.Unit,
                mustBeAtBattlefield: true));

        Card spell = Card.Create(
            new CardDefinition(
                "exhaust-spell",
                "Exhaust Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0),
                [
                    new SpellInstructionDefinition(
                        ExhaustInstructionHandler.Id,
                        [0])
                ]),
            first.Id);

        PlayCardChainItem item =
            PlayCardChainItem.Create(
                first.Id,
                spell,
                new ResourcePayment(0),
                [target]);

        PartialSpellEffectExecutor executor = new(
            new TargetResolver(board),
            [new ExhaustInstructionHandler(board)]);

        PlayCardChainItemResolver resolver = new(
            game,
            executor,
            new EventJournal());

        var result = resolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.False(board.Get(unit.Id).IsReady);
        Assert.Single(first.Trash.Cards);
        Assert.Same(spell, first.Trash.Cards[0]);
    }
}
