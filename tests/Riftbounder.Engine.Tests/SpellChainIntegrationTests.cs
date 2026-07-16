using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Events;
using Riftbounder.Engine.Games;

namespace Riftbounder.Engine.Tests;

public sealed class SpellChainIntegrationTests
{
    [Fact]
    public void ResolveTop_Success_RemovesItemAndTrashesSpell()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        Chain chain = new();
        Card spell = CreateSpell(first.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            first.Id,
            spell,
            new ResourcePayment(0));
        chain.Push(item);

        PlayCardChainItemResolver itemResolver = new(
            game,
            new NoOpSpellEffectExecutor(),
            new EventJournal());
        ChainResolver resolver = new(
            chain,
            itemResolver);

        ResolvedChainItem resolution =
            resolver.ResolveTop();

        Assert.True(resolution.Result.Succeeded);
        Assert.True(chain.IsEmpty);
        Assert.Single(first.Trash.Cards);
        Assert.Same(spell, first.Trash.Cards[0]);
    }

    [Fact]
    public void ResolveTop_Failure_LeavesItemAndSpellOnChain()
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        Chain chain = new();
        Card spell = CreateSpell(first.Id);
        PlayCardChainItem item = PlayCardChainItem.Create(
            first.Id,
            spell,
            new ResourcePayment(0));
        chain.Push(item);

        PlayCardChainItemResolver itemResolver = new(
            game,
            new AlwaysFailExecutor(),
            new EventJournal());
        ChainResolver resolver = new(
            chain,
            itemResolver);

        ResolvedChainItem resolution =
            resolver.ResolveTop();

        Assert.False(resolution.Result.Succeeded);
        Assert.Single(chain.Items);
        Assert.Same(item, chain.Peek());
        Assert.Empty(first.Trash.Cards);
        Assert.Null(game.FindZoneContaining(spell.Id));
    }

    private static Card CreateSpell(PlayerId ownerId) =>
        Card.Create(
            new CardDefinition(
                "test-spell",
                "Test Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0)),
            ownerId);

    private sealed class AlwaysFailExecutor :
        ISpellEffectExecutor
    {
        public SpellExecutionResult Execute(
            PlayCardChainItem spell) =>
            SpellExecutionResult.Failure(
                "Configured engine failure.");
    }
}
