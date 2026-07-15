namespace Riftbounder.Core.Identifiers;

public readonly record struct PlayerId(Guid Value)
{
    public static PlayerId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
