using Riftbounder.Core.Identifiers;

namespace Riftbounder.Core.Runes;

public sealed class Rune
{
    public Rune(RuneId id, PlayerId ownerId, Domain domain)
    {
        Id = id;
        OwnerId = ownerId;
        Domain = domain;
    }

    public RuneId Id { get; }

    public PlayerId OwnerId { get; }

    public Domain Domain { get; }

    public bool IsReady { get; private set; }

    public static Rune Create(PlayerId ownerId, Domain domain) =>
        new(RuneId.New(), ownerId, domain);

    public void Ready() => IsReady = true;

    public void Exhaust() => IsReady = false;
}
