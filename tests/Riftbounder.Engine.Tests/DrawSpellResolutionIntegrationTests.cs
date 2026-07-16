using Riftbounder.Core.Cards;
using Riftbounder.Core.Effects;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Board;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Effects;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Tests;

public sealed class DrawSpellResolutionIntegrationTests
{
    [Fact]
    public void Resolve_DrawSpell_DrawsThenMovesSpellToTrash()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        Card drawnCard = Card.Create("drawn", first.Id);
        game.RegisterCard(drawnCard, first.MainDeck);

        Card spell = Card.Create(
            new CardDefinition(
                "draw-one",
                "Draw One",
                CardType.Spell,
                ResourceCost.EnergyOnly(0),
                [
                    new SpellInstructionDefinition(
                        DrawCardsInstructionHandler.Id,
                        [],
                        1)
                ]),
            first.Id);

        PlayCardChainItem item = PlayCardChainItem.Create(
            first.Id,
            spell,
            new ResourcePayment(0));

        EventJournal journal = new();
        PartialSpellEffectExecutor effectExecutor = new(
            new TargetResolver(new BoardState()),
            [new DrawCardsInstructionHandler(game, journal)]);
        PlayCardChainItemResolver resolver = new(
            game,
            effectExecutor,
            journal);

        var result = resolver.Resolve(item);

        Assert.True(result.Succeeded);
        Assert.Single(first.Hand.Cards);
        Assert.Same(drawnCard, first.Hand.Cards[0]);
        Assert.Single(first.Trash.Cards);
        Assert.Same(spell, first.Trash.Cards[0]);
    }
}
