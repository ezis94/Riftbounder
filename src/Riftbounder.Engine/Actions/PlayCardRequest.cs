using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;
using Riftbounder.Core.Targets;

namespace Riftbounder.Engine.Actions;

public sealed record PlayCardRequest(
    PlayerId PlayerId,
    CardId CardId,
    ResourcePayment Payment,
    IReadOnlyList<TargetSnapshot>? Targets = null);
