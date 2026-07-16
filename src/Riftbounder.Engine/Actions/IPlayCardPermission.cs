using Riftbounder.Core.Cards;
using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Actions;

public interface IPlayCardPermission
{
    bool CanPlay(PlayerId playerId, Card card);
}
