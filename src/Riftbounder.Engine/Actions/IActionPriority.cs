using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Actions;

public interface IActionPriority
{
    bool HasPriority(PlayerId playerId);

    void NotifyActionTaken(PlayerId playerId);
}
