using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Engine.Board;

namespace Riftbounder.Engine.Tests;

public sealed class BoardCardStateReadinessTests
{
    [Fact]
    public void NewBoardCard_DefaultsToReady()
    {
        BoardCardState state = new(
            CreateUnit(),
            CardPosition.Battlefield);

        Assert.True(state.IsReady);
    }

    [Fact]
    public void Exhaust_ReadyCard_ChangesStateAndReturnsTrue()
    {
        BoardCardState state = new(
            CreateUnit(),
            CardPosition.Battlefield);

        bool changed = state.Exhaust();

        Assert.True(changed);
        Assert.False(state.IsReady);
    }

    [Fact]
    public void Exhaust_AlreadyExhaustedCard_ReturnsFalse()
    {
        BoardCardState state = new(
            CreateUnit(),
            CardPosition.Battlefield,
            isReady: false);

        bool changed = state.Exhaust();

        Assert.False(changed);
        Assert.False(state.IsReady);
    }

    [Fact]
    public void Ready_ExhaustedCard_ChangesStateAndReturnsTrue()
    {
        BoardCardState state = new(
            CreateUnit(),
            CardPosition.Battlefield,
            isReady: false);

        bool changed = state.Ready();

        Assert.True(changed);
        Assert.True(state.IsReady);
    }

    [Fact]
    public void Ready_AlreadyReadyCard_ReturnsFalse()
    {
        BoardCardState state = new(
            CreateUnit(),
            CardPosition.Battlefield);

        bool changed = state.Ready();

        Assert.False(changed);
        Assert.True(state.IsReady);
    }

    private static Card CreateUnit() =>
        Card.Create(
            new CardDefinition(
                "unit",
                "Unit",
                CardType.Unit,
                ResourceCost.EnergyOnly(0)),
            PlayerId.New());
}
