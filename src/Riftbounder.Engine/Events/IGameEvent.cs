using DateTimeOffset = System.DateTimeOffset;

namespace Riftbounder.Engine.Events;

public interface IGameEvent
{
    DateTimeOffset OccurredAt { get; }
}
