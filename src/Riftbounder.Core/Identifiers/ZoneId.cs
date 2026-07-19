namespace Riftbounder.Core.Identifiers;

public readonly record struct ZoneId(Guid Value)
{
    public static ZoneId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
