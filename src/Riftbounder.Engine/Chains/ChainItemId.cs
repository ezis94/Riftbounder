namespace Riftbounder.Engine.Chains;

public readonly record struct ChainItemId(Guid Value)
{
    public static ChainItemId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
