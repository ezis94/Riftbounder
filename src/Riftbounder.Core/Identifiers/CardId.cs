namespace Riftbounder.Core.Identifiers;

public readonly record struct CardId(Guid Value)
{
    public static CardId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
