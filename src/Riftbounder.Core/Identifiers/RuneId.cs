namespace Riftbounder.Core.Identifiers;

public readonly record struct RuneId(Guid Value)
{
    public static RuneId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
