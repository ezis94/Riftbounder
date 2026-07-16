using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Runes;

namespace Riftbounder.Core.Tests;

public sealed class RuneTests
{
    [Fact]
    public void NewRune_StartsExhaustedUntilPutOnBoard()
    {
        Rune rune = Rune.Create(PlayerId.New(), Domain.Mind);

        Assert.False(rune.IsReady);
    }

    [Fact]
    public void ReadyAndExhaust_UpdateRuneState()
    {
        Rune rune = Rune.Create(PlayerId.New(), Domain.Order);

        rune.Ready();
        Assert.True(rune.IsReady);

        rune.Exhaust();
        Assert.False(rune.IsReady);
    }
}
