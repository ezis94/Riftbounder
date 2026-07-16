
using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
using Riftbounder.Engine.Games;
namespace Riftbounder.Engine.Tests;

public sealed class BasicRuneAbilityTests
{
    [Fact] public void ExhaustRune_AddsEnergy() { var c = Create(); var r = Ready(c, Domain.Mind); c.Game.ExhaustRuneForEnergy(c.Player.Id, r.Id); Assert.False(r.IsReady); Assert.Equal(1, c.Player.RunePool.Energy); }
    [Fact] public void ExhaustedRune_CannotAddEnergy() { var c = Create(); var r = Ready(c, Domain.Mind); r.Exhaust(); Assert.Throws<InvalidOperationException>(() => c.Game.ExhaustRuneForEnergy(c.Player.Id, r.Id)); }
    [Fact] public void RecycleRune_AddsMatchingPowerAndReturnsToBottom() { var c = Create(); var top = Rune.Create(c.Player.Id, Domain.Order); c.Game.RegisterRune(top, c.Player.RuneDeck); var r = Ready(c, Domain.Mind); c.Game.RecycleRuneForPower(c.Player.Id, r.Id); Assert.Same(top, c.Player.RuneDeck.PeekTop()); Assert.Same(r, c.Player.RuneDeck.Runes[0]); Assert.Equal(1, c.Player.RunePool.GetPower(PowerType.ForDomain(Domain.Mind))); }
    [Fact] public void ExhaustedRune_CanRecycleForPower() { var c = Create(); var r = Ready(c, Domain.Order); r.Exhaust(); c.Game.RecycleRuneForPower(c.Player.Id, r.Id); Assert.Equal(1, c.Player.RunePool.GetPower(PowerType.ForDomain(Domain.Order))); }
    private static Rune Ready(Ctx c, Domain d) { var r = Rune.Create(c.Player.Id, d); c.Game.RegisterRune(r, c.Player.RuneDeck); c.Game.ChannelRunes(c.Player.Id, 1); return r; }
    private static Ctx Create() { var a = new Player(PlayerId.New(), "A"); var b = new Player(PlayerId.New(), "B"); return new(new Game(a, b), a); }
    private sealed record Ctx(Game Game, Player Player);
}
