using DateTimeOffset = System.DateTimeOffset;

namespace Riftbounder.Engine.Events;

public abstract record GameEvent : IGameEvent
{
    protected GameEvent(DateTimeOffset occurredAt)
    {
        OccurredAt = occurredAt;
    }

    public DateTimeOffset OccurredAt { get; }
}
