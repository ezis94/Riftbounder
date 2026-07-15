namespace Riftbounder.Engine.Events;

public sealed class EventJournal
{
    private readonly List<IGameEvent> _events = [];

    public IReadOnlyList<IGameEvent> Events => _events;

    public void Append(IGameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);
        _events.Add(gameEvent);
    }
}
