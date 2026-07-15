using Riftbounder.Engine.Events;

namespace Riftbounder.Engine.Tests;

public sealed class EventJournalTests
{
    [Fact]
    public void Append_PreservesEventOrder()
    {
        EventJournal journal = new();
        TestEvent first = new("first", DateTimeOffset.UnixEpoch);
        TestEvent second = new("second", DateTimeOffset.UnixEpoch.AddSeconds(1));

        journal.Append(first);
        journal.Append(second);

        Assert.Collection(
            journal.Events,
            gameEvent => Assert.Same(first, gameEvent),
            gameEvent => Assert.Same(second, gameEvent));
    }

    [Fact]
    public void Append_RejectsNull()
    {
        EventJournal journal = new();

        Assert.Throws<ArgumentNullException>(() => journal.Append(null!));
    }

    private sealed record TestEvent(string Name, DateTimeOffset OccurredAt)
        : GameEvent(OccurredAt);
}
