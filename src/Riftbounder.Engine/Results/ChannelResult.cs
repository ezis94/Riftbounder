using Riftbounder.Core.Runes;

namespace Riftbounder.Engine.Results;

public sealed class ChannelResult
{
    public ChannelResult(int requestedCount, IReadOnlyList<Rune> channeledRunes)
    {
        if (requestedCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedCount),
                "Requested rune count cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(channeledRunes);

        RequestedCount = requestedCount;
        ChanneledRunes = channeledRunes;
    }

    public int RequestedCount { get; }

    public IReadOnlyList<Rune> ChanneledRunes { get; }

    public int ChanneledCount => ChanneledRunes.Count;

    public bool WasFullySatisfied => ChanneledCount == RequestedCount;
}
