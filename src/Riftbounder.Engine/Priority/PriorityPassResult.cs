using Riftbounder.Core.Identifiers;
using Riftbounder.Engine.Chains;

namespace Riftbounder.Engine.Priority;

public sealed record PriorityPassResult(
    PriorityPassOutcome Outcome,
    PlayerId? PriorityPlayerId,
    ResolvedChainItem? Resolution);
