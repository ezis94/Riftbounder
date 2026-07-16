using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
using Riftbounder.Engine.Actions;
using Riftbounder.Engine.Chains;
using Riftbounder.Engine.Games;
using Riftbounder.Engine.Priority;

namespace Riftbounder.Engine.Tests;

public sealed class PlayCardServiceTests
{
    [Fact]
    public void Play_ValidSpell_RemovesCardPaysCostAndAddsChainItem()
    {
        TestContext context = CreateContext();
        Card spell = PutSpellInHand(
            context,
            "Test Spell",
            new ResourceCost(
                2,
                [PowerRequirement.Of(Domain.Mind, 1)]));
        context.First.RunePool.AddEnergy(2);
        PowerType mind = PowerType.ForDomain(Domain.Mind);
        context.First.RunePool.AddPower(mind, 1);

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(
                    2,
                    [new PowerSpend(mind, 1)])));

        Assert.True(result.Succeeded);
        Assert.Empty(context.First.Hand.Cards);
        Assert.Equal(0, context.First.RunePool.Energy);
        Assert.Equal(0, context.First.RunePool.GetPower(mind));
        Assert.Single(context.Chain.Items);
        Assert.Same(spell, result.ChainItem?.Card);
        Assert.Equal(
            context.First.Id,
            context.Priority.PriorityPlayerId);
    }

    [Fact]
    public void Play_WhenChainActive_AddsNewestItemAndResetsPriority()
    {
        TestContext context = CreateContext();
        TestChainItem existing = TestChainItem.Create(context.Second.Id);
        context.Chain.Push(existing);
        context.Priority.Begin();

        Card spell = PutSpellInHand(
            context,
            "Reaction",
            ResourceCost.EnergyOnly(0));

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(0)));

        Assert.True(result.Succeeded);
        Assert.Equal(2, context.Chain.Count);
        Assert.Same(result.ChainItem, context.Chain.Peek());
        Assert.Equal(
            context.First.Id,
            context.Priority.PriorityPlayerId);
        Assert.Empty(context.Priority.PlayersWhoPassed);
    }

    [Fact]
    public void Play_CardNotInHand_FailsWithoutMutation()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(
            context.First.Id,
            "Missing",
            ResourceCost.EnergyOnly(0));

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(0)));

        Assert.False(result.Succeeded);
        Assert.Equal(
            PlayCardFailure.CardNotInHand,
            result.Failure);
        Assert.True(context.Chain.IsEmpty);
    }

    [Fact]
    public void Play_WrongOwner_FailsWithoutMutation()
    {
        TestContext context = CreateContext();
        Card spell = CreateSpell(
            context.Second.Id,
            "Borrowed",
            ResourceCost.EnergyOnly(0));
        context.Game.RegisterCard(
            spell,
            context.First.Hand);

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(0)));

        Assert.Equal(
            PlayCardFailure.CardNotOwnedByPlayer,
            result.Failure);
        Assert.Single(context.First.Hand.Cards);
        Assert.True(context.Chain.IsEmpty);
    }

    [Fact]
    public void Play_UnsupportedPermanent_FailsWithoutMutation()
    {
        TestContext context = CreateContext();
        Card unit = Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            context.First.Id);
        context.Game.RegisterCard(
            unit,
            context.First.Hand);

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                unit.Id,
                new ResourcePayment(0)));

        Assert.Equal(
            PlayCardFailure.UnsupportedCardType,
            result.Failure);
        Assert.Single(context.First.Hand.Cards);
    }

    [Fact]
    public void Play_InvalidPayment_FailsWithoutSpendingResources()
    {
        TestContext context = CreateContext();
        Card spell = PutSpellInHand(
            context,
            "Expensive",
            ResourceCost.EnergyOnly(2));
        context.First.RunePool.AddEnergy(1);

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(2)));

        Assert.Equal(
            PlayCardFailure.InvalidPayment,
            result.Failure);
        Assert.Equal(1, context.First.RunePool.Energy);
        Assert.Single(context.First.Hand.Cards);
        Assert.True(context.Chain.IsEmpty);
    }

    [Fact]
    public void Play_PlayerWithoutPermission_FailsWithoutMutation()
    {
        TestContext context = CreateContext(
            new DenyAllPlayPermission());
        Card spell = PutSpellInHand(
            context,
            "Denied",
            ResourceCost.EnergyOnly(0));

        PlayCardResult result = context.Service.Play(
            new PlayCardRequest(
                context.First.Id,
                spell.Id,
                new ResourcePayment(0)));

        Assert.Equal(
            PlayCardFailure.PlayerNotPermitted,
            result.Failure);
        Assert.Single(context.First.Hand.Cards);
        Assert.True(context.Chain.IsEmpty);
    }

    [Fact]
    public void Play_ChainPriorityPermission_AllowsOnlyPriorityPlayer()
    {
        TestContext context = CreateContext();
        TestChainItem existing = TestChainItem.Create(context.Second.Id);
        context.Chain.Push(existing);
        context.Priority.Begin();

        Card firstSpell = PutSpellInHand(
            context,
            "First Spell",
            ResourceCost.EnergyOnly(0));
        Card secondSpell = Card.Create(
            new CardDefinition(
                "second",
                "Second Spell",
                CardType.Spell,
                ResourceCost.EnergyOnly(0)),
            context.Second.Id);
        context.Game.RegisterCard(
            secondSpell,
            context.Second.Hand);

        PlayCardService strictService = new(
            context.Game,
            context.Chain,
            context.Priority,
            new ChainPriorityPlayPermission(
                context.Priority));

        PlayCardResult denied = strictService.Play(
            new PlayCardRequest(
                context.First.Id,
                firstSpell.Id,
                new ResourcePayment(0)));
        PlayCardResult allowed = strictService.Play(
            new PlayCardRequest(
                context.Second.Id,
                secondSpell.Id,
                new ResourcePayment(0)));

        Assert.Equal(
            PlayCardFailure.PlayerNotPermitted,
            denied.Failure);
        Assert.True(allowed.Succeeded);
    }

    [Fact]
    public void Play_NullRequest_Throws()
    {
        TestContext context = CreateContext();

        Assert.Throws<ArgumentNullException>(() =>
            context.Service.Play(null!));
    }

    private static Card PutSpellInHand(
        TestContext context,
        string name,
        ResourceCost cost)
    {
        Card card = CreateSpell(
            context.First.Id,
            name,
            cost);
        context.Game.RegisterCard(
            card,
            context.First.Hand);
        return card;
    }

    private static Card CreateSpell(
        PlayerId ownerId,
        string name,
        ResourceCost cost) =>
        Card.Create(
            new CardDefinition(
                name.ToLowerInvariant().Replace(' ', '-'),
                name,
                CardType.Spell,
                cost),
            ownerId);

    private static TestContext CreateContext(
        IPlayCardPermission? permission = null)
    {
        Player first = new(PlayerId.New(), "First");
        Player second = new(PlayerId.New(), "Second");
        Game game = new(first, second);
        Chain chain = new();
        ChainResolver resolver = new(
            chain,
            new SuccessfulResolver());
        ChainPriorityManager priority = new(
            chain,
            resolver,
            [first.Id, second.Id]);

        IPlayCardPermission effectivePermission =
            permission ?? new AllowAllPlayPermission();

        PlayCardService service = new(
            game,
            chain,
            priority,
            effectivePermission);

        return new TestContext(
            game,
            first,
            second,
            chain,
            priority,
            service);
    }

    private sealed record TestContext(
        Game Game,
        Player First,
        Player Second,
        Chain Chain,
        ChainPriorityManager Priority,
        PlayCardService Service);

    private sealed record TestChainItem(
        ChainItemId Id,
        PlayerId ControllerId,
        string Description)
        : IChainItem
    {
        public static TestChainItem Create(
            PlayerId controllerId) =>
            new(
                ChainItemId.New(),
                controllerId,
                "Existing item");
    }

    private sealed class SuccessfulResolver :
        IChainItemResolver
    {
        public ChainResolutionResult Resolve(
            IChainItem item) =>
            ChainResolutionResult.Success();
    }

    private sealed class AllowAllPlayPermission :
        IPlayCardPermission
    {
        public bool CanPlay(
            PlayerId playerId,
            Card card) =>
            true;
    }

    private sealed class DenyAllPlayPermission :
        IPlayCardPermission
    {
        public bool CanPlay(
            PlayerId playerId,
            Card card) =>
            false;
    }
}
