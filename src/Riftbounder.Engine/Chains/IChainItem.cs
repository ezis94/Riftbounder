using Riftbounder.Core.Identifiers;

namespace Riftbounder.Engine.Chains;

public interface IChainItem
{
    ChainItemId Id { get; }

    PlayerId ControllerId { get; }

    string Description { get; }
}
