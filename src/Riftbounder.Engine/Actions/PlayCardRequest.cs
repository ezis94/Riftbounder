using Riftbounder.Core.Identifiers;
using Riftbounder.Core.Resources;

namespace Riftbounder.Engine.Actions;

public sealed record PlayCardRequest(
    PlayerId PlayerId,
    CardId CardId,
    ResourcePayment Payment);
